using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

/// <summary>
/// Writes optimized binary training data to .npz format (ZIP archive containing .npy files).
/// This enables efficient storage and direct loading in Python with NumPy.
/// </summary>
public static class BinaryTrainingDataWriter
{
    /// <summary>
    /// Saves a collection of matches with binary-encoded game states to .npz format.
    /// Format: ZIP containing:
    ///   - metadata.json: Training metadata
    ///   - states/packed_data_*.npy: Continuous state values (float32)
    ///   - states/packed_booleans_*.npy: Bit-packed booleans (uint8)
    ///   - actions/legal_action_indices_*.npy: Sparse action indices
    ///   - sparse/card_indices_p1_*.npy, etc.
    /// </summary>
    public static void SaveTrainingDataNpz(List<MatchResult> matches, string outputPath)
    {
        using (var zipArchive = ZipFile.Open(outputPath, ZipArchiveMode.Create))
        {
            int gameIndex = 0;

            // Collect all encoded states and training targets
            var encodedStates = new List<BinaryEncodingResult>();
            var allPolicyTargets = new List<float>();
            var allValueTargets = new List<float>();

            foreach (var match in matches)
            {
                if (match.Moves == null)
                    continue;

                foreach (var move in match.Moves)
                {
                    if (move.State == null)
                        continue;

                    var binaryResult = new BinaryEncodingResult
                    {
                        PackedData = move.State,
                        ActionMaskDense = move.ActionMask ?? Array.Empty<float>()
                    };

                    var policyTarget = move.PolicyTarget;
                    if (policyTarget == null || policyTarget.Length != ActionSpace.TotalPrimaryActions)
                    {
                        policyTarget = new float[ActionSpace.TotalPrimaryActions];
                        if (move.ActionIndex >= 0 && move.ActionIndex < policyTarget.Length)
                            policyTarget[move.ActionIndex] = 1f;
                    }

                    float valueTarget = 0f;
                    if (!string.IsNullOrWhiteSpace(match.Winner))
                    {
                        valueTarget = string.Equals(move.Agent, match.Winner, StringComparison.Ordinal)
                            ? 1f
                            : -1f;
                    }

                    encodedStates.Add(binaryResult);
                    allPolicyTargets.AddRange(policyTarget);
                    allValueTargets.Add(valueTarget);
                    gameIndex++;
                }
            }

            // Write continuous data
            WriteStateData(zipArchive, encodedStates);

            // Write sparse indices
            WriteSparseData(zipArchive, encodedStates);

            // Write training targets
            WriteTargetData(zipArchive, allPolicyTargets, allValueTargets);

            // Write action data
            WriteActionData(zipArchive, encodedStates, matches);

            // Write metadata
            WriteMetadata(zipArchive, matches.Count, gameIndex);
        }
    }

    /// <summary>
    /// Saves a single match result in binary format.
    /// </summary>
    public static BinaryMatchData EncodeSingleMatch(MatchResult match, Gra gameState)
    {
        var result = new BinaryMatchData
        {
            MatchId = match.MatchId.ToString(),
            Agent1 = match.Agent1Name,
            Agent2 = match.Agent2Name,
            Winner = match.Winner,
            Seed = match.Seed,
            Turns = match.Turns,
            MoveCount = match.Moves?.Count ?? 0
        };

        // Encode states for all moves
        var encodedStates = new List<BinaryEncodingResult>();
        if (match.Moves != null)
        {
            foreach (var move in match.Moves)
            {
                // In production, reconstruct game state from replay and encode
                // For now, store the binary representation if available
                var encoded = new BinaryEncodingResult
                {
                    ActionMaskDense = move.ActionMask ?? Array.Empty<float>()
                };
                encodedStates.Add(encoded);
            }
        }

        result.EncodedStates = encodedStates;
        return result;
    }

