param(
    [int]$Seed = 10000,
    [int]$Games = 100,
    [int]$Epochs = 50,
    # [string]$ModelPath = "GameAI/Encoding/onnx_models/policy_network_50_200.onnx",
    [string]$ModelPath = "GameAI/Encoding/onnx_models/puct_50_100.onnx",
    [string]$OutputName = "self_play_puct",
    [switch]$MinimalLogs = $true,
    [switch]$RunNotebook,
    [switch]$OpenNotebook
)

$ErrorActionPreference = "Stop"

$repoRoot = $PSScriptRoot
$gameConsoleProject = Join-Path $repoRoot "GameConsole/GameConsole.csproj"
$notebookPath = Join-Path $repoRoot "GameAI/Encoding/train_onnx_workflow.ipynb"
$pythonExe = Join-Path $repoRoot ".venv/Scripts/python.exe"
$resultsDir = Join-Path $repoRoot "GameConsole/Results"

function Resolve-ModelPath {
    param([string]$PathValue)

    if ([System.IO.Path]::IsPathRooted($PathValue)) {
        return [System.IO.Path]::GetFullPath($PathValue)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $repoRoot $PathValue))
}

function Compress-JsonFiles {
    param([string]$Directory, [string]$Pattern)
    
    if (-not (Test-Path $Directory)) {
        return
    }

    $files = @(Get-ChildItem -Path $Directory -Filter $Pattern -File | Sort-Object LastWriteTime -Descending | Select-Object -First 1)
    
    foreach ($file in $files) {
        if ($file.Extension -eq ".json") {
            Write-Host "Compressing $($file.Name)..."
            $gzipPath = $file.FullName + ".gz"
            
            $pythonCompress = @"
import gzip
import shutil
src = r'$($file.FullName)'
dst = r'$gzipPath'
with open(src, 'rb') as f_in:
    with gzip.open(dst, 'wb') as f_out:
        shutil.copyfileobj(f_in, f_out)
print(f'Compressed: {src} -> {dst}')
import os
orig_size = os.path.getsize(src)
comp_size = os.path.getsize(dst)
print(f'Size: {orig_size/1024/1024:.1f} MB -> {comp_size/1024/1024:.1f} MB ({comp_size*100//orig_size}%)')
"@
            $pythonCompress | & $pythonExe -
        }
    }
}

$resolvedModelPath = Resolve-ModelPath -PathValue $ModelPath

if ($RunNotebook) {
    if (-not (Test-Path $pythonExe)) {
        throw "Nie znaleziono interpretera Pythona w: $pythonExe"
    }

    New-Item -ItemType Directory -Force -Path $resultsDir | Out-Null

    $env:WONDERS_SEED = $Seed.ToString()
    $env:WONDERS_GAMES = $Games.ToString()
    $env:WONDERS_EPOCHS = $Epochs.ToString()
    $env:WONDERS_MODEL_PATH = $resolvedModelPath
    $env:WONDERS_OUTPUT_NAME = $OutputName
    $env:WONDERS_MINIMAL_LOGS = if ($MinimalLogs) { "1" } else { "0" }

    Write-Host "Running notebook-based training round..."
    Write-Host "  Seed: $Seed"
    Write-Host "  Games: $Games"
    Write-Host "  Epochs: $Epochs"
    Write-Host "  Model: $resolvedModelPath"

    $notebookScript = @"
from pathlib import Path
import nbformat
from nbclient import NotebookClient
import os

notebook_path = Path(r'$notebookPath')
output_dir = Path(r'$resultsDir')
output_dir.mkdir(parents=True, exist_ok=True)
output_path = output_dir / 'train_onnx_workflow.executed.ipynb'

with notebook_path.open('r', encoding='utf-8') as notebook_file:
    notebook = nbformat.read(notebook_file, as_version=4)

client = NotebookClient(
    notebook,
    timeout=None,
    kernel_name='python3',
    resources={'metadata': {'path': str(notebook_path.parent)}}
)
executed_notebook = client.execute()

with output_path.open('w', encoding='utf-8') as output_file:
    nbformat.write(executed_notebook, output_file)

print(output_path)
"@

    $notebookScript | & $pythonExe -

    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}
else {
    $arguments = @(
        "run",
        "--project", $gameConsoleProject,
        "--",
        "self-play-train",
        "--seed", $Seed,
        "--games", $Games,
        "--model", $resolvedModelPath,
        "--output", $OutputName
    )

    if ($MinimalLogs) {
        $arguments += "--minimal-logs"
    }
    else {
        $arguments += "--full-logs"
    }

    Write-Host "Running self-play training..."
    Write-Host "  Seed: $Seed"
    Write-Host "  Games: $Games"
    Write-Host "  Model: $resolvedModelPath"

    & dotnet @arguments

    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

Write-Host "Compressing training data..."
Compress-JsonFiles -Directory $resultsDir -Pattern "$OutputName*_games*.json"

if ($OpenNotebook) {
    Write-Host "Opening notebook: $notebookPath"
    Start-Process "code" -ArgumentList @($notebookPath)
}

Write-Host "Training round complete."
