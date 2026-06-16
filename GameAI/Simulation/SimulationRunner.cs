using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class SimulationRunner
{
    private readonly Func<IRandom, IAgent> agent1Factory;
    private readonly Func<IRandom, IAgent> agent2Factory;
    private readonly string? agent1Type;
    private readonly string? agent2Type;
    private readonly int games;
    private readonly int seed;
    private readonly SimulationMode mode;

    public SimulationRunner(
        int seed,
        int games,
        SimulationMode mode,
        Func<IRandom, IAgent> agent1Factory,
        Func<IRandom, IAgent> agent2Factory,
        string? agent1Type = null,
        string? agent2Type = null)
    {
        this.seed = seed;
        this.games = games;
        this.mode = mode;
        this.agent1Factory = agent1Factory;
        this.agent2Factory = agent2Factory;
        this.agent1Type = agent1Type;
        this.agent2Type = agent2Type;
    }

    public SimulationResult Run(bool both = false)
    {
        var results = new List<MatchResult>();
        var reversedResults = new List<MatchResult>();
        var timings = new List<MatchTimingStat>();
        var totalStopwatch = Stopwatch.StartNew();

        for (int i = 0; i < games; i++)
        {
            //Console.WriteLine($"Starting game {i + 1}/{games} with seed {seed + i}...");
            var runner = new GameRunner(
                seed: seed + i,
                agent1Factory: agent1Factory,
                agent2Factory: agent2Factory
            );

            var gameStopwatch = Stopwatch.StartNew();
            var result = runner.PlayGame(mode);
            gameStopwatch.Stop();

            //if (i == 0 || (i + 1) % 10 == 0)
            //    Console.WriteLine($"Finished game {i + 1}/{games} with seed {seed + i} in {gameStopwatch.ElapsedMilliseconds} ms");
            results.Add(result);
            timings.Add(new MatchTimingStat
            {
                GameNumber = i + 1,
                Seed = result.Seed,
                Agent1Name = result.Agent1Name,
                Agent2Name = result.Agent2Name,
                Turns = result.Turns,
                ElapsedMilliseconds = gameStopwatch.ElapsedMilliseconds,
            });

            if (result.Agent1Score == 0 || result.Agent2Score == 0)
            {
                Console.WriteLine($"Game {i + 1}/{games} had a zero score. Seed: {result.Seed}");
            }
            if (result.Agent1Score > 100 || result.Agent2Score > 100)
            {
                Console.WriteLine($"Game {i + 1}/{games} had an unusually high score. Seed: {result.Seed}");
            }
        }

        if (both)
        {
            Console.WriteLine("Running reversed games...");
            for (int i = 0; i < games; i++)
            {
                var reversedRunner = new GameRunner(
                    seed: seed + i,
                    agent1Factory: agent2Factory,
                    agent2Factory: agent1Factory
                );

                var gameStopwatch = Stopwatch.StartNew();
                var result = reversedRunner.PlayGame(mode);
                gameStopwatch.Stop();

                reversedResults.Add(result);
                timings.Add(new MatchTimingStat
                {
                    GameNumber = games + i + 1,
                    Seed = result.Seed,
                    Agent1Name = result.Agent1Name,
                    Agent2Name = result.Agent2Name,
                    Turns = result.Turns,
                    ElapsedMilliseconds = gameStopwatch.ElapsedMilliseconds,
                });

                if (result.Agent1Score == 0 || result.Agent2Score == 0)
                {
                    Console.WriteLine($"Game {i + 1}/{games} had a zero score. Seed: {result.Seed}");
                }
                if (result.Agent1Score > 100 || result.Agent2Score > 100)
                {
                    Console.WriteLine($"Game {i + 1}/{games} had an unusually high score. Seed: {result.Seed}");
                }
            }
            results.AddRange(reversedResults);
        }

        totalStopwatch.Stop();

        return Summarize(results, timings, totalStopwatch.ElapsedMilliseconds);
    }

    private SimulationResult Summarize(List<MatchResult> results, List<MatchTimingStat> timings, long totalElapsedMilliseconds)
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
            TotalElapsedMilliseconds = totalElapsedMilliseconds,
            AverageGameElapsedMilliseconds = timings.Count == 0 ? 0 : timings.Average(t => (double)t.ElapsedMilliseconds),
            Agent1Wins = winsA1,
            Agent2Wins = winsA2,
            Agent1MaxPoints = results.Max(r => r.Agent1Score),
            Agent2MaxPoints = results.Max(r => r.Agent2Score),
            Agent1MinPoints = results.Min(r => r.Agent1Score),
            Agent2MinPoints = results.Min(r => r.Agent2Score),
            Agent1AveragePoints = avgA1Points,
            Agent2AveragePoints = avgA2Points,
            //GameTimings = timings,
            VictoryTypeCounts = typeCounts,
            Agent1Type = agent1Type,
            Agent2Type = agent2Type,
            //MatchResults = results
        };
    }
}