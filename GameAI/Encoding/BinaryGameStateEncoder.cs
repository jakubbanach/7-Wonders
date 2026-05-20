using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Optimized binary encoding of game states using:
/// - Bit-packing for boolean values (8 bools per byte)
/// - Sparse arrays for large binary structures (cards, wonders, tokens)
/// - Efficient representation for neural network training
/// </summary>
public static class BinaryGameStateEncoder
{
    private const int MaxBoardSlots = 20;
    private const float MaxCoins = 100f;
    private const float MaxVictoryPoints = 100f;
    private const float MaxResources = 10f;
    private const float MaxScienceSymbols = 2f;

    // Counts for sparse array optimization
    private const int NumCards = 73;
    private const int NumWonders = 12;
    private const int NumProgressTokens = 10;

    /// <summary>
    /// Encodes game state in optimized binary format.
    /// Returns structure containing:
    /// - packed_data: Continuous float values + bit-packed booleans
    /// - card_indices_p1: Sparse card indices for player 1
    /// - card_indices_p2: Sparse card indices for player 2
    /// - wonder_indices_p1/p2: Sparse wonder indices
    /// - token_indices_p1/p2: Sparse token indices
    /// - available_tokens: Sparse available progress token indices
    /// </summary>
    public static BinaryEncodingResult EncodeBinary(Gra gra)
    {
        if (gra == null)
            throw new ArgumentNullException(nameof(gra));

        var result = new BinaryEncodingResult();
        var buffer = new List<float>(256);
        var boolBuffer = new List<bool>(128);

        // Global state with sparse card/wonder/token indices
        EncodeGlobalStateOptimized(gra, buffer, boolBuffer, result);
        EncodePlayersOptimized(gra, buffer, boolBuffer, result);
        EncodePyramidOptimized(gra, buffer, boolBuffer, result);
        EncodeDiscardOptimized(gra, result);
        EncodeActionMaskOptimized(gra, result);

        // Pack all continuous data
        result.PackedData = buffer.ToArray();
        
        // Bit-pack booleans (8 per byte)
        result.PackedBooleans = BitPackBooleans(boolBuffer);

        return result;
    }

    private static void EncodeGlobalStateOptimized(Gra gra, List<float> buffer, List<bool> boolBuffer, BinaryEncodingResult result)
    {
        var aktywnyGraczIndex = Array.IndexOf(gra.Gracze, gra.AktywnyGracz);
        boolBuffer.Add(aktywnyGraczIndex == 0);
        boolBuffer.Add(aktywnyGraczIndex == 1);

        // Epoch: 3 booleans
        boolBuffer.Add(gra.Epoka == Epoka.EpokaI);
        boolBuffer.Add(gra.Epoka == Epoka.EpokaII);
        boolBuffer.Add(gra.Epoka == Epoka.EpokaIII);

        // Conflict position (normalized float)
        buffer.Add(Normalize(gra.PlanszaKonfliktu.PionKonfliktu.PobierzPozycje() + 9, 18));
        
        // Game state: 5 booleans
        boolBuffer.Add(gra.StanGry.CzyZakonczona);
        boolBuffer.Add(gra.StanGry.TypZwyciestwa == TypZwyciestwa.Brak);
        boolBuffer.Add(gra.StanGry.TypZwyciestwa == TypZwyciestwa.Militarne);
        boolBuffer.Add(gra.StanGry.TypZwyciestwa == TypZwyciestwa.Naukowe);
        boolBuffer.Add(gra.StanGry.TypZwyciestwa == TypZwyciestwa.Punktowe);

        // Conflict zones (9 zones): 2 values each = 18 floats + 9 bools
        foreach (var strefa in gra.PlanszaKonfliktu.Strefy)
        {
            boolBuffer.Add(strefa.CzyJuzUzyta);
            buffer.Add(Normalize(strefa.LiczbaTraconychMonet, 10));
            buffer.Add(Normalize(strefa.LiczbaPunktow, 10));
        }

        // Available progress tokens: sparse
        var availableTokens = gra.PlanszaKonfliktu.ZetonyPostepu
            .Select((z, idx) => (z.Nazwa, idx))
            .ToList();
        result.AvailableProgressTokenIndices = availableTokens
            .Select(t => (ushort)t.idx)
            .ToArray();
    }

    private static void EncodePlayersOptimized(Gra gra, List<float> buffer, List<bool> boolBuffer, BinaryEncodingResult result)
    {
        EncodePlayerOptimized(gra.AktywnyGracz, buffer, boolBuffer, result, isPlayer1: true);
        EncodePlayerOptimized(gra.Przeciwnik, buffer, boolBuffer, result, isPlayer1: false);
    }

