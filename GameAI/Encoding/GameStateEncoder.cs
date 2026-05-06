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

    public static GamePolicyEncoding EncodePolicy(Gra gra)
    {
        if (gra == null)
            throw new ArgumentNullException(nameof(gra));

        return new GamePolicyEncoding(
            Encode(gra),
            ActionSpace.ActionNames,
            EncodeActionMask(gra));
    }

    public static int GetActionIndex(Gra gra, Ruch ruch)
    {
        if (gra == null)
            throw new ArgumentNullException(nameof(gra));
        if (ruch == null)
            throw new ArgumentNullException(nameof(ruch));

        var slotIndex = ZnajdzIndeksPola(gra.PlanszaEpoki.Pola, ruch.KartaDoZagrania);
        if (slotIndex < 0)
            return -1;

        if (ruch.TypRuchu == TypRuchu.ZbudujCud && ruch.KartaCudu != null)
        {
            var wonderIdx = ActionSpace.FindWonderIndex(gra.AktywnyGracz.KartyCudow, ruch.KartaCudu.Nazwa);
            return wonderIdx >= 0 ? ActionSpace.GetActionIndex(slotIndex, ruch.TypRuchu, wonderIdx) : -1;
        }

        return ActionSpace.GetActionIndex(slotIndex, ruch.TypRuchu);
    }

    /// <summary>
    /// Koduje subdecyzje dla danego typu efektu z ustalona dimensionalnoscia.
    /// </summary>
    public static DecisionEncoding EncodeSubdecision(Gra gra, TypEfektu efekt)
    {
        if (efekt == TypEfektu.WybierzZetonPostepu || efekt == TypEfektu.Wylosuj3ZetonyPostepu)
        {
            return EncodeTokenChoice(gra);
        }

        if (efekt == TypEfektu.OdlozKartePrzeciwnika)
            return EncodeEnemyCardChoice(gra);

        if (efekt == TypEfektu.DarmowaBudowlaZOdrzuconychKart)
            return EncodeDiscardedCardChoice(gra);

        if (efekt == TypEfektu.WybierzGraczaRozpoczynajacegoEpoke)
            return EncodePlayerChoice(gra);

        return EncodeBinaryChoice(Array.Empty<string>());
    }

    private static DecisionEncoding EncodePlayerChoice(Gra gra)
    {
        var options = gra.Gracze.Select(gracz => gracz.Nazwa).ToArray();
        if (options.Length == 0)
            options = Array.Empty<string>();

        var legalMask = new float[ActionSpace.NumPlayers];
        for (int i = 0; i < options.Length && i < legalMask.Length; i++)
            legalMask[i] = 1f;

        return new DecisionEncoding("WybierzGraczaRozpoczynajacegoEpoke", options, legalMask, null);
    }

    private static DecisionEncoding EncodeTokenChoice(Gra gra)
    {
        float[] legalMask = new float[ActionSpace.NumProgressTokens];
        var dostepneWGrze = gra.PlanszaKonfliktu.ZetonyPostepu.Select(z => z.Nazwa).ToHashSet();

        for (int i = 0; i < ProgressTokenCatalog.Length; i++)
            legalMask[i] = dostepneWGrze.Contains(ProgressTokenCatalog[i]) ? 1f : 0f;

        return new DecisionEncoding("WybierzZeton", ProgressTokenCatalog, legalMask, null);
    }

    private static DecisionEncoding EncodeEnemyCardChoice(Gra gra)
    {
        float[] legalMask = new float[ActionSpace.NumCards];
        var kartyPrzeciwnika = gra.Przeciwnik.ZbudowaneKarty.Select(k => k.Nazwa).ToHashSet();

        for (int i = 0; i < CardCatalog.Length; i++)
            legalMask[i] = kartyPrzeciwnika.Contains(CardCatalog[i]) ? 1f : 0f;

        return new DecisionEncoding("ZniszczKarte", CardCatalog, legalMask, null);
    }

    private static DecisionEncoding EncodeDiscardedCardChoice(Gra gra)
    {
        float[] legalMask = new float[ActionSpace.NumCards];
        var odrzucone = gra.StosKartOdrzuconych.Select(k => k.Nazwa).ToHashSet();

        for (int i = 0; i < CardCatalog.Length; i++)
            legalMask[i] = odrzucone.Contains(CardCatalog[i]) ? 1f : 0f;

        return new DecisionEncoding("ZbudujZOdrzuconych", CardCatalog, legalMask, null);
    }

    private static DecisionEncoding EncodeBinaryChoice(string[] options)
    {
        float[] legalMask = CreateFilledMask(options.Length);
        return new DecisionEncoding("Inne", options, legalMask, null);
    }

    public static DecisionEncoding EncodeDecision<T>(DecyzjaKontekst<T> decyzja)
    {
        if (decyzja == null)
            throw new ArgumentNullException(nameof(decyzja));

        var options = decyzja.Opcje
            .Select(opcja => opcja?.ToString() ?? string.Empty)
            .ToArray();

        return new DecisionEncoding(
            decyzja.Efekt.ToString(),
            options,
            CreateFilledMask(options.Length),
            null);
    }

    public static DecisionEncoding EncodeDecision(DecisionLog decyzja)
    {
        if (decyzja == null)
            throw new ArgumentNullException(nameof(decyzja));

        var options = decyzja.Opcje.ToArray();
        var choiceMask = new float[options.Length];
        var chosenIndex = Array.FindIndex(options, option => string.Equals(option, decyzja.Wybor, StringComparison.Ordinal));

        if (chosenIndex >= 0)
        {
            choiceMask[chosenIndex] = 1f;
        }

        return new DecisionEncoding(
            decyzja.TypDecyzji,
            options,
            CreateFilledMask(options.Length),
            choiceMask);
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
        var kartaNazwa = karta?.Nazwa;
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
            buffer.Add(kartaWidoczna && string.Equals(kartaNazwa, cardName, StringComparison.Ordinal) ? 1f : 0f);
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

    private static float[] EncodeActionMask(Gra gra)
    {
        var mask = new float[ActionSpace.TotalPrimaryActions];
        var pola = gra.PlanszaEpoki.Pola;

        foreach (var ruch in gra.DostepneRuchy())
        {
            var actionIndex = GetActionIndex(gra, ruch);

            if (actionIndex >= 0)
                mask[actionIndex] = 1f;
        }

        return mask;
    }

    private static int ZnajdzIndeksPola(IReadOnlyList<PoleKarty> pola, Karta karta)
    {
        for (int i = 0; i < pola.Count; i++)
        {
            var pole = pola[i];
            if (pole.Karta == null)
            {
                continue;
            }

            if (ReferenceEquals(pole.Karta, karta) || string.Equals(pole.Karta.Nazwa, karta.Nazwa, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }

    private static float[] CreateFilledMask(int length)
    {
        var mask = new float[length];

        for (int i = 0; i < length; i++)
        {
            mask[i] = 1f;
        }

        return mask;
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