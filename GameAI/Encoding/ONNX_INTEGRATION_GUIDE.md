# ONNX Integration Guide - 7 Wonders AI

## Overview

This document describes the complete C#↔Python↔ONNX pipeline for training and deploying the 7 Wonders hierarchical policy network.

## Architecture

```
[Game Simulation (C#)]
        ↓
[GameStateEncoder.EncodePolicy()]  → GamePolicyEncoding (state, actionMask, wonderMask)
        ↓
[TrainingDataExporter] → game_data.json (state, action_mask, action_taken, value_target)
        ↓
[Python/PyTorch] ← game_training_pipeline.py
        ↓
[HierarchicalPolicyNetwork.onnx_export()] → policy_network.onnx
        ↓
[C# ONNX Runtime] ← OnnxAgent (OnnxPolicyModel)
        ↓
[OnnxAgent.WybierzRuch()] → Best action selection
```

## C# Side

### 1. ActionSpace.cs (Constants)

Centralized tensor dimension constants shared between C# and Python:

```csharp
public static class ActionSpace
{
    public const int ActionMaskSize = 120;
    public const int WonderMaskSize = 12;
    public const int ResourceDecisionSize = 5;
    public const int TokenDecisionSize = 10;
    public const int CardDecisionSize = 73;
    // ...
}
```

### 2. GameStateEncoder.cs (Encoding)

Key public methods:

```csharp
public static GamePolicyEncoding EncodePolicy(Gra gra)
    → Returns: state (290-d) + actionMask (120-d) + wonderMask (12-d)

public static DecisionEncoding EncodeDecision(DecyzjaKontekst<T> decyzja)
    → Returns: decisionType + options + legalMask (fixed per type)
```

### 3. OnnxAgent.cs (ONNX Agent)

Implements `IAgent` interface using ONNX Runtime for policy inference:

```csharp
public class OnnxAgent : IAgent
{
    public Ruch WybierzRuch(Gra gra)
    {
        var encoding = GameStateEncoder.EncodePolicy(gra);
        var policyLogits = policyModel.GetPolicyLogits(
            encoding.State, 
            encoding.ActionMask
        );
        var action = SampleFromDistribution(Softmax(policyLogits));
        return DecodeActionToMove(gra, action);
    }
}
```

### 4. OnnxPolicyModel.cs (ONNX Runtime)

Wrapper around ONNX Runtime inference session:

```csharp
public class OnnxPolicyModel : IPolicyModel
{
    public float[] GetPolicyLogits(float[] state, float[] actionMask)
        → Uses ONNX Runtime to execute model
    
    public float GetValueEstimate(float[] state)
        → Returns value head output
}
```

### 5. OnnxInferenceServer.cs (Optional REST API)

For distributed inference:

```csharp
public class OnnxInferenceServer
{
    public InferenceResponse Infer(InferenceRequest request)
        → POST /infer → {policyLogits, valueEstimate, metadata}
}
```

### 6. TrainingDataExporter.cs (Data Export)

Export game trajectories for Python training:

```csharp
public static void ExportToJson(List<MoveLog> moves, Gra gra, string outputPath)
    → Saves: state, action_mask, action_taken, value_target (JSON)
```

## Python Side

### 1. game_training_pipeline.py

Complete training pipeline with CLI:

```bash
# Train from scratch
python game_training_pipeline.py \
    --data_path game_data.json \
    --val_data_path game_data_val.json \
    --epochs 100 \
    --batch_size 32 \
    --lr 1e-3 \
    --output_model policy_network.onnx

# Inspect model
python -c "import onnx; m = onnx.load('policy_network.onnx'); print(m.graph)"
```

### 2. HierarchicalPolicyNetwork Class

PyTorch model with:
- **Shared encoder**: state (290-d) → hidden (256-d)
- **Policy head**: hidden → 120 actions
- **Value head**: hidden → 1 (value estimate)
- **Subdecision heads**: hidden → {5, 10, 73} (per effect type)
- **ONNX export**: with masked policy logits

### 3. GameDataset Class

Loads JSON exported from C#:

```python
dataset = GameDataset('game_data.json', normalize=True)
loader = DataLoader(dataset, batch_size=32, shuffle=True)
```

Expected JSON structure:
```json
[
  {
    "state": [float...290],
    "action_mask": [float...120],
    "action_taken": 42,
    "value_target": 0.75,
    "subdecisions": {}
  },
  ...
]
```

### 4. Training Loop

```python
for epoch in range(num_epochs):
    train_losses = train_epoch(model, train_loader, optimizer, device)
    val_losses = evaluate(model, val_loader, device)
    
    if val_losses['total_loss'] < best_val_loss:
        torch.save(model.state_dict(), 'best.pt')

model.onnx_export('policy_network.onnx')
```

## Tensor Specifications

### State Vector (290-d)
```
1. Player stats (5-d):
   - Wood, Clay, Stone, Glass, Paper (resources)

2. Board state (100-d):
   - 20 slots × 5 card features per slot

3. Epoch cards (70-d):
   - Card encodings for current epoch

4. Progress tokens (10-d):
   - One-hot for available tokens

5. Wonder status (12-d):
   - One-hot for wonders (built/available)

6. Opponents state (73-d):
   - Similar structure for each opponent

Total: 5 + 100 + 70 + 10 + 12 + 73 = 290
```

