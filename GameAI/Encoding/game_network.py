"""
7 Wonders AI - Hierarchical Policy Network for Training
================================================

Definiuje hierarchiczny model sieci neuronowej:
- Policy Head (120 akcji głównych)
- Value Head (scalar)
- Subdecision Heads (10, 73, 73 dla różnych typów decyzji)

Wejście:
  - State: ~390 floatów (pełny stan gry)
  - Action Mask: 120 floatów (0/1 dla legalnych akcji)

Wyjście:
  - Policy logits: 120 floatów (logits dla softmax)
  - Value: 1 float (ocena pozycji)
  - Subdecision logits: zmienny rozmiar w zależności od typu decyzji
"""

import torch
import torch.nn as nn
import torch.optim as optim
import numpy as np
from typing import Dict, Tuple, Optional
from dataclasses import dataclass


@dataclass
class ActionSpaceDim:
    """Stałe wymiary ACTION SPACE - muszą pasować do C#"""
    BOARD_SLOTS = 20
    ACTIONS_PER_SLOT = 6
    TOTAL_PRIMARY_ACTIONS = 120  # 20 * 6
    
    NUM_PROGRESS_TOKENS = 10
    NUM_CARDS = 73  # 23 + 23 + 27
    NUM_WONDERS = 12


class HierarchicalPolicyNetwork(nn.Module):
    """
    Hierarchiczny model policy + value dla 7 Wonders AI.
    
    Architektura:
    1. Encoder stanu (shared backbone)
    2. Policy head: 120 działań
    3. Value head: ocena pozycji
    4. Subdecision heads: dla każdego typu decyzji
    """
    
    def __init__(self, state_dim: int = 390, hidden_dim: int = 256):
        super().__init__()
        
        self.state_dim = state_dim
        self.hidden_dim = hidden_dim
        
        # Shared backbone
        self.encoder = nn.Sequential(
            nn.Linear(state_dim, hidden_dim * 2),
            nn.ReLU(),
            nn.LayerNorm(hidden_dim * 2),
            nn.Linear(hidden_dim * 2, hidden_dim),
            nn.ReLU(),
            nn.LayerNorm(hidden_dim),
        )
        
        # Policy Head (główne akcje)
        self.policy_head = nn.Sequential(
            nn.Linear(hidden_dim, hidden_dim),
            nn.ReLU(),
            nn.Linear(hidden_dim, ActionSpaceDim.TOTAL_PRIMARY_ACTIONS)  # 120
        )
        
        # Value Head
        self.value_head = nn.Sequential(
            nn.Linear(hidden_dim, hidden_dim // 2),
            nn.ReLU(),
            nn.Linear(hidden_dim // 2, 1)
        )
        
        # Subdecision Heads (dla każdego typu decyzji)
        self.subdecision_heads = nn.ModuleDict({
            "tokens": self._make_subdecision_head(
                hidden_dim, ActionSpaceDim.NUM_PROGRESS_TOKENS),
            "cards": self._make_subdecision_head(
                hidden_dim, ActionSpaceDim.NUM_CARDS),
            "discarded_cards": self._make_subdecision_head(
                hidden_dim, ActionSpaceDim.NUM_CARDS),
        })
    
    def _make_subdecision_head(self, hidden_dim: int, output_dim: int) -> nn.Sequential:
        return nn.Sequential(
            nn.Linear(hidden_dim, hidden_dim // 2),
            nn.ReLU(),
            nn.Linear(hidden_dim // 2, output_dim)
        )
    
    def forward(self, state: torch.Tensor, 
                action_mask: Optional[torch.Tensor] = None,
                subdecision_type: Optional[str] = None) -> Dict[str, torch.Tensor]:
        """
        Forward pass.
        
        Args:
            state: [batch, state_dim]
            action_mask: [batch, 120] - maska legalnych akcji (0/1)
            subdecision_type: "tokens", "cards", "discarded_cards" lub None
        
        Returns:
            Dict z:
            - policy_logits: [batch, 120]
            - policy_masked_logits: [batch, 120] (z -inf dla nielegalnych)
            - value: [batch, 1]
            - subdecision_logits: [batch, output_dim] (jeśli subdecision_type)
        """
        
        # Shared encoding
        encoded = self.encoder(state)  # [batch, hidden_dim]
        
        # Policy
        policy_logits = self.policy_head(encoded)  # [batch, 120]
        
        # Mask illegal actions
        policy_masked_logits = policy_logits.clone()
        if action_mask is not None:
            # action_mask = 0 -> illegal, set to -inf
            policy_masked_logits = policy_logits + (action_mask - 1) * 1e9
        
        # Value
        value = self.value_head(encoded)  # [batch, 1]
        
        result = {
            "policy_logits": policy_logits,
            "policy_masked_logits": policy_masked_logits,
            "value": value,
        }
        
        # Subdecision (jeśli typ jest podany)
        if subdecision_type and subdecision_type in self.subdecision_heads:
            subdecision_logits = self.subdecision_heads[subdecision_type](encoded)
            result["subdecision_logits"] = subdecision_logits
        
        return result
    
    def compute_policy_loss(self, policy_logits: torch.Tensor,
                            action_indices: torch.Tensor,
                            action_mask: torch.Tensor,
                            advantages: torch.Tensor) -> torch.Tensor:
        """
        Oblicza policy loss (masked cross-entropy).
        
        Args:
            policy_logits: [batch, 120]
            action_indices: [batch] - indeksy wybranych akcji
            action_mask: [batch, 120] - maska legalnych akcji
            advantages: [batch] - advantage estimates
        
        Returns:
            Policy loss (scalar)
        """
        
        # Soft mask: przekształć logity z uwzględnieniem maski
        masked_logits = policy_logits.clone()
        masked_logits[action_mask == 0] = -1e9
        
        # Softmax
        log_probs = torch.nn.functional.log_softmax(masked_logits, dim=1)
        
        # Zbierz log-probs dla wybranych akcji
        action_log_probs = log_probs.gather(1, action_indices.unsqueeze(1)).squeeze(1)
        
        # Policy loss = -mean(log_prob * advantage)
        policy_loss = -(action_log_probs * advantages.detach()).mean()
        
        return policy_loss
    
    def compute_value_loss(self, value_pred: torch.Tensor,
                          value_target: torch.Tensor) -> torch.Tensor:
        """Value loss (MSE)"""
        return nn.MSELoss()(value_pred, value_target)
    
    def compute_subdecision_loss(self, subdecision_logits: torch.Tensor,
                                subdecision_indices: torch.Tensor,
                                subdecision_mask: torch.Tensor) -> torch.Tensor:
        """
        Subdecision loss (masked cross-entropy).
        
        Args:
            subdecision_logits: [batch, dim]
            subdecision_indices: [batch] - indeksy wybranych opcji
            subdecision_mask: [batch, dim] - maska legalnych opcji
        """
        
        masked_logits = subdecision_logits.clone()
        masked_logits[subdecision_mask == 0] = -1e9
        
        log_probs = torch.nn.functional.log_softmax(masked_logits, dim=1)
        action_log_probs = log_probs.gather(1, subdecision_indices.unsqueeze(1)).squeeze(1)
        
        return -action_log_probs.mean()


class GameDataset(torch.utils.data.Dataset):
    """
    Dataset załadowany z JSON logów symulacji z C#.
    Przykład struktury:
    {
        "state": [float; 390],
        "action_mask": [float; 120],
        "action_taken": int,
        "action_logit": float,
        "value_target": float,
        "subdecisions": [
            {"type": "tokens", "options": [...], "legal_mask": [...], "chosen": int}
        ]
    }
    """
    
    def __init__(self, json_log_file: str):
        import json
        with open(json_log_file, 'r') as f:
            self.episodes = json.load(f)
        
        self.data = []
        for episode in self.episodes:
            for step in episode.get("steps", []):
                self.data.append(step)
    
    def __len__(self) -> int:
        return len(self.data)
    
    def __getitem__(self, idx: int) -> Dict:
        step = self.data[idx]
        
        return {
            "state": torch.tensor(step["state"], dtype=torch.float32),
            "action_mask": torch.tensor(step["action_mask"], dtype=torch.float32),
            "action_taken": torch.tensor(step["action_taken"], dtype=torch.long),
            "value_target": torch.tensor(step["value_target"], dtype=torch.float32),
            "advantage": torch.tensor(step.get("advantage", 0.0), dtype=torch.float32),
            "subdecision": step.get("subdecision", None),  # Dict lub None
        }


def train_epoch(model: HierarchicalPolicyNetwork,
                dataloader: torch.utils.data.DataLoader,
                optimizer: optim.Optimizer,
                device: str = "cpu") -> Dict[str, float]:
    """
    Trening na jednym epochs.
    """
    
    model.train()
    total_losses = {"policy": 0, "value": 0, "subdecision": 0}
    num_steps = 0
    
    for batch in dataloader:
        optimizer.zero_grad()
        
        # Move to device
        state = batch["state"].to(device)
        action_mask = batch["action_mask"].to(device)
        action_taken = batch["action_taken"].to(device)
        value_target = batch["value_target"].to(device).unsqueeze(1)
        advantage = batch["advantage"].to(device)
        
        # Forward
        output = model(state, action_mask=action_mask)
        
        # Policy loss
        policy_loss = model.compute_policy_loss(
            output["policy_logits"],
            action_taken,
            action_mask,
            advantage
        )
        
        # Value loss
        value_loss = model.compute_value_loss(output["value"], value_target)
        
        # Total loss
        total_loss = policy_loss + 0.5 * value_loss
        
        total_loss.backward()
        optimizer.step()
        
        # Accumulate
        total_losses["policy"] += policy_loss.item()
        total_losses["value"] += value_loss.item()
        num_steps += 1
    
    return {k: v / num_steps for k, v in total_losses.items()}


def export_to_onnx(model: HierarchicalPolicyNetwork,
                   output_path: str,
                   state_dim: int = 390):
    """
    Eksportuje model do ONNX do użytku w C# (ONNX Runtime).
    
    ONNX Runtime musi być zainstalowany w C# projekcie:
    `dotnet add package Microsoft.ML.OnnxRuntime`
    """
    
    model.eval()
    
    # Dummy inputs
    dummy_state = torch.randn(1, state_dim)
    dummy_action_mask = torch.ones(1, ActionSpaceDim.TOTAL_PRIMARY_ACTIONS)
    
    # Trace
    traced = torch.jit.trace(
        model,
        (dummy_state, dummy_action_mask)
    )
    
    # Export to ONNX
    torch.onnx.export(
        traced,
        (dummy_state, dummy_action_mask),
        output_path,
        input_names=["state", "action_mask"],
        output_names=["policy_logits", "policy_masked_logits", "value"],
        opset_version=12,
        verbose=False
    )
    
    print(f"✓ Model exported to {output_path}")


if __name__ == "__main__":
    # Przykład użycia
    device = "cuda" if torch.cuda.is_available() else "cpu"
    
    # Model
    model = HierarchicalPolicyNetwork(state_dim=390, hidden_dim=256)
    model = model.to(device)
    
    # Optimizer
    optimizer = optim.Adam(model.parameters(), lr=1e-3)
    
    # Training loop (pseudo-code)
    # for epoch in range(10):
    #     dataset = GameDataset("path/to/logs.json")
    #     dataloader = torch.utils.data.DataLoader(dataset, batch_size=32)
    #     losses = train_epoch(model, dataloader, optimizer, device)
    #     print(f"Epoch {epoch}: {losses}")
    
    # Export
    export_to_onnx(model, "7wonders_policy.onnx")
