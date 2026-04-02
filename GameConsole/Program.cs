using System.Diagnostics;
using System.Drawing;
using System.Text.Json;

class Program
{
    static void Main(string[] args)
    {
        //GraKonsolowa graKonsolowa = new GraKonsolowa();
        //graKonsolowa.Start();
        SimulationRunnerFunction();
        //GameRunnerFunction();
    }

    static void SimulationRunnerFunction()
    {
        int seed = 12345;
        int games = 5000; // tutaj jest jakis blad

        var simulationRunner = new SimulationRunner(
            seed,
            games,
            r => new RandomAgent(r),
            r => new RandomAgent(r)
        );

        var result = simulationRunner.Run();

        Console.WriteLine($"Total games: {result.TotalGames}");
        Console.WriteLine($"Agent1 wins: {result.Agent1Wins}, avg points: {result.Agent1AveragePoints:F2}");
        Console.WriteLine($"Agent2 wins: {result.Agent2Wins}, avg points: {result.Agent2AveragePoints:F2}");

        Console.WriteLine("Victory types count:");
        foreach (var kvp in result.VictoryTypeCounts)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value}");
        }
    }
    static void GameRunnerFunction()
    {
        int seed = 12345+45;

        var runner = new GameRunner(
            seed: seed,
            agent1Factory: r => new RandomAgent(r),
            agent2Factory: r => new RandomAgent(r)
        );
        var result = runner.PlayGame();

        PrintResult(result);

        var projectDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..");
        var resultsDir = Path.Combine(projectDir, "Results");
        Directory.CreateDirectory(resultsDir);
        var fileName = $"match_{DateTime.Now:yyyyMMdd_HHmmss}_{result.MatchId}.json";
        var fullPath = Path.Combine(resultsDir, fileName);

        ResultWriter.Save(result, fullPath);
    }

    static void PrintResult(MatchResult result)
    {
        Console.WriteLine($"MatchId: {result.MatchId}");
        Console.WriteLine($"Seed: {result.Seed}");

        Console.WriteLine($"{result.Agent1Name} vs {result.Agent2Name}");

        Console.WriteLine($"Winner: {result.Winner}");
        Console.WriteLine($"Victory type: {result.TypZwyciestwa}");

        Console.WriteLine($"Turns: {result.Turns}");
        Console.WriteLine(
            $"Score: {result.Agent1Score} - {result.Agent2Score}");
    }
}

