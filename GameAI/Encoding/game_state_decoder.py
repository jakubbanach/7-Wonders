"""
Binary Game State Decoder for 7 Wonders Game AI
================================================

Decodes binary-encoded game states from C# BinaryGameStateEncoder:
- Bit-packed booleans → dense boolean arrays
- Sparse card/wonder/token indices → dense feature arrays  
- Continuous normalized values → float arrays

Usage:
    from game_state_decoder import GameStateDecoder
    
    decoder = GameStateDecoder()
    states, actions, masks = decoder.load_training_data("data.npz")
"""

import numpy as np
import zipfile
from pathlib import Path
from typing import Dict, List, Tuple, Optional


class GameStateDecoder:
    """Decodes binary-encoded game states for neural network training."""
    
    # Constants matching GameStateEncoder.cs
    NUM_CARDS = 73
    NUM_WONDERS = 12
    NUM_PROGRESS_TOKENS = 10
    MAX_BOARD_SLOTS = 20
    NUM_PLAYERS = 2
    TOTAL_PRIMARY_ACTIONS = 120
    
    def __init__(self):
        """Initialize decoder with sparse array shapes."""
        self.card_catalog_size = self.NUM_CARDS
        self.wonder_catalog_size = self.NUM_WONDERS
        self.token_catalog_size = self.NUM_PROGRESS_TOKENS
        
    def load_training_data(self, npz_path: str) -> Tuple[np.ndarray, np.ndarray, np.ndarray]:
        """
        Load and decode binary training data from .npz file.
        
        Returns:
            (states, actions, action_masks) - Ready for neural network training
            states: (N, state_dim) float32
            actions: (N,) int32 - action indices
            action_masks: (N, 120) float32 - legal action masks
        """
        with zipfile.ZipFile(npz_path, 'r') as archive:
            namelist = set(archive.namelist())

            # Load action data first to determine number of states
            if 'actions/action_indices.npy' in namelist:
                actions = np.load(archive.open('actions/action_indices.npy'))
            else:
                actions = np.array([], dtype=np.int32)

            num_states = len(actions)

            # Load continuous data (may be empty)
            if 'states/packed_data.npy' in namelist:
                all_packed_data = np.load(archive.open('states/packed_data.npy'))
            else:
                all_packed_data = np.empty(0, dtype=np.float32)

            if 'states/packed_booleans.npy' in namelist:
                all_packed_booleans = np.load(archive.open('states/packed_booleans.npy'))
            else:
                all_packed_booleans = np.empty(0, dtype=np.uint8)

            # Load state offsets (written by the C# writer) so we can split variable-length packed_data
            if 'metadata/state_offsets.npy' in namelist:
                state_offsets = np.load(archive.open('metadata/state_offsets.npy')).astype(int)
            else:
                state_offsets = None

            # Load sparse data (provide num_states so loader can return proper empty lists)
            card_indices_p1 = self._load_sparse_array(archive, 'sparse/card_indices_p1', num_states)
            card_indices_p2 = self._load_sparse_array(archive, 'sparse/card_indices_p2', num_states)

            owned_wonder_p1 = self._load_sparse_array(archive, 'sparse/owned_wonder_indices_p1', num_states)
            built_wonder_p1 = self._load_sparse_array(archive, 'sparse/built_wonder_indices_p1', num_states)
            owned_wonder_p2 = self._load_sparse_array(archive, 'sparse/owned_wonder_indices_p2', num_states)
            built_wonder_p2 = self._load_sparse_array(archive, 'sparse/built_wonder_indices_p2', num_states)

            token_indices_p1 = self._load_sparse_array(archive, 'sparse/progress_token_indices_p1', num_states)
            token_indices_p2 = self._load_sparse_array(archive, 'sparse/progress_token_indices_p2', num_states)

            pyramid_cards = self._load_sparse_array(archive, 'sparse/pyramid_card_indices', num_states)
            discarded_cards = self._load_sparse_array(archive, 'sparse/discarded_card_indices', num_states)

            # Split packed_data and packed_booleans per-state using offsets (if available)
            packed_data_slices: List[np.ndarray] = []
            if all_packed_data.size == 0:
                packed_data_slices = [np.empty(0, dtype=np.float32) for _ in range(num_states)]
            else:
                if state_offsets is not None and len(state_offsets) == num_states:
                    for i in range(num_states):
                        start = int(state_offsets[i])
                        end = int(state_offsets[i + 1]) if i + 1 < len(state_offsets) else int(all_packed_data.size)
                        packed_data_slices.append(np.array(all_packed_data[start:end], dtype=np.float32))
                else:
                    # Fallback: evenly split if offsets missing
                    per = len(all_packed_data) // num_states if num_states > 0 else 0
                    for i in range(num_states):
                        start = i * per
                        end = start + per
                        packed_data_slices.append(np.array(all_packed_data[start:end], dtype=np.float32))

            # Booleans are bit-packed with a fixed per-state bit count (encoder uses a constant layout)
            booleans_per_state: List[np.ndarray] = []
            if all_packed_booleans.size == 0:
                booleans_per_state = [np.empty(0, dtype=bool) for _ in range(num_states)]
            else:
                # Calculate bits per state based on encoder layout: 2 active + 3 epoch + 5 game flags + 9 conflict zone used flags + 3*MAX_BOARD_SLOTS
                bits_per_state = 2 + 3 + 5 + 9 + (self.MAX_BOARD_SLOTS * 3)
                bytes_per_state = (bits_per_state + 7) // 8
                raw_bytes = np.frombuffer(all_packed_booleans.tobytes(), dtype=np.uint8)
                for i in range(num_states):
                    start = i * bytes_per_state
                    slice_bytes = raw_bytes[start:start + bytes_per_state]
                    booleans_per_state.append(self._unpack_booleans(slice_bytes)[:bits_per_state])

            # Load action masks (may be empty)
            if 'actions/action_masks.npy' in namelist:
                action_masks_raw = np.load(archive.open('actions/action_masks.npy'))
            else:
                action_masks_raw = np.empty(0, dtype=np.float32)

            # If there are no actions (no samples), return empty arrays
            if num_states == 0:
                return (np.empty((0, self._calculate_state_dimension()), dtype=np.float32),
                        np.empty((0,), dtype=np.int32),
                        np.empty((0, self.TOTAL_PRIMARY_ACTIONS), dtype=np.float32))

            # Fast path: some archives store the full dense state vector directly.
            state_dim = self._calculate_state_dimension()
            if all_packed_data.size == num_states * state_dim:
                states = all_packed_data.reshape(num_states, state_dim).astype(np.float32)
                action_masks = self._reconstruct_action_masks(action_masks_raw, num_states)
                return states, actions.astype(np.int32), action_masks.astype(np.float32)

            # Reconstruct dense state arrays using per-state slices and per-state booleans
            states = self._reconstruct_states(
                packed_data_slices, booleans_per_state, num_states,
                card_indices_p1, card_indices_p2,
                owned_wonder_p1, built_wonder_p1, owned_wonder_p2, built_wonder_p2,
                token_indices_p1, token_indices_p2,
                pyramid_cards, discarded_cards
            )

            # Reconstruct action masks
            action_masks = self._reconstruct_action_masks(action_masks_raw, num_states)

        return states.astype(np.float32), actions.astype(np.int32), action_masks.astype(np.float32)
    
    def _unpack_booleans(self, packed: np.ndarray) -> np.ndarray:
        """
        Unpack bit-packed booleans (8 per byte) to dense boolean array.
        
        Packing format: bit i of byte j corresponds to boolean (j*8 + i)
        """
        unpacked = np.zeros(len(packed) * 8, dtype=np.bool_)
        
        for byte_idx, byte_val in enumerate(packed):
            for bit_idx in range(8):
                bool_idx = byte_idx * 8 + bit_idx
                if bool_idx < len(unpacked):
                    unpacked[bool_idx] = bool((byte_val >> bit_idx) & 1)
        
        return unpacked
    
    def _load_sparse_array(self, archive: zipfile.ZipFile, base_path: str, num_states: int) -> List[List[int]]:
        """
        Load sparse array data (variable-length indices for each state).
        
        Format: Stores counts and concatenated indices separately
        """
        namelist = set(archive.namelist())
        counts_key = f'{base_path}_counts.npy'
        indices_key = f'{base_path}.npy'

        if counts_key in namelist and indices_key in namelist:
            counts = np.load(archive.open(counts_key))
            indices = np.load(archive.open(indices_key))

            # Reconstruct variable-length arrays
            result = []
            offset = 0
            for count in counts:
                c = int(count)
                result.append(list(indices[offset:offset + c]))
                offset += c
            # If counts length differs from num_states, pad or trim
            if len(result) < num_states:
                result.extend([[] for _ in range(num_states - len(result))])
            elif len(result) > num_states:
                result = result[:num_states]
            return result

        # Missing files: return num_states empty lists
        return [[] for _ in range(num_states if num_states > 0 else 0)]
    
    def _reconstruct_states(self,
        packed_data_slices: List[np.ndarray],
        booleans_per_state: List[np.ndarray],
        num_states: int,
        card_indices_p1: List[List[int]],
        card_indices_p2: List[List[int]],
        owned_wonder_p1: List[List[int]],
        built_wonder_p1: List[List[int]],
        owned_wonder_p2: List[List[int]],
        built_wonder_p2: List[List[int]],
        token_indices_p1: List[List[int]],
        token_indices_p2: List[List[int]],
        pyramid_cards: List[List[int]],
        discarded_cards: List[List[int]]
    ) -> np.ndarray:
        """
        Reconstruct full state arrays by expanding sparse representations
        and interleaving with continuous data.
        """
        
        # Calculate state dimension:
        # Continuous: packed_data.shape[0] / num_states
        # Booleans: total bool features
        # Sparse expanded: cards (73*2), wonders (12*4), tokens (10*2), pyramid (73), discard (73)
        
        state_dim = self._calculate_state_dimension()
        states = np.zeros((num_states, state_dim), dtype=np.float32)
        
        # Fill in continuous data (robust to missing/short per-state slices)
        continuous_expected = self._calculate_continuous_features()
        for i in range(num_states):
            per = packed_data_slices[i] if i < len(packed_data_slices) else np.empty(0, dtype=np.float32)
            if per.size > 0:
                to_fill = min(per.size, continuous_expected)
                states[i, :to_fill] = per[:to_fill]
            # remaining continuous dims remain zero if data missing
        
        # Reconstruct sparse card features
        card_offset = self._calculate_continuous_features()
        for i in range(num_states):
            # Cards for player 1
            card_feature_p1 = np.zeros(self.NUM_CARDS, dtype=np.float32)
            if i < len(card_indices_p1):
                for idx in card_indices_p1[i]:
                    if idx < self.NUM_CARDS:
                        card_feature_p1[idx] = 1.0
            states[i, card_offset:card_offset + self.NUM_CARDS] = card_feature_p1
            
            # Cards for player 2
            card_feature_p2 = np.zeros(self.NUM_CARDS, dtype=np.float32)
            if i < len(card_indices_p2):
                for idx in card_indices_p2[i]:
                    if idx < self.NUM_CARDS:
                        card_feature_p2[idx] = 1.0
            states[i, card_offset + self.NUM_CARDS:card_offset + 2*self.NUM_CARDS] = card_feature_p2
        
        # Reconstruct wonder features (owned and built separate)
        wonder_offset = card_offset + 2 * self.NUM_CARDS
        for i in range(num_states):
            # wonder_features holds: P1 owned (12), P1 built (12), P2 owned (12), P2 built (12) => 4 * NUM_WONDERS
            wonder_features = np.zeros(4 * self.NUM_WONDERS, dtype=np.float32)
            # Owned wonders P1
            if i < len(owned_wonder_p1):
                for idx in owned_wonder_p1[i]:
                    if idx < self.NUM_WONDERS:
                        wonder_features[idx] = 1.0
            
            # Built wonders P1
            if i < len(built_wonder_p1):
                for idx in built_wonder_p1[i]:
                    if idx < self.NUM_WONDERS:
                        wonder_features[self.NUM_WONDERS + idx] = 1.0
            
            # Similar for P2 (offset by 2*NUM_WONDERS)
            if i < len(owned_wonder_p2):
                for idx in owned_wonder_p2[i]:
                    if idx < self.NUM_WONDERS:
                        wonder_features[2*self.NUM_WONDERS + idx] = 1.0
            
            if i < len(built_wonder_p2):
                for idx in built_wonder_p2[i]:
                    if idx < self.NUM_WONDERS:
                        wonder_features[3*self.NUM_WONDERS + idx] = 1.0
            
            states[i, wonder_offset:wonder_offset + 4*self.NUM_WONDERS] = wonder_features
        
        # Reconstruct token features
        token_offset = wonder_offset + 4 * self.NUM_WONDERS
        for i in range(num_states):
            token_features = np.zeros(self.NUM_PROGRESS_TOKENS * 2, dtype=np.float32)
            
            if i < len(token_indices_p1):
                for idx in token_indices_p1[i]:
                    if idx < self.NUM_PROGRESS_TOKENS:
                        token_features[idx] = 1.0
            
            if i < len(token_indices_p2):
                for idx in token_indices_p2[i]:
                    if idx < self.NUM_PROGRESS_TOKENS:
                        token_features[self.NUM_PROGRESS_TOKENS + idx] = 1.0
            
            states[i, token_offset:token_offset + 2*self.NUM_PROGRESS_TOKENS] = token_features
        
        # Reconstruct pyramid slots as the original layout: 20 slots × (3 bools + cost + NUM_CARDS identity bits)
        pyramid_offset = token_offset + 2 * self.NUM_PROGRESS_TOKENS
        slot_block = 4 + self.NUM_CARDS
        bits_before_pyramid = 2 + 3 + 5 + 9  # active + epoch + game flags + conflict zone used flags
        for i in range(num_states):
            # per-state booleans and extra costs for visible slots
            bools = booleans_per_state[i] if i < len(booleans_per_state) else np.zeros(0, dtype=bool)
            per = packed_data_slices[i] if i < len(packed_data_slices) else np.empty(0, dtype=np.float32)
            # continuous part consumed first
            extra_costs = per[continuous_expected:] if per.size > continuous_expected else np.empty(0, dtype=np.float32)
            cost_ptr = 0
            card_ptr = 0
            for s in range(self.MAX_BOARD_SLOTS):
                slot_base = pyramid_offset + s * slot_block
                # booleans for slot: has_card, is_hidden, is_available
                b_idx = bits_before_pyramid + s * 3
                has_card = bools[b_idx] if b_idx < len(bools) else False
                is_hidden = bools[b_idx + 1] if (b_idx + 1) < len(bools) else False
                is_available = bools[b_idx + 2] if (b_idx + 2) < len(bools) else False

                states[i, slot_base] = 1.0 if has_card else 0.0
                states[i, slot_base + 1] = 1.0 if is_hidden else 0.0
                states[i, slot_base + 2] = 1.0 if is_available else 0.0

                # cost for visible & not hidden slots comes from extra_costs in order
                if has_card and (not is_hidden):
                    if cost_ptr < len(extra_costs):
                        states[i, slot_base + 3] = float(extra_costs[cost_ptr])
                        cost_ptr += 1
                    else:
                        states[i, slot_base + 3] = 0.0
                    # assign identity bit for the visible card from pyramid_cards list (ordered by slot)
                    if i < len(pyramid_cards) and card_ptr < len(pyramid_cards[i]):
                        idx = pyramid_cards[i][card_ptr]
                        if idx < self.NUM_CARDS:
                            states[i, slot_base + 4 + idx] = 1.0
                        card_ptr += 1
                else:
                    # no cost and no identity bits
                    states[i, slot_base + 3] = 0.0
        
        # Reconstruct discarded card features
        discard_offset = pyramid_offset + (self.MAX_BOARD_SLOTS * (4 + self.NUM_CARDS))
        for i in range(num_states):
            discard_features = np.zeros(self.NUM_CARDS, dtype=np.float32)
            if i < len(discarded_cards):
                for idx in discarded_cards[i]:
                    if idx < self.NUM_CARDS:
                        discard_features[idx] = 1.0
            states[i, discard_offset:discard_offset + self.NUM_CARDS] = discard_features
        
        return states
    
    def _reconstruct_action_masks(self, action_masks_raw: np.ndarray, num_states: int) -> np.ndarray:
        """
        Reconstruct action masks from sparse representation.
        """
        action_masks = np.zeros((num_states, self.TOTAL_PRIMARY_ACTIONS), dtype=np.float32)
        
        if len(action_masks_raw) > 0:
            masks_per_state = len(action_masks_raw) // num_states if num_states > 0 else len(action_masks_raw)
            for i in range(num_states):
                action_masks[i, :] = action_masks_raw[i*masks_per_state:(i+1)*masks_per_state][:self.TOTAL_PRIMARY_ACTIONS]
        
        return action_masks
    
    def _calculate_continuous_features(self) -> int:
        """Calculate number of continuous features before sparse data."""
        # Layout of the first 74 dimensions in the dense state vector:
        #
        # Global state (0..47):
        #   0..1   - active player one-hot
        #   2..4   - epoch one-hot
        #   5      - conflict position
        #   6      - game ended flag
        #   7..10  - victory type one-hot
        #   11..37 - 9 conflict zones × 3 values each (used / coin loss / VP loss)
        #   38..47 - available progress tokens (10 one-hot flags)
        #
        # Player 1 continuous block (48..61):
        #   coins, victory points, 5 resources, 7 science symbols
        #
        # Player 2 continuous block (62..75):
        #   coins, victory points, 5 resources, 7 science symbols
        continuous_global = 2 + 3 + 1 + 1 + 4 + 27 + 10  # 48
        continuous_per_player = 14
        continuous_both_players = continuous_per_player * 2

        # This is the truly continuous part (global + both players)
        return continuous_global + continuous_both_players
    
    def _calculate_state_dimension(self) -> int:
        """Calculate total state dimension matching original GameStateEncoder."""
        # Original format: all features as floats
        # Final layout:
        #   0..47    - global state
        #   48..75   - both players continuous state (28)
        #   76..289  - both players dense binary state (214)
        #   290..1829 - pyramid board (1540)
        #   1830..1902 - discarded cards (73)
        # Total: 1903
        
        continuous_global = 48
        continuous_per_player = 14  # coins, vp, 5 resources, 7 science
        continuous_both_players = continuous_per_player * 2
        
        binary_cards = 73 * 2  # P1 and P2
        binary_wonders = 24 * 2  # 12 wonders × 2 (owned/built) × 2 players
        binary_tokens = 10 * 2  # P1 and P2
        
        pyramid = 20 * 77  # 20 slots × (4 metadata + 73 card identities)
        discard = 73
        
        total = (continuous_global + continuous_both_players + 
                binary_cards + binary_wonders + binary_tokens + 
                pyramid + discard)
        
        return total


