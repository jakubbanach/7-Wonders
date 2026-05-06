"""
7 Wonders AI - Complete Training Pipeline
===========================================

Pelna pipeline do:
1. Wczytywania danych z C# JSON
2. Trenowania hierarchicznego modelu policy network
3. Eksportu do ONNX dla C# runtime
4. Walidacji shapow tensorow
"""

import torch
import torch.nn as nn
import torch.optim as optim
from torch.utils.data import Dataset, DataLoader
import numpy as np
import json
from pathlib import Path
from typing import Dict, List, Optional, Tuple
import logging
import argparse
import glob
from datetime import datetime
import onnx

logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)


class ActionSpace:
    """Stale wymiary ACTION SPACE - musza pasowac do C# ActionSpace.cs"""
    STATE_VECTOR_SIZE = 1903
    BOARD_SLOTS = 20
    ACTIONS_PER_SLOT = 6
    TOTAL_PRIMARY_ACTIONS = 120  # 20 * 6
    
    # Rozmiary dla podecyzji
    NUM_RESOURCES = 5  # glina, kamien, drewno, szklo, papirus
    NUM_PROGRESS_TOKENS = 10
    NUM_CARDS = 73  # 23 + 23 + 27
    NUM_WONDERS = 12
    NUM_PLAYERS = 2


