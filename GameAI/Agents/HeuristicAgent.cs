using System;
using System.Collections.Generic;
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

        var najlepszyRuch = ruchy
            .OrderByDescending(r => OcenRuch(gra, r, aktywnyGracz))
            .First();
        return najlepszyRuch;
    }
    public T WybierzAkcjePosrednia<T>(Gra gra, DecyzjaKontekst<T> decyzja)
    {
        return decyzja.Opcje
            .OrderByDescending(o => OcenOpcje(gra, decyzja, o))
            .First();
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
    private double OcenStan(Gra gra, Gracz aktywnyGracz)
    {
        var gracz = gra.Gracze.First(g => g.Nazwa == aktywnyGracz.Nazwa);
        var przeciwnik = gra.Gracze.First(g => g != gracz);

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
        if (gracz.Nazwa == gra.Gracze[0].Nazwa)
        {
            score += pionKonfliktu * weights.Wojsko;
        }
        else
        {
            score -= pionKonfliktu * weights.Wojsko;
        }
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