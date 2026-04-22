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
        //HeuristicRunnerFunction();
        //HeuristicGameRunner();
        //MultipleGameRunnerFunction();
        //GameRunnerFunction();
    }

    static void SimulationRunnerFunction()
    {
        int seed = 1;
        int games = 1000;

        var agents = new List<(string Name, Func<IRandom, IAgent> Factory)>
        {
            ("RandomAgent", r => new RandomAgent(r)),
            ("HeuristicAgent (Military)", r => new HeuristicAgent(HeuristicWeightPresets.Military(), r)),
            ("HeuristicAgent (Balanced)", r => new HeuristicAgent(HeuristicWeightPresets.Balanced(), r)),
            ("MCTS Agent", r => new MctsAgent(r))
        };

        //var simulationResults = new List<SimulationResult>();

        for (int i = 0; i < agents.Count; i++)
        {
            for (int j = 0; j < agents.Count; j++)
            {
                Console.WriteLine($"Running simulation: {agents[i].Name} vs {agents[j].Name}");
                var simulationRunner = new SimulationRunner(
                    seed,
                    games,
                    SimulationMode.Tournament,
                    agents[i].Factory,
                    agents[j].Factory
                );
                var result = simulationRunner.Run();
                Console.WriteLine($"Total games: {result.TotalGames}");
                Console.WriteLine($"Agent1 max points: {result.Agent1MaxPoints}, min points: {result.Agent1MinPoints}");
                Console.WriteLine($"Agent2 max points: {result.Agent2MaxPoints}, min points: {result.Agent2MinPoints}");
                Console.WriteLine($"Agent1 wins: {result.Agent1Wins}, avg points: {result.Agent1AveragePoints:F2}");
                Console.WriteLine($"Agent2 wins: {result.Agent2Wins}, avg points: {result.Agent2AveragePoints:F2}");
                Console.WriteLine("Victory types count:");
                foreach (var kvp in result.VictoryTypeCounts)
                {
                    Console.WriteLine($"{kvp.Agent},{kvp.TypZwyciestwa}: {kvp.Liczba}");
                }
                Console.WriteLine(new string('-', 50));
                //simulationResults.Add(result);
            }
        }
        //var projectDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..");
        //var resultsDir = Path.Combine(projectDir, "Simulations");
        //Directory.CreateDirectory(resultsDir);
        //var fileName = $"agent_simulation_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        //var fullPath = Path.Combine(resultsDir, fileName);

        //foreach (var result in simulationResults)
        //{
        //    result.PrepareForSerialization();
        //}

        //ResultWriter.Save(simulationResults, fullPath);
    }
    static void HeuristicRunnerFunction()
    {
        int seed = 1;
        int games = 100;

        var simulationRunner = new SimulationRunner(
            seed,
            games,
            SimulationMode.Tournament,
            //r => new HeuristicAgent(HeuristicWeightPresets.Military(),r),
            r => new RandomAgent(r),
            r => new MctsAgent(r)
        );

        var result = simulationRunner.Run();

        Console.WriteLine($"Total games: {result.TotalGames}");
        Console.WriteLine($"Agent1 max points: {result.Agent1MaxPoints}, min points: {result.Agent1MinPoints}");
        Console.WriteLine($"Agent2 max points: {result.Agent2MaxPoints}, min points: {result.Agent2MinPoints}");
        Console.WriteLine($"Agent1 wins: {result.Agent1Wins}, avg points: {result.Agent1AveragePoints:F2}");
        Console.WriteLine($"Agent2 wins: {result.Agent2Wins}, avg points: {result.Agent2AveragePoints:F2}");

        Console.WriteLine("Victory types count:");
        foreach (var kvp in result.VictoryTypeCounts)
        {
            Console.WriteLine($"{kvp.Agent},{kvp.TypZwyciestwa}: {kvp.Liczba}");
        }
    }
    static void MultipleGameRunnerFunction()
    {
        int seed = 1;

        for (int i = 0; i < 10; i++)
        {
            var runner = new GameRunner(
                seed: seed,
                agent1Factory: r => new MctsAgent(r),
                agent2Factory: r => new RandomAgent(r)
            );
            var result = runner.PlayGame(SimulationMode.Debug);

            PrintResult(result);

            var projectDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..");
            var resultsDir = Path.Combine(projectDir, "Results");
            Directory.CreateDirectory(resultsDir);
            var fileName = $"match_{DateTime.Now:yyyyMMdd_HHmmss}_{result.MatchId}.json";
            var fullPath = Path.Combine(resultsDir, fileName);

            ResultWriter.Save(result, fullPath);
            // Increment seed for next game to get different results
            seed++;
        }
    }
    static void GameRunnerFunction()
    {
        int seed = 8988;

        var runner = new GameRunner(
            seed: seed,
            agent1Factory: r => new MctsAgent(r),
            agent2Factory: r => new RandomAgent(r)
        );
        var result = runner.PlayGame(SimulationMode.Debug);

        PrintResult(result);

        var projectDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..");
        var resultsDir = Path.Combine(projectDir, "Results");
        Directory.CreateDirectory(resultsDir);
        var fileName = $"match_{DateTime.Now:yyyyMMdd_HHmmss}_{result.MatchId}.json";
        var fullPath = Path.Combine(resultsDir, fileName);

        ResultWriter.Save(result, fullPath);
    }
    static void HeuristicGameRunner()
    {
        int seed = 837;

        var runner = new GameRunner(
            seed: seed,
            agent1Factory: r => new HeuristicAgent(HeuristicWeightPresets.Balanced(), r),
            agent2Factory: r => new HeuristicAgent(HeuristicWeightPresets.Military(), r)
        );
        var result = runner.PlayGame(SimulationMode.Debug);

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