    private static void EncodePlayerOptimized(Gracz gracz, List<float> buffer, List<bool> boolBuffer, BinaryEncodingResult result, bool isPlayer1)
    {
        buffer.Add(Normalize(gracz.Monety(), MaxCoins));
        buffer.Add(Normalize(gracz.PunktyZwyciestwa, MaxVictoryPoints));

        // Resources: 6 values (skip Monety enum value)
        foreach (Surowiec surowiec in Enum.GetValues(typeof(Surowiec)))
        {
            if (surowiec == Surowiec.Monety)
                continue;
            buffer.Add(Normalize(gracz.WypiszLiczbeSurowca(surowiec), MaxResources));
        }

        // Science symbols: 5 values
        foreach (SymbolNaukowy symbol in Enum.GetValues(typeof(SymbolNaukowy)))
        {
            buffer.Add(Normalize(gracz.SymboleNaukowe.Count(s => s == symbol), MaxScienceSymbols));
        }

        // Cards: SPARSE - collect indices of built cards
        var builtCardIndices = gracz.ZbudowaneKarty
            .Select(karta => GetCardIndex(karta.Nazwa))
            .Where(idx => idx >= 0)
            .Cast<ushort>()
            .ToArray();

        if (isPlayer1)
            result.CardIndicesP1 = builtCardIndices;
        else
            result.CardIndicesP2 = builtCardIndices;

        // Wonders: SPARSE - collect owned and built wonder indices
        var ownedWonderIndices = new List<ushort>();
        var builtWonderIndices = new List<ushort>();

        foreach (var kartaCudu in gracz.KartyCudow)
        {
            var idx = GetWonderIndex(kartaCudu.Nazwa);
            if (idx >= 0)
            {
                ownedWonderIndices.Add((ushort)idx);
                if (kartaCudu.CzyZagrana)
                    builtWonderIndices.Add((ushort)idx);
            }
        }

        if (isPlayer1)
        {
            result.OwnedWonderIndicesP1 = ownedWonderIndices.ToArray();
            result.BuiltWonderIndicesP1 = builtWonderIndices.ToArray();
        }
        else
        {
            result.OwnedWonderIndicesP2 = ownedWonderIndices.ToArray();
            result.BuiltWonderIndicesP2 = builtWonderIndices.ToArray();
        }

        // Progress tokens: SPARSE
        var tokenIndices = gracz.ZetonyPostepu
            .Select(zeton => GetProgressTokenIndex(zeton.Nazwa))
            .Where(idx => idx >= 0)
            .Cast<ushort>()
            .ToArray();

        if (isPlayer1)
            result.ProgressTokenIndicesP1 = tokenIndices;
        else
            result.ProgressTokenIndicesP2 = tokenIndices;
    }

    private static void EncodePyramidOptimized(Gra gra, List<float> buffer, List<bool> boolBuffer, BinaryEncodingResult result)
    {
        var pola = gra.PlanszaEpoki.Pola;
        var cardIndices = new List<ushort>();

        for (int i = 0; i < MaxBoardSlots; i++)
        {
            if (i >= pola.Count)
            {
                // Empty slot: 3 bools + cost + sparse indicator
                boolBuffer.Add(false); // karta != null
                boolBuffer.Add(false); // nie zakryta
                boolBuffer.Add(false); // nie dostepna
                buffer.Add(0f);        // koszt
                continue;
            }

            var pole = pola[i];
            boolBuffer.Add(pole.Karta != null);
            boolBuffer.Add(pole.CzyZakryta);
            boolBuffer.Add(pole.CzyDostepna);

            if (pole.Karta != null && !pole.CzyZakryta)
            {
                int koszt = pole.Karta.ObliczKoszt(gra.AktywnyGracz, gra.Przeciwnik);
                buffer.Add(Normalize(koszt, MaxCoins));

                // Card identity: add to sparse indices if visible
                var cardIdx = GetCardIndex(pole.Karta.Nazwa);
                if (cardIdx >= 0)
                    cardIndices.Add((ushort)cardIdx);
            }
            else
            {
                buffer.Add(0f);
            }
        }

        result.PyramidCardIndices = cardIndices.ToArray();
    }

