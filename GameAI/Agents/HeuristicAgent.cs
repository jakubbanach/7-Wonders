using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public class HeuristicAgent : IAgent
{
    public string Name { get; set; } = "Heuristic";
    private readonly HeuristicWeights weights;
    private readonly IRandom random;
    public HeuristicAgent(HeuristicWeights heuristicWeights, IRandom random)
    {
        weights = heuristicWeights;
        this.random = random;
    }
    public Ruch WybierzRuch(Gra gra)
    {
        var ruchy = gra.DostepneRuchy();
        var aktywnyGracz = gra.AktywnyGracz;

        //var najlepszyRuch = ruchy
        //    .OrderByDescending(r => OcenRuch(gra, r, aktywnyGracz))
        //    .First();
        var najlepszyRuch = IAgent.WybierzNajlepszyLosowy(ruchy, r => OcenRuch(gra, r, aktywnyGracz), random);
        return najlepszyRuch;
    }
    public T WybierzAkcjePosrednia<T>(Gra gra, DecyzjaKontekst<T> decyzja)
    {
        //return decyzja.Opcje
        //    .OrderByDescending(o => OcenOpcje(gra, decyzja, o))
        //    .First();
        return IAgent.WybierzNajlepszyLosowy(decyzja.Opcje, o => OcenOpcje(gra, decyzja, o), random);
    }
    public double OcenRuch(Gra gra, Ruch ruch, Gracz aktywnyGracz)
    {
        var symulacja = gra.Clone();
        var gracz = symulacja.Gracze.First(g => g.Nazwa == aktywnyGracz.Nazwa);
        var przeciwnik = symulacja.Gracze.First(g => g != gracz);
        var resolver = new SimulationDecisionResolver(this);

        symulacja.WykonajRuch(ruch, resolver, random);
        return OcenStan(symulacja, gracz);
    }
    private double OcenStan(Gra gra, Gracz gracz)
    {
        var przeciwnik = gra.Gracze.First(g => g != gracz);

        var graczRes = gracz.Surowce;
        var przeciwnikRes = przeciwnik.Surowce;

        // Szybki helper do wyciagania wartosci ze slownika
        int GetV(Dictionary<Surowiec, int> d, Surowiec s) => d.GetValueOrDefault(s, 0);

        double score = 0;

        var punktyZwyciestwaDiff = gracz.PunktyZwyciestwa - przeciwnik.PunktyZwyciestwa;
        var kartyCudowDiff = gracz.PobierzZbudowaneKartyCudow().Count - przeciwnik.PobierzZbudowaneKartyCudow().Count;
        var monetyDiff = gracz.Monety() - przeciwnik.Monety();
        var symboleNaukoweDiff = gracz.SymboleNaukowe.Count - przeciwnik.SymboleNaukowe.Count;

        score += punktyZwyciestwaDiff * weights.PunktyZwyciestwa;
        score += kartyCudowDiff * weights.Cuda;
        score += monetyDiff * weights.Monety;
        score += symboleNaukoweDiff * weights.SymboleNaukowe;
        
        var pionKonfliktu = gra.PozycjaKonfliktu;
        double wojskoScore = (gracz == gra.Gracze[0]) ? pionKonfliktu : -pionKonfliktu;
        score += wojskoScore * weights.Wojsko;

        int GetCount(Gracz g, Surowiec s) => g.Surowce.Count(x => x.Key == s);

        var brazowe = new[] { Surowiec.Drewno, Surowiec.Glina, Surowiec.Kamien };
        var szare = new[] { Surowiec.Papirus, Surowiec.Szklo };

        score += brazowe.Sum(s => GetV(graczRes, s) - GetV(przeciwnikRes, s)) * weights.SurowceBrazowe;
        score += szare.Sum(s => GetV(graczRes, s) - GetV(przeciwnikRes, s)) * weights.SurowceSzare;

        // Monopol
        score += brazowe.Concat(szare).Count(s => GetV(graczRes, s) > 0 && GetV(przeciwnikRes, s) == 0) * weights.MonopolBonus;

        var gildieDiff = (gracz.PobierzZbudowaneKarty().Count(k => k.KolorKarty == KolorKarty.Fioletowy) -
                        przeciwnik.PobierzZbudowaneKarty().Count(k => k.KolorKarty == KolorKarty.Fioletowy));

        score += gildieDiff * weights.SynergiaGildii;

        return score;
    }
    public double OcenOpcje<T>(Gra gra, DecyzjaKontekst<T> decyzja, T opcja)
    {
        var symulacja = gra.Clone();
        var gracz = symulacja.AktywnyGracz;
        var przeciwnik = symulacja.Gracze.First(g => g != gracz);

        switch (decyzja.Efekt)
        {
            case TypEfektu.DarmowaBudowlaZOdrzuconychKart:
                var karta = (Karta)(object)opcja;
                foreach (var efekt in karta.Efekty)
                {
                    efekt.ZastosujEfekt(gracz, przeciwnik, symulacja.PlanszaKonfliktu, karta, symulacja,decisionResolver: decyzja.resolver);
                    gracz.DodajEfekt(efekt);
                }
                break;
            case TypEfektu.Wylosuj3ZetonyPostepu:
            case TypEfektu.WybierzZetonPostepu:
                var wynik = WybierzZetonHeurystykaZeusAI((ZetonPostepu)(object)opcja);
                return wynik;
            case TypEfektu.OdlozKartePrzeciwnika:
                przeciwnik.UsunKarte((Karta)(object)opcja);
                break;
            case TypEfektu.WybierzGraczaRozpoczynajacegoEpoke:
                return 0; // TODO: zobaczyc jak to ocenic heurystycznie
        }
        return OcenStan(symulacja, gracz);
    }
    public int WybierzZetonHeurystykaZeusAI(ZetonPostepu opcje)
    {
        var zetonyZeusAI = new List<string> { "Teologia", "Prawo", "Strategia", "Rolnictwo", "Filozofia", 
                                    "Ekonomia", "Urbanistyka", "Architektura", "Matematyka", "Budownictwo" };
        return 10-zetonyZeusAI.IndexOf(opcje.Nazwa);
    }
}