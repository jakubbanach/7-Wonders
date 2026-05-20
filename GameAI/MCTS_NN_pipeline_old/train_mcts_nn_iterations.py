"""
Minimal MCTS+NN training loop for 7 Wonders.

Flow per iteration:
1. Run self-play in C# using the current ONNX model.
2. Load the generated .npz training data.
3. Train a small policy+value network on policy/value targets.
4. Export the next ONNX model and keep a checkpoint for the next round.

This script intentionally reuses the existing C# self-play path and the
existing PyTorch model definition from game_training_pipeline_fixed.py.
"""

from __future__ import annotations

import argparse
import json
import shutil
import subprocess
import sys
from pathlib import Path
from typing import Dict, Tuple

import numpy as np
import torch
from torch.utils.data import DataLoader, Dataset


def _repo_root() -> Path:
    return Path(__file__).resolve().parents[2]


def _game_console_project(repo_root: Path) -> Path:
    return repo_root / "GameConsole" / "GameConsole.csproj"


def _results_dir(repo_root: Path) -> Path:
    return repo_root / "GameConsole" / "Results"


def _encoding_dir(repo_root: Path) -> Path:
    return repo_root / "GameAI" / "Encoding"


def _pipeline_dir() -> Path:
    return Path(__file__).resolve().parent


def _default_start_model(repo_root: Path) -> Path:
    return _encoding_dir(repo_root) / "onnx_models" / "policy_network_50_500.onnx"


def _load_pipeline_modules(repo_root: Path):
    encoding_dir = _encoding_dir(repo_root)
    pipeline_dir = _pipeline_dir()

    sys.path.insert(0, str(encoding_dir))
    sys.path.insert(0, str(pipeline_dir))

    import game_state_decoder as decoder_module  # type: ignore
    import game_training_pipeline_fixed as pipeline  # type: ignore

    return decoder_module, pipeline


def _run_self_play(
    repo_root: Path,
    model_path: Path,
    output_prefix: str,
    games: int,
    seed: int,
    minimal_logs: bool,
) -> Path:
    command = [
        "dotnet",
        "run",
        "--project",
        str(_game_console_project(repo_root)),
        "--",
        "self-play-train",
        "--seed",
        str(seed),
        "--games",
        str(games),
        "--model",
        str(model_path),
        "--output",
        output_prefix,
    ]

    command.append("--minimal-logs" if minimal_logs else "--full-logs")

    subprocess.run(command, cwd=repo_root, check=True)

    suffix = "_minimal" if minimal_logs else ""
    return _results_dir(repo_root) / f"{output_prefix}_{games}_games{suffix}.npz"


def _resolve_input_data(repo_root: Path, input_data: Path) -> Path:
    if input_data.is_absolute():
        return input_data

    return (repo_root / input_data).resolve()


def _load_training_data(npz_path: Path, decoder_module) -> Tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray, np.ndarray]:
    decoder = decoder_module.BatchedGameStateDecoder()
    states, actions, masks = decoder.load_and_preprocess(str(npz_path), normalize=False, shuffle=False)

    with np.load(npz_path) as archive:
        policy_key = "targets/policy_targets.npy"
        value_key = "targets/value_targets.npy"

        if policy_key in archive:
            policy_targets = archive[policy_key].astype(np.float32)
        else:
            policy_targets = np.zeros((len(actions) * 120,), dtype=np.float32)
            for i, action in enumerate(actions):
                if 0 <= int(action) < 120:
                    policy_targets[i * 120 + int(action)] = 1.0

        if value_key in archive:
            value_targets = archive[value_key].astype(np.float32)
        else:
            value_targets = np.zeros((len(actions),), dtype=np.float32)

    if policy_targets.ndim == 1:
        policy_targets = policy_targets.reshape(-1, 120)
    if value_targets.ndim == 1:
        value_targets = value_targets.reshape(-1, 1)

    if len(states) != len(policy_targets) or len(states) != len(value_targets):
        raise ValueError(
            f"Dataset size mismatch: states={len(states)}, policy={len(policy_targets)}, value={len(value_targets)}"
        )

    return states, actions, masks, policy_targets, value_targets


