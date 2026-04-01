using System.Diagnostics;
using System.Drawing;
using System.Text.Json;

class Program
{
    static void Main(string[] args)
    {
        //GraKonsolowa graKonsolowa = new GraKonsolowa();
        //graKonsolowa.Start();
        int seed = 12345;

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

