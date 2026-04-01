using System;
using System.Collections.Generic;
using System.Linq;

public class SimulationRunner
{
    private readonly Func<IRandom, IAgent> agent1Factory;
    private readonly Func<IRandom, IAgent> agent2Factory;
    private readonly int games;
    private readonly int seed;

    public SimulationRunner(
        int seed,
        int games,
        Func<IRandom, IAgent> agent1Factory,
        Func<IRandom, IAgent> agent2Factory)
    {
        this.seed = seed;
        this.games = games;
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

            var result = runner.PlayGame();
            results.Add(result);
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
            .GroupBy(r => r.TypZwyciestwa)
            .ToDictionary(g => g.Key, g => g.Count());

        return new SimulationResult
        {
            TotalGames = results.Count,
            Agent1Wins = winsA1,
            Agent2Wins = winsA2,
            Agent1AveragePoints = avgA1Points,
            Agent2AveragePoints = avgA2Points,
            VictoryTypeCounts = typeCounts,
            MatchResults = results
        };
    }
}