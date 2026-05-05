using System;
using System.Collections.Generic;
using System.Linq;

public static class GameStateEncoder
{
    private const int MaxBoardSlots = 20;
    private const float MaxCoins = 100f;
    private const float MaxVictoryPoints = 100f;
    private const float MaxResources = 10f;
    private const float MaxScienceSymbols = 2f;

    private static readonly string[] CardCatalog = ZbiorKart.TaliaEpokiI // 23 karty
        .Concat(ZbiorKart.TaliaEpokiII) // 23 karty
        .Concat(ZbiorKart.TaliaEpokiIII) // 20 kart + 7 kart Gildii = 27 kart
        .Select(karta => karta.Nazwa)
        .ToArray();

    private static readonly string[] WonderCatalog = ZbiorKart.TaliaKartyCudow // 12 kart
        .Select(kartaCudu => kartaCudu.Nazwa)
        .ToArray();

    private static readonly string[] ProgressTokenCatalog = ZbiorZetonowPostepu.ZetonyPostepu // 10 zetonow
        .Select(zeton => zeton.Nazwa)
        .ToArray();

    private static readonly string[] ConflictZoneCatalog = ZbiorStref.Strefy // 9 stref
        .Select(strefa => strefa.Nazwa)
        .ToArray();

    public static float[] Encode(Gra gra)
    {
        if (gra == null)
            throw new ArgumentNullException(nameof(gra));

        var buffer = new List<float>(1024);

        EncodeGlobalState(gra, buffer);
        EncodePlayers(gra, buffer);
        EncodePyramid(gra, buffer);
        EncodeDiscard(gra, buffer);

        return buffer.ToArray();
    }

    private static void EncodeGlobalState(Gra gra, List<float> buffer)
    {
        var aktywnyGraczIndex = Array.IndexOf(gra.Gracze, gra.AktywnyGracz);
        buffer.Add(aktywnyGraczIndex == 0 ? 1f : 0f);
        buffer.Add(aktywnyGraczIndex == 1 ? 1f : 0f);

        EncodeEpoch(gra.Epoka, buffer);

        buffer.Add(Normalize(gra.PlanszaKonfliktu.PionKonfliktu.PobierzPozycje() + 9, 18));
        buffer.Add(gra.StanGry.CzyZakonczona ? 1f : 0f);

        EncodeVictoryType(gra.StanGry.TypZwyciestwa, buffer);
        EncodeConflictZones(gra.PlanszaKonfliktu, buffer);
        EncodeAvailableProgressTokens(gra.PlanszaKonfliktu, buffer);
    }

    private static void EncodeEpoch(Epoka epoka, List<float> buffer)
    {
        buffer.Add(epoka == Epoka.EpokaI ? 1f : 0f);
        buffer.Add(epoka == Epoka.EpokaII ? 1f : 0f);
        buffer.Add(epoka == Epoka.EpokaIII ? 1f : 0f);
    }

    private static void EncodeVictoryType(TypZwyciestwa typZwyciestwa, List<float> buffer)
    {
        buffer.Add(typZwyciestwa == TypZwyciestwa.Brak ? 1f : 0f);
        buffer.Add(typZwyciestwa == TypZwyciestwa.Militarne ? 1f : 0f);
        buffer.Add(typZwyciestwa == TypZwyciestwa.Naukowe ? 1f : 0f);
        buffer.Add(typZwyciestwa == TypZwyciestwa.Punktowe ? 1f : 0f);
    }

    private static void EncodeConflictZones(PlanszaKonfliktu planszaKonfliktu, List<float> buffer)
    {
        foreach (var strefaNazwa in ConflictZoneCatalog)
        {
            var strefa = planszaKonfliktu.Strefy.First(s => s.Nazwa == strefaNazwa);
            buffer.Add(strefa.CzyJuzUzyta ? 1f : 0f);
            buffer.Add(Normalize(strefa.LiczbaTraconychMonet, 10));
            buffer.Add(Normalize(strefa.LiczbaPunktow, 10));
        }
    }

