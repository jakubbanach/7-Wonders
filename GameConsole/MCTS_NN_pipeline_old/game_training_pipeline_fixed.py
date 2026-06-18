"""
7 Wonders AI - Naprawiony Training Pipeline
============================================

CO ZOSTAŁO NAPRAWIONE I DLACZEGO:

1. NORMALIZACJA (krytyczna poprawka)
   - Problem: Python normalizował stany do treningu, ale C# podawało surowe wartości
     do modelu podczas gry. Model uczył się na zupełnie innych liczbach niż dostawał.
   - Rozwiązanie: Usunięto normalizację z datasetu. Zamiast tego dodano LayerNorm
     jako PIERWSZĄ warstwę sieci — normalizuje się sama, w środku, bez zewnętrznych
     parametrów które trzeba by przekazywać do C#.

2. VALUE TARGET — głośny sygnał (poważna poprawka)
   - Problem: Każdy ruch w grze dostawał +1 jeśli wygrałeś, -1 jeśli przegrałeś.
     Ruch w turze 3 z 50-turnowej gry dostawał tę samą etykietę co ruch decydujący.
     To jakby oceniać każde zdanie w eseju oceną za cały esej.
   - Rozwiązanie: Discount factor gamma=0.98. Ruchy bliżej końca gry mają silniejszy
     sygnał (+1 lub -1), ruchy na początku mają słabszy (np. ±0.6).
     Formuła: value_target[t] = wynik_końcowy * gamma^(T - t)
     gdzie T = liczba ruchów w grze, t = numer ruchu (od 0)

3. FORMAT DANYCH — rozmiar pliku (ważna poprawka)
   - Problem: JSON z 1903 floatami to ~17KB na ruch. 5700 ruchów = ~100MB.
   - Rozwiązanie: Nowy BinaryGameDataset wczytuje pliki .bin eksportowane przez C#.
     Format: State (float32, 1903×4B) + ActionMask (float32, 120×4B) +
             PolicyTarget (float32, 120×4B) + ActionIndex (int32, 4B) +
             ValueTarget (float32, 4B) = ~8.2KB na ruch zamiast ~17KB.
     Dane są spakowane (gzip) więc realnie ~25-30% mniej: ~6KB na ruch.
     Plik 100 gier: ~35MB zamiast ~200MB.

4. ROZMIAR MODELU — uproszczenie (pomocna poprawka)
   - Problem: Sieć 1903→512→256 ma ~1.5M parametrów. Na 5700 próbek to za duże —
     model zapamiętuje dane zamiast się uczyć.
   - Rozwiązanie: Mniejszy backbone 1903→256→128. ~500k parametrów.
     Subdecision heads usunięte — nie są eksportowane do ONNX i tak.

JAK UŻYWAĆ:
   # Stary sposób (JSON):
   dataset = GameDataset("dane.json")

   # Nowy sposób (binarny, 6x mniejsze pliki):
   dataset = BinaryGameDataset("dane.bin")

   # Możesz dalej używać GameDataset jeśli nie chcesz zmieniać C# od razu
"""

import torch
import torch.nn as nn
import torch.optim as optim
from torch.utils.data import Dataset, DataLoader
import numpy as np
import json
import struct
import gzip
from pathlib import Path
from typing import Dict, List, Optional
import logging
import glob
import copy

logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)


class ActionSpace:
    """Stałe wymiary — muszą pasować do C# ActionSpace.cs"""
    STATE_VECTOR_SIZE = 1903
    TOTAL_PRIMARY_ACTIONS = 120


# ---------------------------------------------------------------------------
# Sieć neuronowa — naprawiona wersja
# ---------------------------------------------------------------------------