    private static void EncodeActionMaskOptimized(Gra gra, BinaryEncodingResult result)
    {
        var mask = new float[ActionSpace.TotalPrimaryActions];
        var pola = gra.PlanszaEpoki.Pola;

        foreach (var ruch in gra.DostepneRuchy())
        {
            var actionIndex = GameStateEncoder.GetActionIndex(gra, ruch);
            if (actionIndex >= 0)
                mask[actionIndex] = 1f;
        }

        // Sparse representation: store only indices where mask == 1
        var legalActionIndices = new List<ushort>();
        for (int i = 0; i < mask.Length; i++)
        {
            if (mask[i] > 0.5f)
                legalActionIndices.Add((ushort)i);
        }

        result.LegalActionIndices = legalActionIndices.ToArray();
        result.ActionMaskDense = mask;
    }

    private static void EncodeDiscardOptimized(Gra gra, BinaryEncodingResult result)
    {
        var discardedCardIndices = gra.StosKartOdrzuconych
            .Select(karta => GetCardIndex(karta.Nazwa))
            .Where(idx => idx >= 0)
            .Cast<ushort>()
            .ToArray();

        result.DiscardedCardIndices = discardedCardIndices;
    }

    // Helper methods for sparse index lookups
    private static int GetCardIndex(string cardName)
    {
        var cardCatalog = ZbiorKart.TaliaEpokiI
            .Concat(ZbiorKart.TaliaEpokiII)
            .Concat(ZbiorKart.TaliaEpokiIII)
            .Select(k => k.Nazwa)
            .ToArray();

        return Array.FindIndex(cardCatalog, k => string.Equals(k, cardName, StringComparison.Ordinal));
    }

    private static int GetWonderIndex(string wonderName)
    {
        var wonderCatalog = ZbiorKart.TaliaKartyCudow
            .Select(k => k.Nazwa)
            .ToArray();

        return Array.FindIndex(wonderCatalog, k => string.Equals(k, wonderName, StringComparison.Ordinal));
    }

    private static int GetProgressTokenIndex(string tokenName)
    {
        var tokenCatalog = ZbiorZetonowPostepu.ZetonyPostepu
            .Select(z => z.Nazwa)
            .ToArray();

        return Array.FindIndex(tokenCatalog, t => string.Equals(t, tokenName, StringComparison.Ordinal));
    }

    private static byte[] BitPackBooleans(List<bool> booleans)
    {
        var byteCount = (booleans.Count + 7) / 8;
        var packed = new byte[byteCount];

        for (int i = 0; i < booleans.Count; i++)
        {
            if (booleans[i])
            {
                int byteIndex = i / 8;
                int bitIndex = i % 8;
                packed[byteIndex] |= (byte)(1 << bitIndex);
            }
        }

        return packed;
    }

    private static float Normalize(int value, float maxValue)
    {
        if (maxValue <= 0f)
            return 0f;
        if (value <= 0)
            return 0f;
        return Math.Min(value, maxValue) / maxValue;
    }
}

/// <summary>
/// Result of binary game state encoding containing:
/// - Continuous data (floats)
/// - Bit-packed booleans
/// - Sparse indices for cards, wonders, tokens, actions
/// </summary>
public class BinaryEncodingResult
{
    /// Continuous floating-point values
    public float[] PackedData { get; set; } = Array.Empty<float>();

    // Bit-packed booleans (8 per byte)
    public byte[] PackedBooleans { get; set; } = Array.Empty<byte>();

    // Sparse card indices
    public ushort[] CardIndicesP1 { get; set; } = Array.Empty<ushort>();
    public ushort[] CardIndicesP2 { get; set; } = Array.Empty<ushort>();

    // Sparse wonder indices (owned and built separately for each player)
    public ushort[] OwnedWonderIndicesP1 { get; set; } = Array.Empty<ushort>();
    public ushort[] BuiltWonderIndicesP1 { get; set; } = Array.Empty<ushort>();
    public ushort[] OwnedWonderIndicesP2 { get; set; } = Array.Empty<ushort>();
    public ushort[] BuiltWonderIndicesP2 { get; set; } = Array.Empty<ushort>();

    // Sparse progress token indices
    public ushort[] ProgressTokenIndicesP1 { get; set; } = Array.Empty<ushort>();
    public ushort[] ProgressTokenIndicesP2 { get; set; } = Array.Empty<ushort>();
    public ushort[] AvailableProgressTokenIndices { get; set; } = Array.Empty<ushort>();

    // Sparse pyramid card indices (what cards are on board)
    public ushort[] PyramidCardIndices { get; set; } = Array.Empty<ushort>();

    // Sparse discarded card indices
    public ushort[] DiscardedCardIndices { get; set; } = Array.Empty<ushort>();

    // Action mask can be dense or sparse
    public float[] ActionMaskDense { get; set; } = Array.Empty<float>();
    public ushort[] LegalActionIndices { get; set; } = Array.Empty<ushort>();
}