### Action Mask (120-d)
```
20 board slots × 6 actions per slot:
- Action 0: Build card
- Action 1: Discard card
- Action 2-5: Build wonder (if applicable)

0 = illegal action
1 = legal action
```

### Policy Output (120-d)
```
Same structure as action mask, but logits.
Masked version: illegal actions → -1e9 (for softmax)
```

### Value Output (1-d)
```
Single scalar: expected game outcome [-1, 1]
-1: losing position
0: neutral
+1: winning position
```

## Workflow Examples

### Example 1: Training from Game Logs

**C# side:**
```csharp
var simulation = new SimulationRunner();
var results = simulation.RunMultipleGames(numGames: 1000);

var exporter = new TrainingDataExporter();
foreach (var result in results)
{
    exporter.ExportToJson(result.Moves, result.GameState, 
        $"training_data/game_{result.Id}.json");
}
```

**Python side:**
```bash
# Collect all training files
ls training_data/*.json | wc -l  # 1000 files

# Combine into single dataset (if needed)
python -c "
import json, glob
data = []
for f in glob.glob('training_data/*.json'):
    with open(f) as fp: data.extend(json.load(fp))
with open('game_data.json', 'w') as fp: json.dump(data, fp)
"

# Train
python game_training_pipeline.py \
    --data_path game_data.json \
    --epochs 50 \
    --output_model policy_v1.onnx
```

### Example 2: Deploy Agent

**C# side:**
```csharp
var config = new OnnxAgentConfiguration
{
    ModelPath = "policy_v1.onnx",
    RandomSeed = 42
};

if (config.Validate(out var error))
{
    var agent = config.CreateAgent(new XorShiftRandom(42));
    var tournament = new TournamentRunner();
    tournament.AddAgent(agent);
    var results = tournament.RunTournament();
}
else
{
    Console.WriteLine($"Config error: {error}");
}
```

### Example 3: Monitor Performance

```csharp
var monitor = new OnnxAgentPerformanceMonitor();

for (int i = 0; i < 1000; i++)
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    var move = agent.WybierzRuch(gra);
    sw.Stop();
    monitor.RecordInferenceTime(sw.ElapsedMilliseconds);
}

var stats = monitor.GetStats();
Console.WriteLine($"Avg latency: {stats.AverageLatencyMs:.2f}ms");
Console.WriteLine($"Throughput: {stats.ThroughputPerSecond:.0f} moves/sec");
```

## Dependencies

### C# (NuGet)
```xml
<PackageReference Include="Microsoft.ML.OnnxRuntime" Version="1.17.0" />
```

### Python
```
torch>=2.0
numpy>=1.24
onnx>=1.14
```

Install:
```bash
pip install torch numpy onnx
```

## Troubleshooting

### Issue: "Model file not found" in OnnxAgent

**Solution**: Ensure .onnx file exists and path is absolute:
```csharp
var modelPath = Path.Combine(
    AppDomain.CurrentDomain.BaseDirectory, 
    "policy_network.onnx"
);
var agent = new OnnxAgent(new OnnxPolicyModel(modelPath), random);
```

### Issue: Shape mismatch in ONNX inference

**Cause**: State vector != 290-d or action mask != 120-d

**Solution**: Verify in Python before export:
```python
model = HierarchicalPolicyNetwork(state_dim=290)
state = torch.randn(1, 290)
action_mask = torch.ones(1, 120)
output = model(state, action_mask)
assert output['policy_masked_logits'].shape == (1, 120)
```

### Issue: ONNX Runtime not installed

**C# Solution**:
```bash
dotnet add package Microsoft.ML.OnnxRuntime
```

### Issue: Python training hangs

**Solution**: Check data loading:
```python
dataset = GameDataset('game_data.json')
print(f"Loaded {len(dataset)} samples")
loader = DataLoader(dataset, batch_size=32)
for batch in loader:
    print(batch['state'].shape)  # Should be [batch_size, 290]
    break
```

## Performance Notes

- **C# Inference Latency**: ~5-15ms per forward pass (ONNX Runtime on CPU)
- **Training Speed**: ~100-200 samples/sec on GPU
- **Model Size**: ~2-5 MB (ONNX file)
- **Memory**: ~100-200 MB per agent in memory

## Future Improvements

1. **Multi-head ONNX export**: Include subdecision heads for full hierarchical inference
2. **Quantization**: INT8 ONNX for faster inference (loss: 1-2% accuracy)
3. **Ensemble methods**: Combine multiple models for robustness
4. **Reinforcement learning**: Self-play training instead of supervised learning
5. **Hyperparameter optimization**: Optuna for architecture search

## References

- [ONNX Runtime C#](https://github.com/microsoft/onnxruntime/tree/main/csharp)
- [PyTorch ONNX Export](https://pytorch.org/docs/stable/onnx.html)
- [7 Wonders Game Rules](https://en.wikipedia.org/wiki/7_Wonders)