    private static void WriteStateData(ZipArchive archive, List<BinaryEncodingResult> states)
    {
        // Collect all packed data
        var allPackedData = new List<float>();
        var allPackedBooleans = new List<byte>();
        var stateOffsets = new List<int>();

        foreach (var state in states)
        {
            stateOffsets.Add(allPackedData.Count);
            allPackedData.AddRange(state.PackedData);
            allPackedBooleans.AddRange(state.PackedBooleans);
        }

        // Always write packed_data.npy (may be empty)
        var data = WriteNpyArray(allPackedData.ToArray(), "f4"); // float32
        var entry = archive.CreateEntry("states/packed_data.npy", CompressionLevel.Optimal);
        using (var stream = entry.Open())
        {
            stream.Write(data, 0, data.Length);
        }

        // Write packed booleans
        // Always write packed_booleans.npy (may be empty)
        var boolData = WriteNpyArray(allPackedBooleans.ToArray(), "u1"); // uint8
        var boolEntry = archive.CreateEntry("states/packed_booleans.npy", CompressionLevel.Optimal);
        using (var stream = boolEntry.Open())
        {
            stream.Write(boolData, 0, boolData.Length);
        }

        // Write state offsets for reconstruction
        var offsetData = WriteNpyArray(stateOffsets.ToArray(), "i4"); // int32
        var offsetEntry = archive.CreateEntry("metadata/state_offsets.npy", CompressionLevel.Optimal);
        using (var stream = offsetEntry.Open())
        {
            stream.Write(offsetData, 0, offsetData.Length);
        }
    }

    private static void WriteSparseData(ZipArchive archive, List<BinaryEncodingResult> states)
    {
        WriteSparseArray(archive, states.Select(s => s.CardIndicesP1).ToList(), "sparse/card_indices_p1");
        WriteSparseArray(archive, states.Select(s => s.CardIndicesP2).ToList(), "sparse/card_indices_p2");
        WriteSparseArray(archive, states.Select(s => s.OwnedWonderIndicesP1).ToList(), "sparse/owned_wonder_indices_p1");
        WriteSparseArray(archive, states.Select(s => s.BuiltWonderIndicesP1).ToList(), "sparse/built_wonder_indices_p1");
        WriteSparseArray(archive, states.Select(s => s.OwnedWonderIndicesP2).ToList(), "sparse/owned_wonder_indices_p2");
        WriteSparseArray(archive, states.Select(s => s.BuiltWonderIndicesP2).ToList(), "sparse/built_wonder_indices_p2");
        WriteSparseArray(archive, states.Select(s => s.ProgressTokenIndicesP1).ToList(), "sparse/progress_token_indices_p1");
        WriteSparseArray(archive, states.Select(s => s.ProgressTokenIndicesP2).ToList(), "sparse/progress_token_indices_p2");
        WriteSparseArray(archive, states.Select(s => s.PyramidCardIndices).ToList(), "sparse/pyramid_card_indices");
        WriteSparseArray(archive, states.Select(s => s.DiscardedCardIndices).ToList(), "sparse/discarded_card_indices");
    }

    private static void WriteSparseArray(ZipArchive archive, List<ushort[]> arrays, string basePath)
    {
        // Write as object array (pickle format or structured)
        // For simplicity, write each array individually with count metadata
        var counts = arrays.Select(a => a.Length).ToArray();
        var countData = WriteNpyArray(counts, "i4");
        
        var countEntry = archive.CreateEntry($"{basePath}_counts.npy", CompressionLevel.Optimal);
        using (var stream = countEntry.Open())
        {
            stream.Write(countData, 0, countData.Length);
        }

        // Concatenate all indices with offset information
        var allIndices = new List<ushort>();
        foreach (var arr in arrays)
        {
            allIndices.AddRange(arr);
        }

        // Always write indices file (may be empty)
        var indexData = WriteNpyArray(allIndices.ToArray(), "u2"); // uint16
        var indicesEntry = archive.CreateEntry($"{basePath}.npy", CompressionLevel.Optimal);
        using (var stream = indicesEntry.Open())
        {
            stream.Write(indexData, 0, indexData.Length);
        }
    }