    private static void EncodeAvailableProgressTokens(PlanszaKonfliktu planszaKonfliktu, List<float> buffer)
    {
        var availableTokens = planszaKonfliktu.ZetonyPostepu
            .Select(zeton => zeton.Nazwa)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var tokenName in ProgressTokenCatalog)
        {
            buffer.Add(availableTokens.Contains(tokenName) ? 1f : 0f);
        }
    }

    private static void EncodePlayers(Gra gra, List<float> buffer)
    {
        EncodePlayer(gra.AktywnyGracz, buffer);
        EncodePlayer(gra.Przeciwnik, buffer);
    }

    private static void EncodePlayer(Gracz gracz, List<float> buffer)
    {
        buffer.Add(Normalize(gracz.Monety(), MaxCoins));
        buffer.Add(Normalize(gracz.PunktyZwyciestwa, MaxVictoryPoints));

        EncodeResources(gracz, buffer);
        EncodeScience(gracz, buffer);
        EncodeCards(gracz, buffer);
        EncodeWonders(gracz, buffer);
        EncodeProgressTokens(gracz, buffer);
    }

    private static void EncodeResources(Gracz gracz, List<float> buffer)
    {
        foreach (Surowiec surowiec in Enum.GetValues(typeof(Surowiec)))
        {
            if (surowiec == Surowiec.Monety)
            {
                continue;
            }
            buffer.Add(Normalize(gracz.WypiszLiczbeSurowca(surowiec), MaxResources));
        }
    }

    private static void EncodeScience(Gracz gracz, List<float> buffer)
    {
        foreach (SymbolNaukowy symbol in Enum.GetValues(typeof(SymbolNaukowy)))
        {
            buffer.Add(Normalize(gracz.SymboleNaukowe.Count(s => s == symbol), MaxScienceSymbols));
        }
    }

    private static void EncodeCards(Gracz gracz, List<float> buffer)
    {
        var builtCards = gracz.ZbudowaneKarty
            .Select(karta => karta.Nazwa)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var cardName in CardCatalog)
        {
            buffer.Add(builtCards.Contains(cardName) ? 1f : 0f);
        }
    }

    private static void EncodeWonders(Gracz gracz, List<float> buffer)
    {
        var builtWonders = gracz.KartyCudow
            .Where(kartaCudu => kartaCudu.CzyZagrana)
            .Select(kartaCudu => kartaCudu.Nazwa)
            .ToHashSet(StringComparer.Ordinal);
        
        var ownedWonders = gracz.KartyCudow
            .Select(kartaCudu => kartaCudu.Nazwa)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var wonderName in WonderCatalog)
        {
            buffer.Add(ownedWonders.Contains(wonderName) ? 1f : 0f);
            buffer.Add(builtWonders.Contains(wonderName) ? 1f : 0f);
        }   
    }

    private static void EncodeProgressTokens(Gracz gracz, List<float> buffer)
    {
        var ownedTokens = gracz.ZetonyPostepu
            .Select(zeton => zeton.Nazwa)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var tokenName in ProgressTokenCatalog)
        {
            buffer.Add(ownedTokens.Contains(tokenName) ? 1f : 0f);
        }
    }

    private static void EncodePyramid(Gra gra, List<float> buffer)
    {
        var pola = gra.PlanszaEpoki.Pola;

        //???
        for (int i = 0; i < MaxBoardSlots; i++)
        {
            if (i >= pola.Count)
            {
                EncodeEmptyBoardSlot(buffer);
                continue;
            }

            EncodeBoardSlot(pola[i], gra, buffer);
        }
    }

    private static void EncodeBoardSlot(PoleKarty pole, Gra gra, List<float> buffer)
    {
        var karta = pole.Karta;
        var kartaWidoczna = karta != null && !pole.CzyZakryta;

        buffer.Add(karta != null ? 1f : 0f);
        buffer.Add(pole.CzyZakryta ? 1f : 0f);
        buffer.Add(pole.CzyDostepna ? 1f : 0f);

        if (kartaWidoczna)
        {
            int koszt = karta.ObliczKoszt(gra.AktywnyGracz, gra.Przeciwnik);
            buffer.Add(Normalize(koszt, MaxCoins));
        }
        else
        {
            buffer.Add(0f);
        }

        // kodowanie tylko gdy karta jest widoczna - w przeciwnym razie wszystkie cechy karty to 0
        foreach (var cardName in CardCatalog)
        {
            buffer.Add(kartaWidoczna && string.Equals(karta!.Nazwa, cardName, StringComparison.Ordinal) ? 1f : 0f);
        }
    }

    private static void EncodeEmptyBoardSlot(List<float> buffer)
    {
        // Puste pole planszy - wszystkie cechy 0
        buffer.Add(0f);
        buffer.Add(0f);
        buffer.Add(0f);
        buffer.Add(0f);

        for (int i = 0; i < CardCatalog.Length; i++)
        {
            buffer.Add(0f);
        }
    }

    private static void EncodeDiscard(Gra gra, List<float> buffer)
    {
        var discardedCards = gra.StosKartOdrzuconych
            .Select(karta => karta.Nazwa)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var cardName in CardCatalog)
        {
            buffer.Add(discardedCards.Contains(cardName) ? 1f : 0f);
        }
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