def _split_train_val(
    states: np.ndarray,
    masks: np.ndarray,
    policy_targets: np.ndarray,
    value_targets: np.ndarray,
    val_ratio: float = 0.1,
    seed: int = 12345,
):
    rng = np.random.default_rng(seed)
    indices = rng.permutation(len(states))
    split = max(1, int(len(indices) * (1.0 - val_ratio))) if len(indices) > 1 else len(indices)

    train_idx = indices[:split]
    val_idx = indices[split:] if split < len(indices) else indices[:0]

    def select(idx: np.ndarray):
        return (
            torch.from_numpy(states[idx]).float(),
            torch.from_numpy(masks[idx]).float(),
            torch.from_numpy(policy_targets[idx]).float(),
            torch.from_numpy(value_targets[idx]).float(),
        )

    return select(train_idx), select(val_idx)


def _make_loader(tensors, batch_size: int, shuffle: bool) -> DataLoader:
    state, mask, policy, value = tensors

    class DictTensorDataset(Dataset):
        def __len__(self):
            return state.shape[0]

        def __getitem__(self, idx):
            return {
                "state": state[idx],
                "action_mask": mask[idx],
                "policy_target": policy[idx],
                "value_target": value[idx],
            }

    dataset = DictTensorDataset()
    return DataLoader(dataset, batch_size=batch_size, shuffle=shuffle, drop_last=False)


def _train_round(
    pipeline,
    train_loader: DataLoader,
    val_loader: DataLoader,
    device: torch.device,
    epochs: int,
    learning_rate: float,
    value_weight: float,
    checkpoint_path: Path,
):
    model = pipeline.PolicyNetwork().to(device)
    optimizer = torch.optim.Adam(model.parameters(), lr=learning_rate)

    history = []
    best_val = float("inf")
    best_epoch = 0
    best_state = None

    for epoch in range(1, epochs + 1):
        train_metrics = pipeline.train_epoch(model, train_loader, optimizer, device, value_weight=value_weight)
        val_metrics = pipeline.evaluate(model, val_loader, device, value_weight=value_weight) if len(val_loader.dataset) > 0 else train_metrics
        history.append({"epoch": epoch, "train": train_metrics, "val": val_metrics})

        current_val = float(val_metrics["total_loss"])
        if current_val < best_val:
            best_val = current_val
            best_epoch = epoch
            best_state = {k: v.detach().cpu().clone() for k, v in model.state_dict().items()}

        print(
            f"Epoch {epoch:03d}/{epochs:03d} | "
            f"train loss={train_metrics['total_loss']:.4f} | "
            f"val loss={val_metrics['total_loss']:.4f}"
        )

    if best_state is not None:
        model.load_state_dict(best_state)

    checkpoint_path.parent.mkdir(parents=True, exist_ok=True)
    torch.save(model.state_dict(), checkpoint_path)
    return model, history, best_epoch, best_val


def _export_model(model, onnx_path: Path):
    onnx_path.parent.mkdir(parents=True, exist_ok=True)
    model.onnx_export(str(onnx_path), validate=True)


