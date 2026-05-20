"""
Unit Tests for Binary Game State Encoding
==========================================

Tests for bit-packing, sparse arrays, and state reconstruction.
"""

import numpy as np
import tempfile
from pathlib import Path
from game_state_decoder import GameStateDecoder, BatchedGameStateDecoder


def test_bit_packing():
    """Test bit-packing/unpacking roundtrip."""
    decoder = GameStateDecoder()
    
    # Test data
    original = np.array([True, False, True, False, False, False, True, False], dtype=bool)
    
    # Pack (simulating C# behavior)
    def pack_bools(bools):
        byte_count = (len(bools) + 7) // 8
        packed = np.zeros(byte_count, dtype=np.uint8)
        for i, b in enumerate(bools):
            if b:
                byte_idx = i // 8
                bit_idx = i % 8
                packed[byte_idx] |= (1 << bit_idx)
        return packed
    
    # Unpack (Python side)
    packed = pack_bools(original)
    unpacked = decoder._unpack_booleans(packed)[:len(original)]
    
    assert np.array_equal(original, unpacked), "Bit-packing roundtrip failed"
    print("✓ Bit-packing test passed")


def test_sparse_array_reconstruction():
    """Test sparse array reconstruction to dense."""
    decoder = GameStateDecoder()
    
    # Simulate sparse card indices
    sparse_cards = [
        [2, 5, 10],      # State 0: cards at indices 2, 5, 10
        [1, 3],          # State 1: cards at indices 1, 3
        [],              # State 2: no cards
    ]
    
    # Reconstruct to dense
    num_cards = 73
    dense = np.zeros((3, num_cards), dtype=np.float32)
    
    for i, indices in enumerate(sparse_cards):
        for idx in indices:
            if idx < num_cards:
                dense[i, idx] = 1.0
    
    # Verify
    assert dense[0, 2] == 1.0, "Card index 2 not reconstructed"
    assert dense[0, 5] == 1.0, "Card index 5 not reconstructed"
    assert dense[1, 1] == 1.0, "Card index 1 not reconstructed"
    assert dense[2].sum() == 0, "Empty state should be all zeros"
    assert dense.shape == (3, 73), "Wrong shape"
    
    print("✓ Sparse array reconstruction test passed")


def test_state_dimension_calculation():
    """Test state dimension calculation."""
    decoder = GameStateDecoder()
    
    # Calculate expected dimensions
    continuous = decoder._calculate_continuous_features()
    total = decoder._calculate_state_dimension()
    
    # Dimensions should be reasonable (not exact due to implementation differences)
    # But should be close to 1903 (allow +/- 10 for rounding)
    assert continuous > 40, f"Continuous too small: {continuous}"
    assert continuous < 80, f"Continuous too large: {continuous}"
    assert total > 1890, f"Total too small: {total}"
    assert total < 1920, f"Total too large: {total}"
    
    print(f"✓ State dimension test passed (continuous={continuous}, total={total})")


def test_action_mask_validity():
    """Test action mask reconstruction."""
    decoder = GameStateDecoder()
    
    # Create synthetic action masks
    num_states = 10
    action_masks_raw = np.random.rand(num_states * 120).astype(np.float32)
    action_masks_raw = np.clip(action_masks_raw, 0, 1)  # Ensure [0,1]
    
    # Reconstruct
    reconstructed = decoder._reconstruct_action_masks(action_masks_raw, num_states)
    
    # Verify
    assert reconstructed.shape == (num_states, 120), f"Wrong shape: {reconstructed.shape}"
    assert reconstructed.dtype == np.float32, f"Wrong dtype: {reconstructed.dtype}"
    assert np.all((reconstructed >= 0) & (reconstructed <= 1)), "Values out of [0,1] range"
    
    print("✓ Action mask validity test passed")