    private static void WriteActionData(ZipArchive archive, List<BinaryEncodingResult> states, List<MatchResult> matches)
    {
        var allActions = new List<int>();
        var allActionMasks = new List<float>();

        int stateIdx = 0;
        foreach (var match in matches)
        {
            if (match.Moves == null)
                continue;

            foreach (var move in match.Moves)
            {
                allActions.Add(move.ActionIndex);
                if (move.ActionMask != null)
                    allActionMasks.AddRange(move.ActionMask);
                stateIdx++;
            }
        }

        // Always write action indices and masks (may be empty)
        var actionData = WriteNpyArray(allActions.ToArray(), "i4");
        var actionEntry = archive.CreateEntry("actions/action_indices.npy", CompressionLevel.Optimal);
        using (var stream = actionEntry.Open())
        {
            stream.Write(actionData, 0, actionData.Length);
        }

        var maskDataAll = WriteNpyArray(allActionMasks.ToArray(), "f4");
        var maskEntry = archive.CreateEntry("actions/action_masks.npy", CompressionLevel.Optimal);
        using (var stream = maskEntry.Open())
        {
            stream.Write(maskDataAll, 0, maskDataAll.Length);
        }
    }

    private static void WriteTargetData(ZipArchive archive, List<float> policyTargets, List<float> valueTargets)
    {
        var policyData = WriteNpyArray(policyTargets.ToArray(), "f4");
        var policyEntry = archive.CreateEntry("targets/policy_targets.npy", CompressionLevel.Optimal);
        using (var stream = policyEntry.Open())
        {
            stream.Write(policyData, 0, policyData.Length);
        }

        var valueData = WriteNpyArray(valueTargets.ToArray(), "f4");
        var valueEntry = archive.CreateEntry("targets/value_targets.npy", CompressionLevel.Optimal);
        using (var stream = valueEntry.Open())
        {
            stream.Write(valueData, 0, valueData.Length);
        }
    }

    private static void WriteMetadata(ZipArchive archive, int matchCount, int stateCount)
    {
        var metadata = new StringBuilder();
        metadata.AppendLine("Matches,States,Encoding");
        metadata.AppendLine($"{matchCount},{stateCount},binary_optimized");
        
        var metadataEntry = archive.CreateEntry("metadata.csv");
        using (var stream = metadataEntry.Open())
        using (var writer = new StreamWriter(stream))
        {
            writer.Write(metadata.ToString());
        }
    }

    /// <summary>
    /// Writes a simple NPY file header for numpy array format.
    /// Reference: https://numpy.org/doc/stable/reference/generated/numpy.lib.format.html
    /// </summary>
    private static byte[] WriteNpyArray<T>(T[] array, string dtype) where T : unmanaged
    {
        var header = GenerateNpyHeader(array.Length, dtype);
        var headerBytes = Encoding.ASCII.GetBytes(header);
        
        var buffer = new MemoryStream();
        
        // NPY magic string
        buffer.Write(new byte[] { 0x93, (byte)'N', (byte)'U', (byte)'M', (byte)'P', (byte)'Y' }, 0, 6);
        
        // Version (1.0)
        buffer.WriteByte(1);
        buffer.WriteByte(0);
        
        // Header length (little-endian uint16)
        ushort headerLen = (ushort)headerBytes.Length;
        buffer.Write(BitConverter.GetBytes(headerLen), 0, 2);
        
        // Header
        buffer.Write(headerBytes, 0, headerBytes.Length);
        
        // Data (convert to bytes)
        var handle = System.Runtime.InteropServices.GCHandle.Alloc(array, System.Runtime.InteropServices.GCHandleType.Pinned);
        try
        {
            var ptr = handle.AddrOfPinnedObject();
            int byteCount = array.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
            byte[] data = new byte[byteCount];
            System.Runtime.InteropServices.Marshal.Copy(ptr, data, 0, byteCount);
            buffer.Write(data, 0, byteCount);
        }
        finally
        {
            handle.Free();
        }
        
        return buffer.ToArray();
    }

    private static string GenerateNpyHeader(int arrayLength, string dtype)
    {
        return $"{{'descr': '{dtype}', 'fortran_order': False, 'shape': ({arrayLength},), }}\n";
    }
}

public class BinaryMatchData
{
    public string MatchId { get; set; } = string.Empty;
    public string Agent1 { get; set; } = string.Empty;
    public string Agent2 { get; set; } = string.Empty;
    public string? Winner { get; set; }
    public int Seed { get; set; }
    public int Turns { get; set; }
    public int MoveCount { get; set; }
    public List<BinaryEncodingResult> EncodedStates { get; set; } = new List<BinaryEncodingResult>();
}
