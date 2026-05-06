using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Definiuje stala przestrzen akcji dla modelu RL.
/// - Glowna akcja: (20 slotow × 6 akcji) = 120 wymiarow
/// - Subdecyzje: stale rozmiary tensorow dla kazdego typu efektu
/// </summary>
public static class ActionSpace
{
    public const int StateVectorSize = 1903;

    // Wymiary glownej przestrzeni akcji
    public const int BoardSlots = 20;
    public const int ActionsPerSlot = 6; // ZbudujKarte, OdrzucKarte, ZbudujCud (4 warianty cudu)
    public const int TotalPrimaryActions = BoardSlots * ActionsPerSlot; // 120

    // Wymiary subdecyzji (fixed tensors)
    public const int NumProgressTokens = 10;
    public const int NumCards = 73; // 23+23+27 karty z trzech epok + gildie
    public const int NumWonders = 12;
    public const int NumPlayers = 2;

    /// <summary>
    /// Generuje katalog nazw akcji: "slot_00:ZbudujKarte", "slot_00:OdrzucKarte", itd.
    /// </summary>
    public static readonly string[] ActionNames = GenerateActionCatalog();

    private static string[] GenerateActionCatalog()
    {
        var actions = new List<string>();

        for (int slot = 0; slot < BoardSlots; slot++)
        {
            actions.Add($"slot_{slot:00}:ZbudujKarte");
            actions.Add($"slot_{slot:00}:OdrzucKarte");

            for (int wonderIdx = 0; wonderIdx < 4; wonderIdx++)
            {
                actions.Add($"slot_{slot:00}:ZbudujCud_{wonderIdx}");
            }
        }

        return actions.ToArray();
    }

    /// <summary>
    /// Mapuje TypRuchu + indeks cudu na indeks w ActionMask (0-119).
    /// </summary>
    public static int GetActionIndex(int slotIndex, TypRuchu typRuchu, int wonderIndex = -1)
    {
        if (slotIndex < 0 || slotIndex >= BoardSlots)
            return -1;

        return typRuchu switch
        {
            TypRuchu.ZbudujKarte => slotIndex * ActionsPerSlot + 0,
            TypRuchu.OdrzucKarte => slotIndex * ActionsPerSlot + 1,
            TypRuchu.ZbudujCud when wonderIndex >= 0 && wonderIndex <= 3 =>
                slotIndex * ActionsPerSlot + 2 + wonderIndex,
            _ => -1
        };
    }

    /// <summary>
    /// Zwraca indeks cudu w liscie cudow gracza (0-3), albo -1 jesli nie znaleziono.
    /// </summary>
    public static int FindWonderIndex(List<KartaCudu> playerWonders, string wonderName)
    {
        if (playerWonders == null || string.IsNullOrEmpty(wonderName))
            return -1;

        for (int i = 0; i < playerWonders.Count && i < 4; i++)
        {
            if (string.Equals(playerWonders[i].Nazwa, wonderName, StringComparison.Ordinal))
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Zlicza strukture ActionMask: ile legalnych akcji w kazdej kategorii.
    /// </summary>
    public struct ActionMaskStats
    {
        public int BuildCardActions { get; set; }
        public int DiscardCardActions { get; set; }
        public int BuildWonderActions { get; set; }
        public int TotalLegalActions { get; set; }
    }

    public static ActionMaskStats AnalyzeMask(float[] mask)
    {
        if (mask == null || mask.Length != TotalPrimaryActions)
            throw new ArgumentException($"ActionMask must be {TotalPrimaryActions} elements");

        var stats = new ActionMaskStats();

        for (int slot = 0; slot < BoardSlots; slot++)
        {
            // Index 0: ZbudujKarte
            if (mask[slot * ActionsPerSlot + 0] > 0.5f)
                stats.BuildCardActions++;

            // Index 1: OdrzucKarte
            if (mask[slot * ActionsPerSlot + 1] > 0.5f)
                stats.DiscardCardActions++;

            // Indeksy 2-5: ZbudujCud_0-3
            for (int w = 0; w < 4; w++)
            {
                if (mask[slot * ActionsPerSlot + 2 + w] > 0.5f)
                    stats.BuildWonderActions++;
            }
        }

        stats.TotalLegalActions = stats.BuildCardActions + stats.DiscardCardActions + stats.BuildWonderActions;
        return stats;
    }
}

/// <summary>
/// Definiuje wymiary i nazwy dla wszystkich typow subdecyzji.
/// </summary>
public static class SubdecisionSpace
{
    public enum DecisionType
    {
        Zetony,              // 10-d: wybor jednego z dostepnych tokenow
        ZniszczKarte,        // 73-d: wybor karty do zniszczenia
        ZbudujZOdrzuconych,  // 73-d: wybor karty z odrzuconych
        Inne                 // variable-d: proste tak/nie lub inne
    }

    public static int GetSubdecisionDimension(DecisionType type) => type switch
    {
        DecisionType.Zetony => ActionSpace.NumProgressTokens,
        DecisionType.ZniszczKarte => ActionSpace.NumCards,
        DecisionType.ZbudujZOdrzuconych => ActionSpace.NumCards,
        DecisionType.Inne => -1, // Variable, zalezy od kontekstu
        _ => throw new ArgumentException($"Unknown DecisionType: {type}")
    };

    public static DecisionType MapEffectType(TypEfektu efekt) => efekt switch
    {
        TypEfektu.WybierzZetonPostepu => DecisionType.Zetony,
        TypEfektu.Wylosuj3ZetonyPostepu => DecisionType.Zetony,
        TypEfektu.OdlozKartePrzeciwnika => DecisionType.ZniszczKarte,
        TypEfektu.DarmowaBudowlaZOdrzuconychKart => DecisionType.ZbudujZOdrzuconych,
        _ => DecisionType.Inne
    };
}
