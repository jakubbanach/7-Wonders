using System.Diagnostics;
using System.Drawing;
using System.Text.Json;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length > 0 && string.Equals(args[0], "compare-models", StringComparison.OrdinalIgnoreCase))
        {
            CompareModelsFunction(args.Skip(1).ToArray());
            return;
        }

        if (args.Length > 0 && string.Equals(args[0], "benchmark-models", StringComparison.OrdinalIgnoreCase))
        {
            BenchmarkModelsFunction(args.Skip(1).ToArray());
            return;
        }

        if (args.Length > 0 && string.Equals(args[0], "export-data", StringComparison.OrdinalIgnoreCase))
        {
            ExportTrainingDataFunction(args.Skip(1).ToArray());
            return;
        }
        BenchmarkModelsFunction(args.Skip(1).ToArray());
        //SimulationRunnerFunction();
        //HeuristicRunnerFunction();
        //HeuristicGameRunner();
        //GameRunnerFunction();
        //GeneticAlgorithmRunnerFunction();
    }

    static void ExportTrainingDataFunction(string[] args)
    {
        int seed = 12345;
        int games = 20;
        string agent1Name = "heuristic-personal";
        string agent2Name = "mcts";

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--seed" when i + 1 < args.Length:
                    seed = int.Parse(args[++i]);
                    break;
                case "--games" when i + 1 < args.Length:
                    games = int.Parse(args[++i]);
                    break;
                case "--agent1" when i + 1 < args.Length:
                    agent1Name = args[++i];
                    break;
                case "--agent2" when i + 1 < args.Length:
                    agent2Name = args[++i];
                    break;
            }
        }

        var agentFactories = BuildAgentFactories();
        if (!agentFactories.TryGetValue(agent1Name, out var agent1Factory))
            throw new ArgumentException($"Unknown agent1: {agent1Name}");
        if (!agentFactories.TryGetValue(agent2Name, out var agent2Factory))
            throw new ArgumentException($"Unknown agent2: {agent2Name}");

        var simulationRunner = new SimulationRunner(
            seed,
            games,
            SimulationMode.Debug,
            agent1Factory,
            agent2Factory);

        var result = simulationRunner.Run();
        var projectDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..");
        var resultsDir = Path.GetFullPath(Path.Combine(projectDir, "Results"));
        Directory.CreateDirectory(resultsDir);

        var fileName = $"training_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        var fullPath = Path.Combine(resultsDir, fileName);
        ResultWriter.Save(result, fullPath);

        Console.WriteLine($"Training data saved to: {fullPath}");
    }

    static void CompareModelsFunction(string[] args)
    {
        int seed = 12345;
        int games = 50;
        string agent1Spec = "onnx:policy_network.onnx";
        string agent2Spec = "heuristic-personal";

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--seed" when i + 1 < args.Length:
                    seed = int.Parse(args[++i]);
                    break;
                case "--games" when i + 1 < args.Length:
                    games = int.Parse(args[++i]);
                    break;
                case "--agent1" when i + 1 < args.Length:
                    agent1Spec = args[++i];
                    break;
                case "--agent2" when i + 1 < args.Length:
                    agent2Spec = args[++i];
                    break;
            }
        }

        var agent1Factory = CreateAgentFactoryFromSpec(agent1Spec);
        var agent2Factory = CreateAgentFactoryFromSpec(agent2Spec);

        Console.WriteLine($"Comparing: {agent1Spec} vs {agent2Spec}");

        var simulationRunner = new SimulationRunner(
            seed,
            games,
            SimulationMode.Tournament,
            agent1Factory,
            agent2Factory);

        var result = simulationRunner.Run();

        Console.WriteLine($"Total games: {result.TotalGames}");
        Console.WriteLine($"{agent1Spec} wins: {result.Agent1Wins}, avg points: {result.Agent1AveragePoints:F2}");
        Console.WriteLine($"{agent2Spec} wins: {result.Agent2Wins}, avg points: {result.Agent2AveragePoints:F2}");
        Console.WriteLine($"Points: {result.Agent1MinPoints}-{result.Agent1MaxPoints} vs {result.Agent2MinPoints}-{result.Agent2MaxPoints}");
    }

    static void BenchmarkModelsFunction(string[] args)
    {
        int seed = 12345;
        int games = 10;
        string agent1Spec = "onnx:GameAI/Encoding/policy_network.onnx";
        string agent2Spec = "heuristic-personal";
        string outputName = $"benchmark_{DateTime.Now:yyyyMMdd_HHmmss}";

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--seed" when i + 1 < args.Length:
                    seed = int.Parse(args[++i]);
                    break;
                case "--games" when i + 1 < args.Length:
                    games = int.Parse(args[++i]);
                    break;
                case "--agent1" when i + 1 < args.Length:
                    agent1Spec = args[++i];
                    break;
                case "--agent2" when i + 1 < args.Length:
                    agent2Spec = args[++i];
                    break;
                case "--output" when i + 1 < args.Length:
                    outputName = args[++i];
                    break;
            }
        }

        var agent1Factory = CreateAgentFactoryFromSpec(agent1Spec);
        var agent2Factory = CreateAgentFactoryFromSpec(agent2Spec);

        Console.WriteLine($"Benchmarking: {agent1Spec} vs {agent2Spec}");
        Console.WriteLine($"Games: {games}, seed: {seed}");

        var simulationRunner = new SimulationRunner(
            seed,
            games,
            SimulationMode.Debug,
            agent1Factory,
            agent2Factory);

        var result = simulationRunner.Run();
        var benchmarkDir = GetBenchmarkOutputDirectory(outputName);
        Directory.CreateDirectory(benchmarkDir);

        var summaryPath = Path.Combine(benchmarkDir, "summary.json");
        var matchesDir = Path.Combine(benchmarkDir, "matches");
        Directory.CreateDirectory(matchesDir);

        ResultWriter.Save(result, summaryPath);

        for (int i = 0; i < result.MatchResults.Count; i++)
        {
            var match = result.MatchResults[i];
            var fileName = $"match_{i + 1:D4}_seed_{match.Seed}.json";
            var matchPath = Path.Combine(matchesDir, fileName);
            ResultWriter.Save(match, matchPath);
        }

        Console.WriteLine($"Benchmark saved to: {benchmarkDir}");
        Console.WriteLine($"Summary: {summaryPath}");
        Console.WriteLine($"Match logs: {matchesDir}");
        Console.WriteLine($"Total games: {result.TotalGames}");
        Console.WriteLine($"{agent1Spec} wins: {result.Agent1Wins}, avg points: {result.Agent1AveragePoints:F2}");
        Console.WriteLine($"{agent2Spec} wins: {result.Agent2Wins}, avg points: {result.Agent2AveragePoints:F2}");
    }

    static Func<IRandom, IAgent> CreateAgentFactoryFromSpec(string spec)
    {
        if (spec.StartsWith("onnx:", StringComparison.OrdinalIgnoreCase))
        {
            var projectDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..");

            var modelPath = spec.Substring("onnx:".Length);
            modelPath = Path.Combine(projectDir, modelPath);
            return random => new OnnxAgentConfiguration
            {
                ModelPath = Path.GetFullPath(modelPath)
            }.CreateAgent(random);
        }

        var builtIn = BuildAgentFactories();
        if (builtIn.TryGetValue(spec, out var factory))
            return factory;

        throw new ArgumentException($"Unknown agent spec: {spec}. Use 'onnx:<path>' or one of: {string.Join(", ", builtIn.Keys)}");
    }

    static Dictionary<string, Func<IRandom, IAgent>> BuildAgentFactories()
    {
        return new Dictionary<string, Func<IRandom, IAgent>>(StringComparer.OrdinalIgnoreCase)
        {
            ["random"] = r => new RandomAgent(r),
            ["mcts"] = r => new MctsAgent(r),
            ["heuristic-personal"] = r => new HeuristicAgent(HeuristicWeightPresets.Personal(), r),
            ["heuristic-double"] = r => new HeuristicAgent(HeuristicWeightPresets.GeneticDouble(), r),
            ["heuristic-balanced"] = r => new HeuristicAgent(HeuristicWeightPresets.Balanced(), r),
            ["heuristic-military"] = r => new HeuristicAgent(HeuristicWeightPresets.Military(), r),
            ["onnx"] = r => new OnnxAgentConfiguration
            {
                ModelPath = Path.Combine(AppContext.BaseDirectory, "policy_network.onnx")
            }.CreateAgent(r),
        };
    }

    static string GetBenchmarkOutputDirectory(string outputName)
    {
        var projectDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..");
        var resultsDir = Path.GetFullPath(Path.Combine(projectDir, "Results"));
        return Path.Combine(resultsDir, outputName);
    }

    static void SimulationRunnerFunction()
    {
        int seed = 12345;
        int games = 20;

        var agents = new List<(string Name, Func<IRandom, IAgent> Factory)>
        {
            //("RandomAgent", r => new RandomAgent(r)),
            //("HeuristicAgent (Military)", r => new HeuristicAgent(HeuristicWeightPresets.Military(), r)),
            //("HeuristicAgent (Balanced)", r => new HeuristicAgent(HeuristicWeightPresets.Balanced(), r)),
            ("HeuristicAgent (GeneticPersonal)", r => new HeuristicAgent(HeuristicWeightPresets.Personal(), r)),
            ("HeuristicAgent (GeneticDouble)", r => new HeuristicAgent(HeuristicWeightPresets.GeneticDouble(), r)),
            ("MCTS Agent", r => new MctsAgent(r))
        };

        //var simulationResults = new List<SimulationResult>();

        for (int i = 0; i < agents.Count; i++)
        {
            for (int j = 0; j < agents.Count; j++)
            {
                if (i == j) continue; // Skip self-play for now
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
        int seed = 1000;

        for (int i = 0; i < 10; i++)
        {
            var runner = new GameRunner(
                seed: seed,
                agent1Factory: r => new HeuristicAgent(HeuristicWeightPresets.Personal(), r),
                agent2Factory: r => new HeuristicAgent(HeuristicWeightPresets.GeneticDouble(), r)
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
    static void GeneticAlgorithmRunnerFunction()
    {
        int seed = 1000;
        IRandom masterRng = new RandomAdapter(seed);

        Console.WriteLine("Rozpoczynam trening ewolucyjny...");

        var em = new EvolutionManager(masterRng, popSize: 20);
        em.UruchomEwolucje(generacje: 40);

        Console.WriteLine("Ewolucja zakończona. Najlepsze wagi zapisane.");
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

