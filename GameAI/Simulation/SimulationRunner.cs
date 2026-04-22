using System;
using System.Collections.Generic;
using System.Linq;

public class SimulationRunner
{
    private readonly Func<IRandom, IAgent> agent1Factory;
    private readonly Func<IRandom, IAgent> agent2Factory;
    private readonly int games;
    private readonly int seed;
    private readonly SimulationMode mode;

    public SimulationRunner(
        int seed,
        int games,
        SimulationMode mode,
        Func<IRandom, IAgent> agent1Factory,
        Func<IRandom, IAgent> agent2Factory)
    {
        this.seed = seed;
        this.games = games;
        this.mode = mode;
        this.agent1Factory = agent1Factory;
        this.agent2Factory = agent2Factory;
    }

    public SimulationResult Run()
    {
        var results = new List<MatchResult>();

        for (int i = 0; i < games; i++)
        {
            var runner = new GameRunner(
                seed: seed + i,
                agent1Factory: agent1Factory,
                agent2Factory: agent2Factory
            );

            var result = runner.PlayGame(mode);
            results.Add(result);
            // switch (result.TypZwyciestwa)
            // {
            //     case TypZwyciestwa.Militarne:
            //         Console.WriteLine($"Game {i + 1}/{games} ended with a Military victory. Seed: {result.Seed}");
            //         break;
            //     case TypZwyciestwa.Naukowe:
            //         Console.WriteLine($"Game {i + 1}/{games} ended with a Scientific victory. Seed: {result.Seed}");
            //         break;
            //     case TypZwyciestwa.Brak:
            //         Console.WriteLine($"Game {i + 1}/{games} ended with no victory (Tie). Seed: {result.Seed}");
            //         break;
            // }
            if (result.Agent1Score == 0 || result.Agent2Score == 0)
            {
                Console.WriteLine($"Game {i + 1}/{games} had a zero score. Seed: {result.Seed}");
            }
            if (result.Agent1Score > 90 || result.Agent2Score > 90)
            {
                Console.WriteLine($"Game {i + 1}/{games} had an unusually high score. Seed: {result.Seed}");
            }
        }

        return Summarize(results);
    }

    private SimulationResult Summarize(List<MatchResult> results)
    {
        var winsA1 = results.Count(r => r.Winner == results[0].Agent1Name);
        var winsA2 = results.Count(r => r.Winner == results[0].Agent2Name);

        var avgA1Points = results.Average(r => r.Agent1Score);
        var avgA2Points = results.Average(r => r.Agent2Score);

        var typeCounts = results
            .GroupBy(r => new
            {
                Agent = r.Winner ?? "Remis",
                Typ = r.TypZwyciestwa
            })
            .Select(g => new VictoryTypeStat
            {
                Agent = g.Key.Agent,
                TypZwyciestwa = g.Key.Typ,
                Liczba = g.Count()
            })
            .ToList();

        return new SimulationResult
        {
            TotalGames = results.Count,
            Agent1Wins = winsA1,
            Agent2Wins = winsA2,
            Agent1MaxPoints = results.Max(r => r.Agent1Score),
            Agent2MaxPoints = results.Max(r => r.Agent2Score),
            Agent1MinPoints = results.Min(r => r.Agent1Score),
            Agent2MinPoints = results.Min(r => r.Agent2Score),
            Agent1AveragePoints = avgA1Points,
            Agent2AveragePoints = avgA2Points,
            VictoryTypeCounts = typeCounts,
            MatchResults = results
        };
    }
}