class BatchedGameStateDecoder(GameStateDecoder):
    """Extended decoder for efficient batched loading and preprocessing."""
    
    def load_and_preprocess(self, npz_path: str, 
                           normalize: bool = True,
                           shuffle: bool = False) -> Tuple[np.ndarray, np.ndarray, np.ndarray]:
        """
        Load, decode, and preprocess training data.
        
        Args:
            npz_path: Path to .npz file
            normalize: Whether to normalize state features
            shuffle: Whether to shuffle data
        
        Returns:
            (states, actions, action_masks) as float32 arrays
        """
        states, actions, masks = self.load_training_data(npz_path)
        
        if normalize:
            states = self._normalize_states(states)
        
        if shuffle:
            # Shuffle maintaining alignment
            indices = np.random.permutation(len(states))
            states = states[indices]
            actions = actions[indices]
            masks = masks[indices]
        
        return states, actions, masks
    
    def _normalize_states(self, states: np.ndarray) -> np.ndarray:
        """
        Normalize state features using z-score normalization.
        Handles sparse features appropriately.
        """
        normalized = states.copy()
        
        # Normalize continuous features (first 47 dimensions)
        continuous_dim = self._calculate_continuous_features()
        for i in range(continuous_dim):
            col = normalized[:, i]
            mean = np.mean(col)
            std = np.std(col)
            if std > 1e-6:
                normalized[:, i] = (col - mean) / std
        
        # Binary features (sparse reconstructed) - no normalization needed
        # They're already 0 or 1
        
        return normalized


def main():
    """Example usage of decoder."""
    decoder = BatchedGameStateDecoder()
    
    # Load example data
    npz_file = "training_data_3.npz"
    if Path(npz_file).exists():
        print(f"Loading {npz_file}...")
        states, actions, masks = decoder.load_and_preprocess(npz_file, normalize=True)
        
        print(f"States shape: {states.shape}")
        print(f"Actions shape: {actions.shape}")
        print(f"Masks shape: {masks.shape}")
        
        print(f"\nStates dtype: {states.dtype}")
        print(f"Actions dtype: {actions.dtype}")
        print(f"Masks dtype: {masks.dtype}")
        
        print(f"\nState value ranges:")
        print(f"  Min: {states.min():.4f}, Max: {states.max():.4f}")
        print(f"  Mean: {states.mean():.4f}, Std: {states.std():.4f}")
        
        return states, actions, masks
    else:
        print(f"{npz_file} not found")
        return None, None, None


if __name__ == "__main__":
    main()