class HierarchicalPolicyNetwork(nn.Module):
    """
    Hierarchiczny model policy + value dla 7 Wonders AI.
    
    Architektura:
    1. Encoder stanu (shared backbone): state (1903-d) → hidden (256-d)
    2. Policy head: hidden (256-d) → 120 głownych dzialan
    3. Value head: hidden (256-d) → 1 (ocena pozycji)
    4. Subdecision heads: hidden (256-d) → zmienne rozmiary
    
    Export: ONNX z masked policy logits dla C# runtime
    """

    SUBDECISION_HEADS = {
        'Wylosuj3ZetonyPostepu': ActionSpace.NUM_PROGRESS_TOKENS,
        'WybierzZetonPostepu': ActionSpace.NUM_PROGRESS_TOKENS,
        'OdlozKartePrzeciwnika': ActionSpace.NUM_CARDS,
        'DarmowaBudowlaZOdrzuconychKart': ActionSpace.NUM_CARDS,
        'WybierzGraczaRozpoczynajacegoEpoke': ActionSpace.NUM_PLAYERS,
    }

    def __init__(self, state_dim: int = ActionSpace.STATE_VECTOR_SIZE, hidden_dim: int = 256, dropout: float = 0.1):
        super().__init__()
        self.state_dim = state_dim
        self.hidden_dim = hidden_dim

        # Shared encoder (backbone)
        self.encoder = nn.Sequential(
            nn.Linear(state_dim, 512),
            nn.ReLU(),
            nn.Dropout(dropout),
            nn.Linear(512, hidden_dim),
            nn.ReLU(),
            nn.Dropout(dropout // 2),
        )

        # Policy head: main action selection
        self.policy_head = nn.Sequential(
            nn.Linear(hidden_dim, hidden_dim),
            nn.ReLU(),
            nn.Dropout(dropout // 2),
            nn.Linear(hidden_dim, ActionSpace.TOTAL_PRIMARY_ACTIONS)
        )

        # Value head: state value estimation
        self.value_head = nn.Sequential(
            nn.Linear(hidden_dim, hidden_dim),
            nn.ReLU(),
            nn.Linear(hidden_dim, 1)
        )

        # Subdecision heads: dla każdego typu efektu
        self.subdecision_heads = nn.ModuleDict({
            effect_type: nn.Sequential(
                nn.Linear(hidden_dim, hidden_dim // 2),
                nn.ReLU(),
                nn.Linear(hidden_dim // 2, output_size)
            )
            for effect_type, output_size in self.SUBDECISION_HEADS.items()
        })

    def forward(
        self,
        state: torch.Tensor,
        action_mask: Optional[torch.Tensor] = None,
        effect_type: Optional[str] = None
    ) -> Dict[str, torch.Tensor]:
        """
        Forward pass.
        
        Args:
            state: [batch_size, 1903] - encoded game state
            action_mask: [batch_size, 120] - binary mask (1 = legal, 0 = illegal)
            effect_type: str or None - if specified, compute only that subdecision head
        
        Returns:
            dict with keys:
                - policy_logits: [batch, 120]
                - policy_masked_logits: [batch, 120] (with -inf for illegal actions)
                - value: [batch, 1]
                - subdecisions: dict[str, tensor] (effect_type -> [batch, output_size])
        """
        # Encode state through shared backbone
        hidden = self.encoder(state)

        # Policy head: compute action logits
        policy_logits = self.policy_head(hidden)

        # Apply action mask: illegal actions → -inf
        if action_mask is not None:
            policy_masked_logits = policy_logits.clone()
            # Ensure action_mask is proper shape
            assert action_mask.shape == policy_logits.shape, \
                f"Action mask shape {action_mask.shape} != policy logits shape {policy_logits.shape}"
            policy_masked_logits[action_mask == 0] = -1e9
        else:
            # No mask provided, all actions are legal
            policy_masked_logits = policy_logits

        # Value head: estimate position value
        value = self.value_head(hidden)

        # Subdecision heads
        subdecisions = {}
        if effect_type and effect_type in self.subdecision_heads:
            # Compute only the requested head
            subdecisions[effect_type] = self.subdecision_heads[effect_type](hidden)
        elif effect_type is None:
            # Compute all subdecision heads
            for effect_name, head in self.subdecision_heads.items():
                subdecisions[effect_name] = head(hidden)

        return {
            'policy_logits': policy_logits,
            'policy_masked_logits': policy_masked_logits,
            'value': value,
            'subdecisions': subdecisions,
        }

    def onnx_export(self, output_path: str = 'policy_network.onnx', validate: bool = True):
        """
        Export model to ONNX format for C# ONNX Runtime integration.
        
        Args:
            output_path: path to save .onnx file
            validate: whether to validate exported model
        """
        logger.info(f"Exporting model to ONNX format: {output_path}")

        device = next(self.parameters()).device

        class ExportWrapper(nn.Module):
            def __init__(self, base_model: "HierarchicalPolicyNetwork"):
                super().__init__()
                self.base_model = base_model

            def forward(self, state: torch.Tensor, action_mask: torch.Tensor):
                outputs = self.base_model(state, action_mask=action_mask)
                return outputs['policy_masked_logits'], outputs['value']

        # Create dummy inputs matching expected shapes
        dummy_state = torch.randn(1, self.state_dim, device=device)
        dummy_action_mask = torch.ones(1, ActionSpace.TOTAL_PRIMARY_ACTIONS, device=device)
        export_model = ExportWrapper(self).to(device)

        # Trace and export
        try:
            torch.onnx.export(
                export_model,
                (dummy_state, dummy_action_mask),
                output_path,
                input_names=['state', 'action_mask'],
                output_names=['policy_masked_logits', 'value'],
                opset_version=13,
                do_constant_folding=True,
                verbose=False,
                dynamic_axes={
                    'state': {0: 'batch_size'},
                    'action_mask': {0: 'batch_size'},
                    'policy_masked_logits': {0: 'batch_size'},
                    'value': {0: 'batch_size'},
                }
            )
            logger.info(f"✓ Model exported successfully: {output_path}")

            if validate:
                _validate_onnx_model(output_path)

        except Exception as e:
            logger.error(f"✗ Export failed: {e}")
            raise


def _validate_onnx_model(model_path: str):
    """Validate exported ONNX model structure and shapes."""
    try:
        logger.info(f"Validating ONNX model: {model_path}")
        onnx_model = onnx.load(model_path)
        onnx.checker.check_model(onnx_model)
        
        # Check input/output shapes
        graph = onnx_model.graph
        logger.info(f"  Inputs: {[inp.name for inp in graph.input]}")
        logger.info(f"  Outputs: {[out.name for out in graph.output]}")
        logger.info(f"✓ ONNX model validation passed")
        
    except Exception as e:
        logger.error(f"✗ ONNX validation failed: {e}")
        raise


class GameDataset(Dataset):
    """
    Dataset for loading encoded game states from C# JSON exports.
    
    Expected JSON format:
    [
        {
            "state": [float, ...],  # 1903 values
            "action_mask": [float, ...],  # 120 values (0/1)
            "action_taken": int,  # 0-119
            "value_target": float,  # -1.0 to 1.0
            "subdecisions": {...}  # optional
        },
        ...
    ]
    """

    def __init__(self, json_path: str, normalize: bool = True, validate_shapes: bool = True):
        self.data = []
        self.normalize = normalize
        self.validate_shapes = validate_shapes
        self.state_mean = None
        self.state_std = None

        logger.info(f"Loading dataset from {json_path}")
        source_path = Path(json_path)
        files = []
        if source_path.is_dir():
            files = sorted(source_path.glob('*.json'))
        elif source_path.is_file():
            files = [source_path]
        else:
            files = [Path(path) for path in sorted(glob.glob(json_path))]

        for file_path in files:
            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    payload = json.load(f)
                self._ingest_payload(payload, file_path)
            except Exception as e:
                logger.warning(f"Skipping file {file_path}: {e}")

        logger.info(f"Loaded {len(self.data)} valid samples")

        # Normalize states
        if self.normalize:
            self._normalize()

    def _normalize(self):
        """Normalize state vectors using z-score normalization."""
        states = np.array([d['state'] for d in self.data])
        self.state_mean = states.mean(axis=0, keepdims=True)
        self.state_std = states.std(axis=0, keepdims=True) + 1e-8

        for d in self.data:
            d['state'] = (d['state'] - self.state_mean.flatten()) / self.state_std.flatten()

        logger.info(f"State normalized: mean={self.state_mean.mean():.4f}, std={self.state_std.mean():.4f}")

    def _ingest_payload(self, payload, source_name):
        if isinstance(payload, dict) and 'Moves' in payload:
            self._ingest_match_result(payload, source_name)
            return

        if isinstance(payload, dict) and 'MatchResults' in payload:
            for match_idx, match_result in enumerate(payload.get('MatchResults') or []):
                self._ingest_match_result(match_result, f"{source_name}::MatchResults[{match_idx}]")
            return

        if isinstance(payload, list):
            self._ingest_flat_samples(payload, source_name)
            return

        raise ValueError(f"Unsupported dataset format in {source_name}")

    def _ingest_flat_samples(self, raw_data, source_name):
        for i, sample in enumerate(raw_data):
            self._append_sample(sample, source_name, i, default_value_target=sample.get('value_target', 0.0))

    def _ingest_match_result(self, match_result, source_name):
        winner = match_result.get('Winner')
        for i, move in enumerate(match_result.get('Moves') or []):
            if not move.get('State') or not move.get('ActionMask'):
                continue

            if winner is None:
                value_target = 0.0
            else:
                value_target = 1.0 if move.get('Agent') == winner else -1.0

            self._append_sample(move, source_name, i, default_value_target=value_target)

    def _append_sample(self, sample, source_name, sample_index, default_value_target=0.0):
        try:
            state = np.array(sample['State'] if 'State' in sample else sample['state'], dtype=np.float32)
            action_mask = np.array(sample['ActionMask'] if 'ActionMask' in sample else sample['action_mask'], dtype=np.float32)

            if self.validate_shapes:
                assert state.shape == (ActionSpace.STATE_VECTOR_SIZE,), f"Sample {sample_index} in {source_name}: state shape {state.shape} != ({ActionSpace.STATE_VECTOR_SIZE},)"
                assert action_mask.shape == (ActionSpace.TOTAL_PRIMARY_ACTIONS,), f"Sample {sample_index} in {source_name}: action_mask shape {action_mask.shape} != ({ActionSpace.TOTAL_PRIMARY_ACTIONS},)"

            action_taken = int(sample.get('ActionIndex', sample.get('action_taken', -1)))
            if action_taken < 0:
                return

            self.data.append({
                'state': state,
                'action_mask': action_mask,
                'action_taken': action_taken,
                'value_target': float(sample.get('value_target', default_value_target)),
                'subdecisions': sample.get('Decisions', sample.get('subdecisions', {})),
            })
        except Exception as e:
            logger.warning(f"Skipping sample {sample_index} in {source_name}: {e}")

    def __len__(self):
        return len(self.data)

    def __getitem__(self, idx):
        sample = self.data[idx]
        return {
            'state': torch.from_numpy(sample['state']),
            'action_mask': torch.from_numpy(sample['action_mask']),
            'action_taken': torch.tensor(sample['action_taken'], dtype=torch.long),
            'value_target': torch.tensor([sample['value_target']], dtype=torch.float32),
        }


def train_epoch(
    model: HierarchicalPolicyNetwork,
    dataloader: DataLoader,
    optimizer: optim.Optimizer,
    device: torch.device,
) -> Dict[str, float]:
    """
    Train model for one epoch.
    
    Returns:
        dict with keys: policy_loss, value_loss, total_loss
    """
    model.train()
    total_policy_loss = 0.0
    total_value_loss = 0.0
    total_samples = 0

    for batch_idx, batch in enumerate(dataloader):
        state = batch['state'].to(device)
        action_mask = batch['action_mask'].to(device)
        action_taken = batch['action_taken'].to(device)
        value_target = batch['value_target'].to(device)

        # Forward pass
        outputs = model(state, action_mask=action_mask)
        policy_logits = outputs['policy_masked_logits']
        value_pred = outputs['value']

        # Policy loss: cross-entropy on legal actions
        policy_loss = nn.functional.cross_entropy(policy_logits, action_taken)

        # Value loss: MSE
        value_loss = nn.functional.mse_loss(value_pred, value_target)

        # Combined loss
        total_loss = policy_loss + 0.5 * value_loss

        # Backward pass
        optimizer.zero_grad()
        total_loss.backward()
        torch.nn.utils.clip_grad_norm_(model.parameters(), max_norm=1.0)
        optimizer.step()

        # Accumulate
        batch_size = state.size(0)
        total_policy_loss += policy_loss.item() * batch_size
        total_value_loss += value_loss.item() * batch_size
        total_samples += batch_size

    return {
        'policy_loss': total_policy_loss / max(total_samples, 1),
        'value_loss': total_value_loss / max(total_samples, 1),
        'total_loss': (total_policy_loss + 0.5 * total_value_loss) / max(total_samples, 1),
    }


def evaluate(
    model: HierarchicalPolicyNetwork,
    dataloader: DataLoader,
    device: torch.device,
) -> Dict[str, float]:
    """
    Evaluate model on validation/test set.
    
    Returns:
        dict with keys: policy_loss, value_loss, total_loss
    """
    model.eval()
    total_policy_loss = 0.0
    total_value_loss = 0.0
    total_samples = 0

    with torch.no_grad():
        for batch in dataloader:
            state = batch['state'].to(device)
            action_mask = batch['action_mask'].to(device)
            action_taken = batch['action_taken'].to(device)
            value_target = batch['value_target'].to(device)

            outputs = model(state, action_mask=action_mask)
            policy_logits = outputs['policy_masked_logits']
            value_pred = outputs['value']

            policy_loss = nn.functional.cross_entropy(policy_logits, action_taken)
            value_loss = nn.functional.mse_loss(value_pred, value_target)

            batch_size = state.size(0)
            total_policy_loss += policy_loss.item() * batch_size
            total_value_loss += value_loss.item() * batch_size
            total_samples += batch_size

    return {
        'policy_loss': total_policy_loss / max(total_samples, 1),
        'value_loss': total_value_loss / max(total_samples, 1),
        'total_loss': (total_policy_loss + 0.5 * total_value_loss) / max(total_samples, 1),
    }


def main():
    parser = argparse.ArgumentParser(
        description='Train 7 Wonders Hierarchical Policy Network'
    )
    parser.add_argument('--data_path', type=str, default='game_data.json',
                        help='Path to training data JSON')
    parser.add_argument('--val_data_path', type=str, default=None,
                        help='Path to validation data JSON')
    parser.add_argument('--epochs', type=int, default=100,
                        help='Number of training epochs')
    parser.add_argument('--batch_size', type=int, default=32,
                        help='Batch size')
    parser.add_argument('--lr', type=float, default=1e-3,
                        help='Learning rate')
    parser.add_argument('--output_model', type=str, default='policy_network.onnx',
                        help='Output ONNX model path')
    parser.add_argument('--checkpoint_dir', type=str, default='checkpoints',
                        help='Directory for checkpoints')
    parser.add_argument('--device', type=str, default='cuda' if torch.cuda.is_available() else 'cpu',
                        help='Device (cuda/cpu)')
    args = parser.parse_args()

    logger.info("=" * 70)
    logger.info("7 Wonders - Hierarchical Policy Network Training")
    logger.info("=" * 70)
    logger.info(f"Data: {args.data_path}")
    logger.info(f"Epochs: {args.epochs}, Batch size: {args.batch_size}, LR: {args.lr}")
    logger.info(f"Device: {args.device}")
    logger.info("=" * 70)

    device = torch.device(args.device)
    Path(args.checkpoint_dir).mkdir(exist_ok=True)

    # Load datasets
    logger.info("Loading data...")
    train_dataset = GameDataset(args.data_path, normalize=True)
    train_loader = DataLoader(train_dataset, batch_size=args.batch_size, shuffle=True)

    val_loader = None
    if args.val_data_path and Path(args.val_data_path).exists():
        val_dataset = GameDataset(args.val_data_path, normalize=True)
        val_loader = DataLoader(val_dataset, batch_size=args.batch_size, shuffle=False)
        logger.info(f"Training: {len(train_dataset)}, Validation: {len(val_dataset)}")
    else:
        logger.info(f"Training: {len(train_dataset)}")

    # Create model
    model = HierarchicalPolicyNetwork(state_dim=ActionSpace.STATE_VECTOR_SIZE, hidden_dim=256, dropout=0.1)
    model = model.to(device)
    logger.info(f"Model parameters: {sum(p.numel() for p in model.parameters()):,}")

    # Optimizer
    optimizer = optim.Adam(model.parameters(), lr=args.lr)
    scheduler = optim.lr_scheduler.CosineAnnealingLR(optimizer, T_max=args.epochs)

    # Training loop
    best_val_loss = float('inf')
    logger.info("Starting training...")

    for epoch in range(args.epochs):
        train_loss = train_epoch(model, train_loader, optimizer, device)
        scheduler.step()

        msg = f"Epoch {epoch+1:3d}/{args.epochs} | Train: {train_loss['total_loss']:.4f} " \
              f"(policy: {train_loss['policy_loss']:.4f}, value: {train_loss['value_loss']:.4f})"

        if val_loader:
            val_loss = evaluate(model, val_loader, device)
            msg += f" | Val: {val_loss['total_loss']:.4f}"

            if val_loss['total_loss'] < best_val_loss:
                best_val_loss = val_loss['total_loss']
                checkpoint = Path(args.checkpoint_dir) / f"best_epoch{epoch+1}.pt"
                torch.save(model.state_dict(), checkpoint)
                logger.info(f"  → Saved checkpoint: {checkpoint}")

        if (epoch + 1) % 10 == 0 or epoch == 0:
            logger.info(msg)

    # Export to ONNX
    logger.info("Training complete. Exporting to ONNX...")
    model.onnx_export(args.output_model, validate=True)
    logger.info(f"✓ Model saved to {args.output_model}")


if __name__ == '__main__':
    main()
