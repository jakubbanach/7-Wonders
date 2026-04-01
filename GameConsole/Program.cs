using System.Diagnostics;
using System.Drawing;

class Program
{
    static void Main(string[] args)
    {
        //GraKonsolowa graKonsolowa = new GraKonsolowa();
        //graKonsolowa.Start();

        var agent1 = new RandomAgent();
        var agent2 = new RandomAgent();

        var runner = new GameRunner(seed: 12345);

        var result = runner.PlayGame(agent1, agent2);

        PrintResult(result);
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

