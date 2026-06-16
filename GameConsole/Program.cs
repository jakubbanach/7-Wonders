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

        if (args.Length > 0 && string.Equals(args[0], "self-play-train", StringComparison.OrdinalIgnoreCase))
        {
            SelfPlayTrainFunction(args.Skip(1).ToArray());
            return;
        }
        //SelfPlayTrainFunction(args.Skip(1).ToArray());
        //ExportTrainingDataFunction(args.Skip(1).ToArray());
        //BenchmarkModelsFunction(args.Skip(1).ToArray());
        //SimulationPuctTesting();
        //SimulationMctsIterationsFunction();
        SimulationRunnerFunction();
        //HeuristicRunnerFunction();
        //HeuristicGameRunner();
        //GameRunnerFunction();
        //GeneticAlgorithmRunnerFunction();
    }

    static void ExportTrainingDataFunction(string[] args)
    {
        int seed = 10000;
        int games = 500;
        string agent2Name = "heuristic-double";
        string agent1Name = "onnx3";
        bool minimal = true;
        bool both = false;

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
                case "--minimal-logs":
                    minimal = true;
                    break;
                case "--both":
                    both = true;
                    break;
            }
        }

        var agentFactories = BuildAgentFactories();
        if (!agentFactories.TryGetValue(agent1Name, out var agent1Factory))
            throw new ArgumentException($"Unknown agent1: {agent1Name}");
        if (!agentFactories.TryGetValue(agent2Name, out var agent2Factory))
            throw new ArgumentException($"Unknown agent2: {agent2Name}");

        Console.WriteLine($"Starting training: {agent1Name} vs {agent2Name} ({games} gier{(minimal ? " - MINIMAL LOGS" : "")})");
        var simulationRunner = new SimulationRunner(
            seed,
            games,
            SimulationMode.Debug,
            agent1Factory,
            agent2Factory);

        var result = simulationRunner.Run(both: both);

        if (minimal)
        {
            Console.WriteLine("Minimalizowanie logow dla economii pamieci...");
            foreach (var match in result.MatchResults)
            {
                match.MinimalizeForTraining();
            }
        }

        var projectDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..");
        var resultsDir = Path.GetFullPath(Path.Combine(projectDir, "Results"));
        Directory.CreateDirectory(resultsDir);

        var fileName = $"training_{DateTime.Now:yyyyMMdd_HHmmss}_{agent1Name}_vs_{agent2Name}_{games}_games{(minimal ? "_minimal" : "")}.json";
        var fullPath = Path.Combine(resultsDir, fileName);
        //ResultWriter.Save(result, fullPath);
        ResultWriter.SaveBinaryTrainingDataNpz(result, Path.ChangeExtension(fullPath, ".npz"));

        Console.WriteLine($"Training data saved to: {fullPath}");
    }

    static void SelfPlayTrainFunction(string[] args)
    {
        int seed = 12345;
        int games = 1000;
        string modelPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "GameAI/Encoding/onnx_models/policy_network_50_500.onnx");
        string outputName = $"{DateTime.Now:yyyyMMdd_HHmmss}";
        bool minimal = true;

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
                case "--model" when i + 1 < args.Length:
                    modelPath = args[++i];
                    break;
                case "--output" when i + 1 < args.Length:
                    outputName = args[++i];
                    break;
                case "--full-logs":
                    minimal = false;
                    break;
                case "--minimal-logs":
                    minimal = true;
                    break;
            }
        }

        modelPath = Path.GetFullPath(modelPath);
        var puctSpec = $"puct:{modelPath}";
        //var agentFactory = CreateAgentFactoryFromSpec(puctSpec);
        var agentFactory = CreateAgentFactoryFromSpec("mcts");

        Console.WriteLine($"Starting self-play MCTS training");
        Console.WriteLine($"Model: {modelPath}");
        Console.WriteLine($"Games: {games}, seed: {seed}{(minimal ? " - MINIMAL LOGS" : "")}");

        var simulationRunner = new SimulationRunner(
            seed,
            games,
            SimulationMode.Debug,
            agentFactory,
            agentFactory);

        var result = simulationRunner.Run();
        if (minimal)
        {
            //Console.WriteLine("Minimalizowanie logow dla economii pamieci...");
            foreach (var match in result.MatchResults)
            {
                match.MinimalizeForTraining();
            }
        }
        var projectDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..");
        var resultsDir = Path.GetFullPath(Path.Combine(projectDir, "Results"));
        Directory.CreateDirectory(resultsDir);

        var fileName = $"mcts_{outputName}_{games}_games{(minimal ? "_minimal" : "")}.npz";
        var fullPath = Path.Combine(resultsDir, fileName);
        //ResultWriter.Save(result, fullPath);
        ResultWriter.SaveBinaryTrainingDataNpz(result, fullPath);

        Console.WriteLine($"Self-play training data saved to: {fullPath}");
        Console.WriteLine($"Agent1 wins: {result.Agent1Wins}, avg points: {result.Agent1AveragePoints:F2}");
        Console.WriteLine($"Agent2 wins: {result.Agent2Wins}, avg points: {result.Agent2AveragePoints:F2}");
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
        int games = 100;
        string agent1Spec = "heuristic-double";
        string agent2Spec = "onnx_50_500";
        string outputName = $"benchmark_{DateTime.Now:yyyyMMdd_HHmmss}_{agent1Spec}_vs_{agent2Spec}";
        bool minimal = false;

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
                case "--minimal-logs":
                    minimal = true;
                    break;
            }
        }

        var agent1Factory = CreateAgentFactoryFromSpec(agent1Spec);
        var agent2Factory = CreateAgentFactoryFromSpec(agent2Spec);

        Console.WriteLine($"Benchmarking: {agent1Spec} vs {agent2Spec}");
        Console.WriteLine($"Games: {games}, seed: {seed}{(minimal ? " - MINIMAL LOGS" : "")}");

        var simulationRunner = new SimulationRunner(
            seed,
            games,
            SimulationMode.Debug,
            agent1Factory,
            agent2Factory);

        var result = simulationRunner.Run();

        //if (minimal)
        //{
        //    Console.WriteLine("Minimalizowanie logów dla économii pamięci...");
        //    foreach (var match in result.MatchResults)
        //    {
        //        match.MinimalizeForTraining();
        //    }
        //}

        var benchmarkDir = GetBenchmarkOutputDirectory(outputName + (minimal ? "_minimal" : ""));
        Directory.CreateDirectory(benchmarkDir);

        var summaryPath = Path.Combine(benchmarkDir, "summary.json");
        var matchesDir = Path.Combine(benchmarkDir, "matches");
        Directory.CreateDirectory(matchesDir);

        ResultWriter.Save(result, summaryPath);

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

        if (spec.StartsWith("puct:", StringComparison.OrdinalIgnoreCase))
        {
            var projectDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..");

            var modelPath = spec.Substring("puct:".Length);
            modelPath = Path.Combine(projectDir, modelPath);
            return random => new PuctValueMctsAgent(
                random,
                modelPath: Path.GetFullPath(modelPath));
        }

        var builtIn = BuildAgentFactories();
        if (builtIn.TryGetValue(spec, out var factory))
            return factory;

        throw new ArgumentException($"Unknown agent spec: {spec}. Use 'onnx:<path>', 'puct:<path>' or one of: {string.Join(", ", builtIn.Keys)}");
    }

    static Dictionary<string, Func<IRandom, IAgent>> BuildAgentFactories()
    {
        return new Dictionary<string, Func<IRandom, IAgent>>(StringComparer.OrdinalIgnoreCase)
        {
            ["random"] = r => new RandomAgent(r),
            ["mcts-rd-50"] = r => new MctsAgent(r, true, false, 50),
            ["mcts-rd-100"] = r => new MctsAgent(r, true, false, 100),
            ["mcts-rd-200"] = r => new MctsAgent(r, true, false, 200),
            ["mcts-rd"] = r => new MctsAgent(r, true, false), //300
            ["mcts-rd-rr"] = r => new MctsAgent(r, true, true),
            ["ismcts"] = r => new ISMctsAgent(r),
            ["heuristic-personal"] = r => new HeuristicAgent(HeuristicWeightPresets.Personal(), r),
            ["heuristic-double"] = r => new HeuristicAgent(HeuristicWeightPresets.GeneticDouble(), r),
            ["heuristic-double-new"] = r => new HeuristicAgent(HeuristicWeightPresets.GeneticDoubleNew(), r),
            ["heuristic-balanced"] = r => new HeuristicAgent(HeuristicWeightPresets.Balanced(), r),
            ["heuristic-military"] = r => new HeuristicAgent(HeuristicWeightPresets.Military(), r),
            ["onnx"] = r => new OnnxAgentConfiguration
            {
                ModelPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "GameAI/Encoding/onnx_models/policy_network_20_100.onnx")
            }.CreateAgent(r),
            ["onnx2"] = r => new OnnxAgentConfiguration
            {
                ModelPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "GameAI/Encoding/onnx_models/policy_network_20_100_2.onnx")
            }.CreateAgent(r),
            ["onnx3"] = r => new OnnxAgentConfiguration
            {
                ModelPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "GameAI/Encoding/onnx_models/policy_network_50_200.onnx")
            }.CreateAgent(r),
            ["puct"] = r => new PuctValueMctsAgent(r, Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "GameAI/Encoding/onnx_models/puct_50_100.onnx")),
            ["puct_match"] = r => new PuctValueMctsAgent(r, Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "GameAI/Encoding/onnx_models/puct_50_100_match_split.onnx")),
            ["puct_iter_1"] = r => new PuctValueMctsAgent(r, Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "GameAI/Encoding/onnx_models/mcts_nn_iterations/policy_network_iter_001_best.onnx")),
            ["puct_iter_2"] = r => new PuctValueMctsAgent(r, Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "GameAI/Encoding/onnx_models/mcts_nn_iterations/policy_network_iter_002_best.onnx")),
            ["puct_iter_3"] = r => new PuctValueMctsAgent(r, Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "GameAI/Encoding/onnx_models/mcts_nn_iterations/policy_network_iter_003_best.onnx")),
            ["puct_iter_4"] = r => new PuctValueMctsAgent(r, Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "GameAI/Encoding/onnx_models/mcts_nn_iterations/policy_network_iter_004_best.onnx")),
            ["puct_iter_5"] = r => new PuctValueMctsAgent(r, Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "GameAI/Encoding/onnx_models/mcts_nn_iterations/policy_network_iter_005_best.onnx")),
            ["puct_latest"] = r => new PuctValueMctsAgent(r, Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "GameAI/Encoding/onnx_models/mcts_nn_iterations/policy_network_latest.onnx")),
            ["onnx_50_500"] = r => new OnnxAgentConfiguration
            {
                ModelPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "GameAI/Encoding/onnx_models/policy_network_50_500.onnx")
            }.CreateAgent(r),
        };
    }

    static string GetBenchmarkOutputDirectory(string outputName)
    {
        var projectDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..");
        var resultsDir = Path.GetFullPath(Path.Combine(projectDir, "Results"));
        return Path.Combine(resultsDir, outputName);
    }

    static void SimulationPuctTesting()
    {
        int seed = 1;
        int games = 100;

        var agents = new List<(string Name, Func<IRandom, IAgent> Factory)>
        {
            ("RandomAgent", r => new RandomAgent(r)),
            ("HeuristicAgent (GeneticPersonal)", CreateAgentFactoryFromSpec("heuristic-personal")),
            ("HeuristicAgent (GeneticDouble)", CreateAgentFactoryFromSpec("heuristic-double")),
            ("MCTSAgent", CreateAgentFactoryFromSpec("mcts")),
            ("PUCTAgent", CreateAgentFactoryFromSpec("puct")),
        };
        var puctAgents = new List<(string Name, Func<IRandom, IAgent> Factory)>
        {
            //("PUCTAgent Match", CreateAgentFactoryFromSpec("puct_match")),
            //("PUCTAgent Iter 2", CreateAgentFactoryFromSpec("puct_iter_2")),
            ("PUCTAgent Latest", CreateAgentFactoryFromSpec("puct_latest")),
        };


        for (int i = 0; i < agents.Count; i++)
        {
            Console.WriteLine($"Running simulation: {agents[i].Name} vs {puctAgents[0].Name}");


            var simulationRunner = new SimulationRunner(
                seed,
                games,
                SimulationMode.Tournament,
                agents[i].Factory,
                puctAgents[0].Factory
            );
            var result = simulationRunner.Run();
            Console.WriteLine($"Total games: {result.TotalGames}");
            Console.WriteLine($"Agent1 max points: {result.Agent1MaxPoints}, min points: {result.Agent1MinPoints}");
            Console.WriteLine($"Agent2 max points: {result.Agent2MaxPoints}, min points: {result.Agent2MinPoints}");
            Console.WriteLine($"Agent1 wins: {result.Agent1Wins}, avg points: {result.Agent1AveragePoints:F2}");
            Console.WriteLine($"Agent2 wins: {result.Agent2Wins}, avg points: {result.Agent2AveragePoints:F2}");
            Console.WriteLine($"Total elapsed: {TimeSpan.FromMilliseconds(result.TotalElapsedMilliseconds):g}");
            Console.WriteLine($"Average game elapsed: {result.AverageGameElapsedMilliseconds:F0} ms");
            Console.WriteLine("Victory types count:");
            foreach (var kvp in result.VictoryTypeCounts)
            {
                Console.WriteLine($"{kvp.Agent},{kvp.TypZwyciestwa}: {kvp.Liczba}");
            }
            Console.WriteLine(new string('-', 50));

            Console.WriteLine($"Running simulation: {puctAgents[0].Name} vs {agents[i].Name}");


            simulationRunner = new SimulationRunner(
                seed,
                games,
                SimulationMode.Tournament,
                puctAgents[0].Factory,
                agents[i].Factory
            );
            result = simulationRunner.Run();
            Console.WriteLine($"Total games: {result.TotalGames}");
            Console.WriteLine($"Agent1 max points: {result.Agent1MaxPoints}, min points: {result.Agent1MinPoints}");
            Console.WriteLine($"Agent2 max points: {result.Agent2MaxPoints}, min points: {result.Agent2MinPoints}");
            Console.WriteLine($"Agent1 wins: {result.Agent1Wins}, avg points: {result.Agent1AveragePoints:F2}");
            Console.WriteLine($"Agent2 wins: {result.Agent2Wins}, avg points: {result.Agent2AveragePoints:F2}");
            Console.WriteLine($"Total elapsed: {TimeSpan.FromMilliseconds(result.TotalElapsedMilliseconds):g}");
            Console.WriteLine($"Average game elapsed: {result.AverageGameElapsedMilliseconds:F0} ms");
            Console.WriteLine("Victory types count:");
            foreach (var kvp in result.VictoryTypeCounts)
            {
                Console.WriteLine($"{kvp.Agent},{kvp.TypZwyciestwa}: {kvp.Liczba}");
            }
            Console.WriteLine(new string('-', 50));
            
        }
    }
    static void SimulationRunnerFunction()
    {
        int seed = 10150; //12345
        int games = 150;

        var agents = new List<(string Name, Func<IRandom, IAgent> Factory)>
        {
            //("RandomAgent", r => new RandomAgent(r)),
            //("HeuristicAgent (GeneticPersonal)", CreateAgentFactoryFromSpec("heuristic-personal")),
            //("HeuristicAgent Genetic", CreateAgentFactoryFromSpec("heuristic-double")),
            ("MCTSAgent RD", CreateAgentFactoryFromSpec("mcts-rd")),
            ("MCTSAgent RD + RR", CreateAgentFactoryFromSpec("mcts-rd-rr")),
            ("ISMCTSAgent", CreateAgentFactoryFromSpec("ismcts")),
            //("PUCTAgent", CreateAgentFactoryFromSpec("puct")),
            //("PUCTAgent Match", CreateAgentFactoryFromSpec("puct_match")),
            //("PUCTAgent Iter 2", CreateAgentFactoryFromSpec("puct_iter_2")),
            //("PUCTAgent Latest", CreateAgentFactoryFromSpec("puct_latest")),
        };

        var allResults = new List<SimulationResult>();
        for (int i = 0; i < agents.Count; i++)
        {
            for (int j = 0; j < agents.Count; j++)
            {
                //if (i == j) continue; // Skip self-play for now
                Console.WriteLine($"Running simulation: {agents[i].Name} vs {agents[j].Name}");
                // reset instrumentation counters before run

                var simulationRunner = new SimulationRunner(
                    seed,
                    games,
                    SimulationMode.Tournament,
                    agents[i].Factory,
                    agents[j].Factory,
                    agents[i].Name,
                    agents[j].Name
                );
                var result = simulationRunner.Run();
                Console.WriteLine($"Total games: {result.TotalGames}");
                Console.WriteLine($"Agent1 max points: {result.Agent1MaxPoints}, min points: {result.Agent1MinPoints}");
                Console.WriteLine($"Agent2 max points: {result.Agent2MaxPoints}, min points: {result.Agent2MinPoints}");
                Console.WriteLine($"Agent1 wins: {result.Agent1Wins}, avg points: {result.Agent1AveragePoints:F2}");
                Console.WriteLine($"Agent2 wins: {result.Agent2Wins}, avg points: {result.Agent2AveragePoints:F2}");
                Console.WriteLine($"Total elapsed: {TimeSpan.FromMilliseconds(result.TotalElapsedMilliseconds):g}");
                Console.WriteLine($"Average game elapsed: {result.AverageGameElapsedMilliseconds:F0} ms");
                Console.WriteLine("Victory types count:");
                foreach (var kvp in result.VictoryTypeCounts)
                {
                    Console.WriteLine($"{kvp.Agent},{kvp.TypZwyciestwa}: {kvp.Liczba}");
                }
                Console.WriteLine(new string('-', 50));
                allResults.Add(result);
            }
        }
        var path = GetBenchmarkOutputDirectory("full_comparison_summary_10150_150.json");
        ResultWriter.Save(allResults, path);
    }
    static void SimulationMctsIterationsFunction()
    {
        int seed = 12345;
        int games = 1;

        var agents = new List<(string Name, Func<IRandom, IAgent> Factory)>
        {
            ("RandomAgent", r => new RandomAgent(r)),
            ("HeuristicAgent Genetic", CreateAgentFactoryFromSpec("heuristic-double")),
            ("MCTSAgent RD 50", CreateAgentFactoryFromSpec("mcts-rd-50")),
            ("MCTSAgent RD 100", CreateAgentFactoryFromSpec("mcts-rd-100")),
            ("MCTSAgent RD 200", CreateAgentFactoryFromSpec("mcts-rd-200")),
            ("MCTSAgent RD 300", CreateAgentFactoryFromSpec("mcts-rd")),
        };

        var allResults = new List<SimulationResult>();
        for (int i = 0; i < agents.Count; i++)
        {
            for (int j = 0; j < agents.Count; j++)
            {
                if (agents[i].Name.StartsWith("MCTSAgent") && agents[j].Name.StartsWith("MCTSAgent") && i != j)
                    continue; // Skip different MCTS
                Console.WriteLine($"Running simulation: {agents[i].Name} vs {agents[j].Name}");

                var simulationRunner = new SimulationRunner(
                    seed,
                    games,
                    SimulationMode.Tournament,
                    agents[i].Factory,
                    agents[j].Factory,
                    agents[i].Name,
                    agents[j].Name
                );
                var result = simulationRunner.Run();
                Console.WriteLine($"Total games: {result.TotalGames}");
                Console.WriteLine($"Agent1 max points: {result.Agent1MaxPoints}, min points: {result.Agent1MinPoints}");
                Console.WriteLine($"Agent2 max points: {result.Agent2MaxPoints}, min points: {result.Agent2MinPoints}");
                Console.WriteLine($"Agent1 wins: {result.Agent1Wins}, avg points: {result.Agent1AveragePoints:F2}");
                Console.WriteLine($"Agent2 wins: {result.Agent2Wins}, avg points: {result.Agent2AveragePoints:F2}");
                Console.WriteLine($"Total elapsed: {TimeSpan.FromMilliseconds(result.TotalElapsedMilliseconds):g}");
                Console.WriteLine($"Average game elapsed: {result.AverageGameElapsedMilliseconds:F0} ms");
                Console.WriteLine("Victory types count:");
                foreach (var kvp in result.VictoryTypeCounts)
                {
                    Console.WriteLine($"{kvp.Agent},{kvp.TypZwyciestwa}: {kvp.Liczba}");
                }
                Console.WriteLine(new string('-', 50));
                allResults.Add(result);
            }
        }
        var path = GetBenchmarkOutputDirectory("full_comparison_mcts_summary.json");
        ResultWriter.Save(allResults, path);
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
        Console.WriteLine($"Total elapsed: {TimeSpan.FromMilliseconds(result.TotalElapsedMilliseconds):g}");
        Console.WriteLine($"Average game elapsed: {result.AverageGameElapsedMilliseconds:F0} ms");

        Console.WriteLine("Per-game timings:");
        //foreach (var timing in result.GameTimings)
        //{
        //    Console.WriteLine($"  Game {timing.GameNumber:00} | Seed {timing.Seed} | {timing.Agent1Name} vs {timing.Agent2Name} | Turns {timing.Turns} | {timing.ElapsedMilliseconds} ms");
        //}

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
        int seed = 1013;

        var runner = new GameRunner(
            seed: seed,
            //agent1Factory: CreateAgentFactoryFromSpec("puct"),
            //agent2Factory: CreateAgentFactoryFromSpec("puct_latest")
            agent1Factory: CreateAgentFactoryFromSpec("ismcts"),
            agent2Factory: CreateAgentFactoryFromSpec("mcts")
        );
        var result = runner.PlayGame(SimulationMode.Debug);

        PrintResult(result);

        //var projectDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..");
        //var resultsDir = Path.Combine(projectDir, "Results");
        //Directory.CreateDirectory(resultsDir);
        //var fileName = $"match_{DateTime.Now:yyyyMMdd_HHmmss}_{result.MatchId}.json";
        //var fullPath = Path.Combine(resultsDir, fileName);

        //ResultWriter.Save(result, fullPath);
    }
    static void HeuristicGameRunner()
    {
        int seed = 12690;

        var runner = new GameRunner(
            seed: seed,
            agent1Factory: r => new HeuristicAgent(HeuristicWeightPresets.GeneticDoubleNew(), r),
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