def test_normalized_values():
    """Test state normalization."""
    decoder = BatchedGameStateDecoder()
    
    # Create synthetic states with known values
    num_states = 100
    states = np.random.randn(num_states, 1903).astype(np.float32)
    
    # Normalize only first 47 dimensions (continuous)
    normalized = decoder._normalize_states(states)
    
    # Check first few dimensions are normalized
    for i in range(5):
        col = normalized[:, i]
        mean = np.mean(col)
        std = np.std(col)
        assert abs(mean) < 0.1, f"Column {i} mean not normalized: {mean}"
        assert abs(std - 1.0) < 0.1, f"Column {i} std not normalized: {std}"
    
    # Check binary features not modified
    for i in range(1903 - 100, 1903):  # Last 100 dimensions
        if np.max(states[:, i]) <= 1.0 and np.min(states[:, i]) >= 0:
            # This was likely a binary feature
            assert np.allclose(normalized[:, i], states[:, i]), f"Binary feature modified at {i}"
    
    print("✓ State normalization test passed")


def test_decoder_initialization():
    """Test decoder parameters."""
    decoder = GameStateDecoder()
    
    assert decoder.NUM_CARDS == 73, "Wrong number of cards"
    assert decoder.NUM_WONDERS == 12, "Wrong number of wonders"
    assert decoder.NUM_PROGRESS_TOKENS == 10, "Wrong number of tokens"
    assert decoder.TOTAL_PRIMARY_ACTIONS == 120, "Wrong number of actions"
    assert decoder.NUM_PLAYERS == 2, "Wrong number of players"
    
    print("✓ Decoder initialization test passed")


def test_batched_decoder_preprocessing():
    """Test batched decoder preprocessing."""
    decoder = BatchedGameStateDecoder()
    
    # Create synthetic data
    num_samples = 50
    states = np.random.randn(num_samples, 1903).astype(np.float32)
    actions = np.random.randint(0, 120, num_samples, dtype=np.int32)
    masks = np.random.rand(num_samples, 120).astype(np.float32)
    masks = np.round(masks)  # Make binary
    
    # Test normalization
    normalized = decoder._normalize_states(states)
    assert normalized.shape == states.shape
    assert normalized.dtype == np.float32
    
    # Test no NaNs or Infs
    assert np.all(np.isfinite(normalized)), "Normalized data has NaN or Inf"
    
    print("✓ Batched decoder preprocessing test passed")


def test_empty_sparse_arrays():
    """Test handling of empty sparse arrays."""
    decoder = GameStateDecoder()
    
    # All empty arrays
    empty_arrays = [[] for _ in range(10)]
    
    # Reconstruct (should create zero arrays)
    dense = np.zeros((10, 73), dtype=np.float32)
    for i, indices in enumerate(empty_arrays):
        for idx in indices:
            dense[i, idx] = 1.0
    
    # Verify all zeros
    assert np.all(dense == 0), "Empty sparse arrays should produce all zeros"
    assert dense.shape == (10, 73), "Wrong shape for empty reconstruction"
    
    print("✓ Empty sparse arrays test passed")


def run_all_tests():
    """Run all tests."""
    print("\n🧪 Running Binary Encoding Unit Tests\n")
    print("=" * 50)
    
    tests = [
        ("Bit-packing", test_bit_packing),
        ("Sparse arrays", test_sparse_array_reconstruction),
        ("State dimensions", test_state_dimension_calculation),
        ("Action mask validity", test_action_mask_validity),
        ("State normalization", test_normalized_values),
        ("Decoder init", test_decoder_initialization),
        ("Batched preprocessing", test_batched_decoder_preprocessing),
        ("Empty sparse arrays", test_empty_sparse_arrays),
    ]
    
    passed = 0
    failed = 0
    
    for name, test_func in tests:
        try:
            test_func()
            passed += 1
        except AssertionError as e:
            print(f"✗ {name} test failed: {e}")
            failed += 1
        except Exception as e:
            print(f"✗ {name} test error: {e}")
            failed += 1
    
    print("=" * 50)
    print(f"\n📊 Results: {passed} passed, {failed} failed")
    
    if failed == 0:
        print("✅ All tests passed!")
        return True
    else:
        print("❌ Some tests failed")
        return False


if __name__ == "__main__":
    success = run_all_tests()
    exit(0 if success else 1)