def run_pipeline(
    rounds: int,
    games_per_round: int,
    epochs: int,
    seed: int,
    start_model: Path,
    model_dir: Path,
    output_prefix: str,
    minimal_logs: bool,
    batch_size: int,
    learning_rate: float,
    value_weight: float,
    initial_data: Path | None,
):
    repo_root = _repo_root()
    decoder_module, pipeline = _load_pipeline_modules(repo_root)
    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")

    current_model = start_model
    if not current_model.exists():
        raise FileNotFoundError(f"Start model not found: {current_model}")

    model_dir.mkdir(parents=True, exist_ok=True)
    results_dir = _results_dir(repo_root)
    results_dir.mkdir(parents=True, exist_ok=True)

    round_reports = []
    resolved_initial_data = _resolve_input_data(repo_root, initial_data) if initial_data is not None else None

    for round_index in range(2, rounds + 1):
        round_tag = f"{round_index:03d}"
        iter_prefix = f"{output_prefix}_iter_{round_tag}"
        print(f"\n=== Round {round_tag} ===")
        print(f"Self-play model: {current_model}")

        if round_index == 2 and resolved_initial_data is not None:
            data_path = resolved_initial_data
            print(f"Using initial dataset: {data_path}")
        else:
            data_path = _run_self_play(
                repo_root=repo_root,
                model_path=current_model,
                output_prefix=iter_prefix,
                games=games_per_round,
                seed=seed + round_index + 1000,
                minimal_logs=minimal_logs,
            )
        print(f"Training data: {data_path}")

        states, actions, masks, policy_targets, value_targets = _load_training_data(data_path, decoder_module)
        train_tensors, val_tensors = _split_train_val(
            states,
            masks,
            policy_targets,
            value_targets,
            val_ratio=0.1,
            seed=seed + round_index,
        )

        train_loader = _make_loader(train_tensors, batch_size=batch_size, shuffle=True)
        val_loader = _make_loader(val_tensors, batch_size=batch_size, shuffle=False)

        checkpoint_path = model_dir / f"policy_network_iter_{round_tag}.pt"
        onnx_path = model_dir / f"policy_network_iter_{round_tag}.onnx"
        latest_path = model_dir / "policy_network_latest.onnx"

        model, history, best_epoch, best_val = _train_round(
            pipeline=pipeline,
            train_loader=train_loader,
            val_loader=val_loader,
            device=device,
            epochs=epochs,
            learning_rate=learning_rate,
            value_weight=value_weight,
            checkpoint_path=checkpoint_path,
        )

        model = model.to(device)
        _export_model(model, onnx_path)
        shutil.copy2(onnx_path, latest_path)

        best_tag = f"policy_network_iter_{round_tag}_best.onnx"
        best_path = model_dir / best_tag
        shutil.copy2(onnx_path, best_path)

        report = {
            "round": round_index,
            "data_path": str(data_path),
            "input_model": str(current_model),
            "checkpoint_path": str(checkpoint_path),
            "onnx_path": str(onnx_path),
            "best_onnx_path": str(best_path),
            "latest_path": str(latest_path),
            "best_epoch": best_epoch,
            "best_val_loss": best_val,
            "samples": int(len(states)),
            "history": history,
        }
        round_reports.append(report)

        manifest_path = model_dir / f"policy_network_iter_{round_tag}.json"
        manifest_path.write_text(json.dumps(report, indent=2), encoding="utf-8")
        print(f"Saved checkpoint: {checkpoint_path}")
        print(f"Saved ONNX: {onnx_path}")
        print(f"Best epoch: {best_epoch:03d}, best val loss: {best_val:.4f}")
        print(f"Latest model: {latest_path}")

        current_model = onnx_path

    final_manifest = model_dir / "training_manifest.json"
    final_manifest.write_text(json.dumps({"rounds": round_reports}, indent=2), encoding="utf-8")
    print(f"\nTraining complete. Manifest: {final_manifest}")


def main() -> int:
    repo_root = _repo_root()
    parser = argparse.ArgumentParser(description="Run iterative MCTS+NN self-play and training rounds.")
    parser.add_argument("--rounds", type=int, default=1, help="Number of self-play + training rounds.")
    parser.add_argument("--games", type=int, default=100, help="Number of self-play games per round.")
    parser.add_argument("--epochs", type=int, default=10, help="Training epochs per round.")
    parser.add_argument("--seed", type=int, default=12345, help="Base random seed.")
    parser.add_argument("--start-model", type=Path, default=_default_start_model(repo_root), help="Initial ONNX model.")
    parser.add_argument("--model-dir", type=Path, default=_encoding_dir(repo_root) / "onnx_models" / "mcts_nn_iterations", help="Where to store per-round models.")
    parser.add_argument("--output-prefix", type=str, default="mcts_nn", help="Prefix for generated training files.")
    parser.add_argument("--batch-size", type=int, default=64, help="Training batch size.")
    parser.add_argument("--learning-rate", type=float, default=1e-3, help="Adam learning rate.")
    parser.add_argument("--value-weight", type=float, default=0.5, help="Weight of the value loss.")
    parser.add_argument("--full-logs", action="store_true", help="Keep full logs instead of minimal logs.")
    parser.add_argument("--initial-data", type=Path, default=None, help="Existing .npz file to use for the first iteration instead of running self-play.")

    args = parser.parse_args()

    run_pipeline(
        rounds=args.rounds,
        games_per_round=args.games,
        epochs=args.epochs,
        seed=args.seed,
        start_model=args.start_model,
        model_dir=args.model_dir,
        output_prefix=args.output_prefix,
        minimal_logs=not args.full_logs,
        batch_size=args.batch_size,
        learning_rate=args.learning_rate,
        value_weight=args.value_weight,
        initial_data=args.initial_data,
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