class PolicyNetwork(nn.Module):
    """
    Sieć policy + value dla 7 Wonders AI.

    Architektura (uproszczona w stosunku do oryginału):
    - Backbone: 1903 → 256 → 128  (LayerNorm na wejściu zamiast zewnętrznej normalizacji)
    - Policy head: 128 → 120
    - Value head: 128 → 1

    Dlaczego LayerNorm na wejściu?
    LayerNorm normalizuje KAŻDY wektor stanu osobno, bez potrzeby pamiętania
    mean/std z danych treningowych. Model sam "wie" jak normalizować — C# nie
    musi robić nic specjalnego przed podaniem danych.
    """

    def __init__(
        self,
        state_dim: int = ActionSpace.STATE_VECTOR_SIZE,
        hidden_dim: int = 128,
        dropout: float = 0.1,
    ):
        super().__init__()
        self.state_dim = state_dim
        self.hidden_dim = hidden_dim

        # LayerNorm na wejściu zastępuje zewnętrzną normalizację
        # Sieć uczy się jak normalizować — C# nie musi wiedzieć o żadnych mean/std
        self.input_norm = nn.LayerNorm(state_dim)

        # Backbone: kompresuje stan do zwięzłej reprezentacji
        # Mniejszy niż oryginał (1903→512→256) bo mamy ~6k próbek, nie miliony
        self.backbone = nn.Sequential(
            nn.Linear(state_dim, 256),
            nn.ReLU(),
            nn.Dropout(dropout),
            nn.Linear(256, hidden_dim),
            nn.ReLU(),
            nn.Dropout(dropout / 2),
        )

        # Policy head: "które ruchy są dobre?" → 120 liczb (logity)
        # Logity → softmax = prawdopodobieństwa ruchów
        self.policy_head = nn.Sequential(
            nn.Linear(hidden_dim, hidden_dim),
            nn.ReLU(),
            nn.Linear(hidden_dim, ActionSpace.TOTAL_PRIMARY_ACTIONS),
        )

        # Value head: "jak dobra jest ta pozycja?" → 1 liczba od -1 do +1
        self.value_head = nn.Sequential(
            nn.Linear(hidden_dim, hidden_dim // 2),
            nn.ReLU(),
            nn.Linear(hidden_dim // 2, 1),
            nn.Tanh(),  # Tanh wymusza zakres [-1, +1] — czytelniejsze dla treningu
        )

    def forward(
        self,
        state: torch.Tensor,
        action_mask: Optional[torch.Tensor] = None,
    ) -> Dict[str, torch.Tensor]:
        """
        Args:
            state:       [batch, 1903] — surowe wartości ze stanu gry
            action_mask: [batch, 120]  — 1 = legalny ruch, 0 = nielegalny

        Returns:
            policy_masked_logits: [batch, 120] — nielegalne akcje mają -1e9
            value:                [batch, 1]   — ocena pozycji, zakres [-1, +1]
        """
        # Normalizacja wejścia — zastępuje zewnętrzne mean/std
        x = self.input_norm(state)

        # Backbone — rozumienie stanu
        hidden = self.backbone(x)

        # Policy — które ruchy rozważać
        policy_logits = self.policy_head(hidden)

        # Maskowanie nielegalnych ruchów (ustawiamy -1e9 = praktycznie -nieskończoność)
        # Po softmaxie te ruchy będą miały prawdopodobieństwo ~0
        if action_mask is not None:
            policy_masked_logits = policy_logits.clone()
            policy_masked_logits[action_mask == 0] = -1e9
        else:
            policy_masked_logits = policy_logits

        # Value — ocena pozycji
        value = self.value_head(hidden)

        return {
            'policy_masked_logits': policy_masked_logits,
            'value': value,
        }

    def onnx_export(self, output_path: str, validate: bool = True):
        """
        Eksportuje model do ONNX dla C# (ONNX Runtime).
        LayerNorm jest częścią modelu — C# podaje surowe dane, sieć normalizuje sama.
        """
        import onnx as _onnx

        logger.info(f"Eksportuję model do: {output_path}")
        self.eval()
        device = next(self.parameters()).device

        dummy_state = torch.randn(1, self.state_dim, device=device)
        dummy_mask = torch.ones(1, ActionSpace.TOTAL_PRIMARY_ACTIONS, device=device)

        torch.onnx.export(
            self,
            (dummy_state, dummy_mask),
            output_path,
            input_names=['state', 'action_mask'],
            output_names=['policy_masked_logits', 'value'],
            opset_version=13,
            do_constant_folding=True,
            dynamic_axes={
                'state': {0: 'batch_size'},
                'action_mask': {0: 'batch_size'},
                'policy_masked_logits': {0: 'batch_size'},
                'value': {0: 'batch_size'},
            },
        )

        if validate:
            model = _onnx.load(output_path)
            _onnx.checker.check_model(model)
            logger.info(f"✓ ONNX OK: {output_path}")

        return output_path


# ---------------------------------------------------------------------------
# Dataset — wersja binarna (nowy format, mniejsze pliki)
# ---------------------------------------------------------------------------

class BinaryGameDataset(Dataset):
    """
    Wczytuje dane z pliku binarnego eksportowanego przez C#.

    Dlaczego binarny zamiast JSON?
    JSON zapisuje każdy float jako tekst (np. "0.123456789" = 11 znaków = 11 bajtów).
    Format binarny zapisuje float jako 4 bajty. To 2-3x mniej miejsca, a wczytywanie
    jest 10x szybsze bo nie trzeba parsować tekstu.

    Format pliku .bin (każdy rekord = jeden ruch):
    - [int32]   liczba ruchów w pliku (nagłówek)
    - Dla każdego ruchu:
      - [float32 × 1903]  State
      - [float32 × 120]   ActionMask
      - [float32 × 120]   PolicyTarget (visit counts z MCTS, znormalizowane do sumy 1)
      - [int32]           ActionIndex (który ruch wykonano, 0-119)
      - [float32]         ValueTarget (wynik z discount factor, patrz niżej)

    Jeśli plik ma rozszerzenie .bin.gz — wczytuje skompresowany (gzip).
    """

    RECORD_SIZE = (
        ActionSpace.STATE_VECTOR_SIZE * 4  # State: 1903 × float32
        + ActionSpace.TOTAL_PRIMARY_ACTIONS * 4  # ActionMask: 120 × float32
        + ActionSpace.TOTAL_PRIMARY_ACTIONS * 4  # PolicyTarget: 120 × float32
        + 4  # ActionIndex: int32
        + 4  # ValueTarget: float32
    )  # = 8440 bajtów na ruch

    def __init__(self, path: str):
        self.data: List[Dict] = []
        path = Path(path)

        opener = gzip.open if path.suffix == '.gz' else open
        with opener(path, 'rb') as f:
            n_records = struct.unpack('<i', f.read(4))[0]
            logger.info(f"Wczytuję {n_records} ruchów z {path.name}")

            for i in range(n_records):
                state = np.frombuffer(f.read(ActionSpace.STATE_VECTOR_SIZE * 4), dtype=np.float32).copy()
                action_mask = np.frombuffer(f.read(ActionSpace.TOTAL_PRIMARY_ACTIONS * 4), dtype=np.float32).copy()
                policy_target = np.frombuffer(f.read(ActionSpace.TOTAL_PRIMARY_ACTIONS * 4), dtype=np.float32).copy()
                action_index = struct.unpack('<i', f.read(4))[0]
                value_target = struct.unpack('<f', f.read(4))[0]

                self.data.append({
                    'state': state,
                    'action_mask': action_mask,
                    'policy_target': policy_target,
                    'action_index': action_index,
                    'value_target': value_target,
                })

        logger.info(f"✓ Wczytano {len(self.data)} ruchów")

    def __len__(self):
        return len(self.data)

    def __getitem__(self, idx):
        s = self.data[idx]
        return {
            'state': torch.from_numpy(s['state']),
            'action_mask': torch.from_numpy(s['action_mask']),
            'policy_target': torch.from_numpy(s['policy_target']),
            'action_taken': torch.tensor(s['action_index'], dtype=torch.long),
            'value_target': torch.tensor([s['value_target']], dtype=torch.float32),
        }


# ---------------------------------------------------------------------------
# Dataset — wersja JSON (kompatybilna ze starym formatem, z poprawkami)
# ---------------------------------------------------------------------------

def _apply_discount(moves: List[Dict], winner: Optional[str], gamma: float = 0.98) -> None:
    """
    Oblicza value_target dla każdego ruchu z discount factor.

    Co to jest discount factor?
    Wyobraź sobie grę szachową: wygrałeś. Który ruch był ważniejszy — mat w ostatniej
    turze czy ruch pionkiem w turze 3? Intuicyjnie: mat był ważniejszy.
    Discount factor gamma=0.98 daje:
    - ostatni ruch:  ±1.0
    - 10 ruchów wcześniej: ±0.98^10 ≈ ±0.82
    - 25 ruchów wcześniej: ±0.98^25 ≈ ±0.60

    Ruchy z początku gry mają słabszy sygnał — sieć nie "winni" ich tak mocno
    za wynik. To zmniejsza szum i przyspiesza uczenie.
    """
    if winner is None:
        for m in moves:
            m['value_target'] = 0.0
        return

    T = len(moves)
    for t, move in enumerate(moves):
        steps_from_end = T - 1 - t  # 0 dla ostatniego ruchu, T-1 dla pierwszego
        discount = gamma ** steps_from_end
        if move.get('Agent') == winner:
            move['value_target'] = 1.0 * discount
        else:
            move['value_target'] = -1.0 * discount


class GameDataset(Dataset):
    """
    Dataset wczytujący stary format JSON z C#.
    Kompatybilny z istniejącymi plikami — bez zmian w C#.

    Poprawki względem oryginału:
    - Usunięto normalizację stanów (sieć ma wbudowany LayerNorm)
    - Dodano discount factor dla value_target (patrz _apply_discount)
    - Zachowano obsługę PolicyTarget z MCTS (jeśli dostarczone przez C#)
    """

    def __init__(self, json_path: str, gamma: float = 0.98, validate_shapes: bool = True):
        self.data: List[Dict] = []
        self.gamma = gamma
        self.validate_shapes = validate_shapes

        source_path = Path(json_path)
        if source_path.is_dir():
            files = sorted(source_path.glob('*.json'))
        elif source_path.is_file():
            files = [source_path]
        else:
            files = [Path(p) for p in sorted(glob.glob(json_path))]

        for file_path in files:
            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    payload = json.load(f)
                self._ingest(payload, file_path)
            except Exception as e:
                logger.warning(f"Pomijam {file_path}: {e}")

        logger.info(f"Wczytano {len(self.data)} próbek")

    def _ingest(self, payload, source_name):
        # Format: jeden mecz = {"Moves": [...], "Winner": "gracz1", "MatchId": "..."}
        if isinstance(payload, dict) and 'Moves' in payload:
            self._ingest_match(payload, source_name)

        # Format: wiele meczów = {"MatchResults": [{...}, {...}]}
        elif isinstance(payload, dict) and 'MatchResults' in payload:
            for i, match in enumerate(payload.get('MatchResults') or []):
                self._ingest_match(match, f"{source_name}[{i}]")

        # Format: lista ruchów bez struktury meczów
        elif isinstance(payload, list):
            for i, sample in enumerate(payload):
                self._append_sample(sample, source_name, i, 0.0)

        else:
            raise ValueError(f"Nieznany format w {source_name}")

    def _ingest_match(self, match: Dict, source_name: str):
        winner = match.get('Winner')
        match_id = match.get('MatchId', str(source_name))
        moves = list(match.get('Moves') or [])

        # Tutaj liczymy discount dla każdego ruchu — w oryginale każdy dostawał ±1
        _apply_discount(moves, winner, gamma=self.gamma)

        for i, move in enumerate(moves):
            if not move.get('State') and not move.get('state'):
                continue
            self._append_sample(move, source_name, i,
                                 move.get('value_target', 0.0),
                                 match_id=match_id)

    def _append_sample(self, sample, source_name, idx, default_value_target, match_id=None):
        try:
            state = np.array(
                sample.get('State') or sample.get('state'), dtype=np.float32
            )
            action_mask = np.array(
                sample.get('ActionMask') or sample.get('action_mask'), dtype=np.float32
            )

            if self.validate_shapes:
                assert state.shape == (ActionSpace.STATE_VECTOR_SIZE,), \
                    f"Próbka {idx}: state ma kształt {state.shape}"
                assert action_mask.shape == (ActionSpace.TOTAL_PRIMARY_ACTIONS,), \
                    f"Próbka {idx}: action_mask ma kształt {action_mask.shape}"

            action_taken = int(
                sample.get('ActionIndex', sample.get('action_taken', -1))
            )
            if action_taken < 0:
                return

            # PolicyTarget = rozkład odwiedzin z MCTS (najlepsza informacja)
            # Jeśli C# nie eksportuje PolicyTarget, używamy one-hot (gorsza opcja)
            raw_pt = sample.get('PolicyTarget') or sample.get('policy_target')
            if raw_pt is not None:
                policy_target = np.array(raw_pt, dtype=np.float32)
            else:
                # One-hot: mniej informacji, ale lepsze niż nic
                # DOCELOWO: upewnij się że C# eksportuje PolicyTarget!
                policy_target = np.zeros(ActionSpace.TOTAL_PRIMARY_ACTIONS, dtype=np.float32)
                policy_target[action_taken] = 1.0

            self.data.append({
                'state': state,
                'action_mask': action_mask,
                'policy_target': policy_target,
                'action_taken': action_taken,
                'value_target': float(
                    sample.get('value_target', default_value_target)
                ),
                'match_id': match_id or sample.get('MatchId', str(source_name)),
            })

        except Exception as e:
            logger.warning(f"Pomijam próbkę {idx} z {source_name}: {e}")

    def __len__(self):
        return len(self.data)

    def __getitem__(self, idx):
        s = self.data[idx]
        return {
            'state': torch.from_numpy(s['state']),
            'action_mask': torch.from_numpy(s['action_mask']),
            'policy_target': torch.from_numpy(s['policy_target']),
            'action_taken': torch.tensor(s['action_taken'], dtype=torch.long),
            'value_target': torch.tensor([s['value_target']], dtype=torch.float32),
        }


# ---------------------------------------------------------------------------
# Funkcje strat (loss functions)
# ---------------------------------------------------------------------------

def policy_loss(logits: torch.Tensor, target: torch.Tensor) -> torch.Tensor:
    """
    Soft cross-entropy między przewidywaniami sieci a rozkładem odwiedzin MCTS.

    Co to znaczy?
    Sieć produkuje logity → po softmaxie rozkład prawdopodobieństwa ruchów.
    MCTS produkuje rozkład odwiedzin (np. ruch A: 40%, B: 35%, C: 25%).
    Strata mierzy jak daleko jest sieć od MCTS — chcemy żeby sieć "myślała"
    podobnie jak MCTS po wielu iteracjach.
    """
    target = target / target.sum(dim=1, keepdim=True).clamp_min(1e-8)
    log_probs = torch.log_softmax(logits, dim=1)
    return -(target * log_probs).sum(dim=1).mean()


def value_loss(pred: torch.Tensor, target: torch.Tensor) -> torch.Tensor:
    """MSE między przewidywaną a rzeczywistą (zdyskontowaną) wartością pozycji."""
    return nn.functional.mse_loss(pred, target)


# ---------------------------------------------------------------------------
# Trening i ewaluacja
# ---------------------------------------------------------------------------

def train_epoch(
    model: PolicyNetwork,
    dataloader: DataLoader,
    optimizer: optim.Optimizer,
    device: torch.device,
    value_weight: float = 0.5,
) -> Dict[str, float]:
    model.train()
    total_p, total_v, total_n = 0.0, 0.0, 0

    for batch in dataloader:
        state = batch['state'].to(device)
        action_mask = batch['action_mask'].to(device)
        pt = batch['policy_target'].to(device)
        vt = batch['value_target'].to(device)

        outputs = model(state, action_mask=action_mask)

        p_loss = policy_loss(outputs['policy_masked_logits'], pt)
        v_loss = value_loss(outputs['value'], vt)
        loss = p_loss + value_weight * v_loss

        optimizer.zero_grad()
        loss.backward()
        # Gradient clipping: zapobiega "eksplozji gradientów" — sytuacji gdy
        # jeden zły batch psuje całą sieć przez ogromną aktualizację wag
        torch.nn.utils.clip_grad_norm_(model.parameters(), max_norm=1.0)
        optimizer.step()

        n = state.size(0)
        total_p += p_loss.item() * n
        total_v += v_loss.item() * n
        total_n += n

    return {
        'policy_loss': total_p / max(total_n, 1),
        'value_loss': total_v / max(total_n, 1),
        'total_loss': (total_p + value_weight * total_v) / max(total_n, 1),
    }


def evaluate(
    model: PolicyNetwork,
    dataloader: DataLoader,
    device: torch.device,
    value_weight: float = 0.5,
) -> Dict[str, float]:
    model.eval()
    total_p, total_v, total_n = 0.0, 0.0, 0

    with torch.no_grad():
        for batch in dataloader:
            state = batch['state'].to(device)
            action_mask = batch['action_mask'].to(device)
            pt = batch['policy_target'].to(device)
            vt = batch['value_target'].to(device)

            outputs = model(state, action_mask=action_mask)
            p_loss = policy_loss(outputs['policy_masked_logits'], pt)
            v_loss = value_loss(outputs['value'], vt)

            n = state.size(0)
            total_p += p_loss.item() * n
            total_v += v_loss.item() * n
            total_n += n

    return {
        'policy_loss': total_p / max(total_n, 1),
        'value_loss': total_v / max(total_n, 1),
        'total_loss': (total_p + value_weight * total_v) / max(total_n, 1),
    }
