using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

public class EvolutionManager
{
    public List<HeuristicWeights> Populacja { get; private set; } = new List<HeuristicWeights>();
    private readonly IRandom _rng;
    private HeuristicWeights _bestEver;
    private const int TreningowNaGen = 20;

    public EvolutionManager(IRandom rng, int popSize = 30)
    {
        _rng = rng;
        for (int i = 0; i < popSize; i++)
            Populacja.Add(GenerateRandomWeights());
    }

    private HeuristicWeights GenerateRandomWeights() => new HeuristicWeights
    {
        PunktyZwyciestwa = 10.0,
        Wojsko = _rng.NextDouble(1, 15),
        Monety = _rng.NextDouble(1, 15),
        Cuda = _rng.NextDouble(1, 15),
        SymboleNaukowe = _rng.NextDouble(1, 15),
        SurowceBrazowe = _rng.NextDouble(1, 15),
        SurowceSzare = _rng.NextDouble(1, 15),
        SynergiaGildii = _rng.NextDouble(1, 15),
        MonopolBonus = _rng.NextDouble(1, 15)
    };

    public void UruchomEwolucje(int generacje)
    {
        for (int g = 0; g < generacje; g++)
        {
            var wyniki = new List<(HeuristicWeights w, int wins)>();

            foreach (var wagi in Populacja)
            {
                int wygrane = Trenuj(wagi);
                wyniki.Add((wagi, wygrane));
            }

            // Selekcja i Mutacja
            var posortowane = wyniki.OrderByDescending(x => x.wins).ToList();
            if (posortowane[0].wins > (TreningowNaGen)) // Jeśli najlepszy ma więcej niż 50% wygranych, aktualizujemy bestEver
                _bestEver = posortowane[0].w;

            Console.WriteLine($"G:{g} | Best Wins: {posortowane[0].wins}/{TreningowNaGen*2} | PV: {_bestEver.PunktyZwyciestwa:F2} " +
                $"Mil: {_bestEver.Wojsko:F2} Mon: {_bestEver.Monety:F2} Cuda: {_bestEver.Cuda:F2} Symb: {_bestEver.SymboleNaukowe:F2} " +
                $"Braz: {_bestEver.SurowceBrazowe:F2} Szare: {_bestEver.SurowceSzare:F2} Synergia: {_bestEver.SynergiaGildii:F2} Monopol: {_bestEver.MonopolBonus:F2}");

            // Nowa populacja: Top 2 zostaje, reszta to zmutowane kopie top 2
            var nastepnaPopulacja = new List<HeuristicWeights> { posortowane[0].w, posortowane[1].w };
            while (nastepnaPopulacja.Count < Populacja.Count)
            {
                nastepnaPopulacja.Add(Mutuj(posortowane[0].w));
            }
            Populacja = nastepnaPopulacja;
        }
        SaveBest();
    }

    private int Trenuj(HeuristicWeights wagi)
    {
        int wins = 0;
        for (int i = 0; i < TreningowNaGen; i++)
        {
            // Przeciwnik: Jesli mamy BestEver, graj z nim, inaczej z Randomem
            Func<IRandom, IAgent> przeciwnikFactory;
            if (_bestEver != null)
                przeciwnikFactory = r => new HeuristicAgent(_bestEver, r);
            else
                przeciwnikFactory = r => new RandomAgent(r);

            var rngSeed = _rng.Next();
            var runner = new GameRunner(
                rngSeed,
                r => new HeuristicAgent(wagi, r),
                przeciwnikFactory
            );

            var res = runner.PlayGame(SimulationMode.Tournament);
            if (res.Winner == res.Agent1Name) wins++;

            var runnerReverse = new GameRunner(
                rngSeed,
                przeciwnikFactory,
                r => new HeuristicAgent(wagi, r)
            );

            var resReverse = runnerReverse.PlayGame(SimulationMode.Tournament);
            // Agent1 to heurystyczny!!!!
            if (resReverse.Winner == resReverse.Agent2Name) wins++;
        }
        return wins;
    }
    double Clamp(double val) => Math.Max(0.0, Math.Min(40.0, val));
    private HeuristicWeights Mutuj(HeuristicWeights baseW) => new HeuristicWeights
    {

        PunktyZwyciestwa = 10.0, // Nie mutujemy PV, bo to jest cel
        Wojsko = Clamp(baseW.Wojsko + (_rng.NextDouble(-2, 2))),
        Monety = Clamp(baseW.Monety + (_rng.NextDouble(-2, 2))),
        Cuda = Clamp(baseW.Cuda + (_rng.NextDouble(-2, 2))),
        SymboleNaukowe = Clamp(baseW.SymboleNaukowe + (_rng.NextDouble(-2, 2))),
        SurowceBrazowe = Clamp(baseW.SurowceBrazowe + (_rng.NextDouble(-2, 2))),
        SurowceSzare = Clamp(baseW.SurowceSzare + (_rng.NextDouble(-2, 2))),
        SynergiaGildii = Clamp(baseW.SynergiaGildii + (_rng.NextDouble(-2, 2))),
        MonopolBonus = Clamp(baseW.MonopolBonus + (_rng.NextDouble(-2, 2)))
    };

    public void SaveBest() 
    {
        var projectDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..");
        var resultsDir = Path.Combine(projectDir, "Simulations");
        Directory.CreateDirectory(resultsDir);
        var fullPath = Path.Combine(resultsDir, "best_heuristic_weights_20_double_new.json");

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(_bestEver, options);
        File.WriteAllText(fullPath, json);
    }
}