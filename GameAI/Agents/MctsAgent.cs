using System;
using System.Collections.Generic;
using System.Linq;

public class MctsAgent : IAgent
{
    public string Name { get; set; } = "MCTS";
    public static int Iterations { get; set; } = 300;
    public bool UseRootDeterminization { get; set; } = true;
    public bool ReshuffleInRollout { get; set; } = true;

    private readonly IRandom random;
    private MctsNode? currentRoot;
    private Gra? simulationGame;
    private readonly RandomAgent rolloutAgent;
    private readonly SimulationDecisionResolver rolloutResolver;
    private const double C = 1.414;

    public MctsAgent(IRandom random, bool useRootDeterminization = true, bool reshuffleInRollout = true, int iterations = 300)
    {
        this.random = random;
        UseRootDeterminization = useRootDeterminization;
        ReshuffleInRollout = reshuffleInRollout;
        Iterations = iterations;
        rolloutAgent = new RandomAgent(random);
        rolloutResolver = new SimulationDecisionResolver(rolloutAgent);
    }

    public Ruch WybierzRuch(Gra gra)
    {
        if (gra == null) throw new ArgumentNullException(nameof(gra));

        var rootGame = gra.Clone();
        bool hasHiddenCards = rootGame.PlanszaEpoki?.Pola.Any(p => p.CzyZakryta && p.Karta != null) == true;
        // bez determinizacji agent moglby uzywac faktycznych przypisan zakrytych kart, ktorych gracz nie powinien znac
        if (UseRootDeterminization || hasHiddenCards)
            rootGame.PotasujZakryteKarty(random);

        var root = MctsNode.Create(rootGame, null);

        if (!root.NieprzetestowaneRuchy.Any() && !root.Dzieci.Any())
            throw new InvalidOperationException("MCTS nie znalazl zadnego legalnego ruchu.");

        try
        {
            for (int i = 0; i < Iterations; i++)
                RunIteration(root, gra.AktywnyGracz.Nazwa);

            var najlepszy = NajlepszyRuch(root);
            // wyczysc konsole
            //Console.Clear();
            //PrintTreeStats(root);
            //MctsTreePrinter.Print(root, $"Ruch gracza: {gra.AktywnyGracz.Nazwa}");
            return najlepszy;
        }
        finally
        {
            MctsNodePool.ReturnTree(root);
        }
    }

    public T WybierzAkcjePosrednia<T>(Gra gra, DecyzjaKontekst<T> decyzja)
    {
        if (decyzja.Opcje == null || decyzja.Opcje.Count == 0)
            throw new InvalidOperationException("Brak opcji decyzji posredniej.");

        return decyzja.Opcje[random.Next(decyzja.Opcje.Count)];
    }

    void RunIteration(MctsNode root, string rootPlayerName)
    {
        var node = Select(root);
        node = Expand(node);
        var winner = Simulate(node);
        Backpropagate(node, winner, rootPlayerName);
    }

    MctsNode Select(MctsNode node)
    {
        while (!node.Gra.CzyKoniecGry() &&
               node.NieprzetestowaneRuchy.Count == 0 &&
               node.Dzieci.Count > 0)
        {
            node = BestUcbChild(node);
        }

        return node;
    }

    MctsNode Expand(MctsNode node)
    {
        if (node.Gra.CzyKoniecGry() || !node.NieprzetestowaneRuchy.Any())
            return node;

        int index = random.Next(node.NieprzetestowaneRuchy.Count);
        Ruch move = node.NieprzetestowaneRuchy[index];
        node.NieprzetestowaneRuchy.RemoveAt(index);

        var newGame = node.Gra.Clone();
        newGame.WykonajRuch(move, rolloutResolver, random);

        var child = MctsNode.Create(newGame, node, move);
        node.Dzieci.Add(child);

        return child;
    }

    Gracz Simulate(MctsNode node)
    {
        var gra = node.Gra;

        if (ReshuffleInRollout)
        {
            // Create a fresh simulation game for this rollout and reshuffle hidden cards.
            //simulationGame ??= gra.Clone();
            //simulationGame.CopyFrom(gra);         // reuse zamiast Clone
            //simulationGame.PotasujZakryteKarty(random);
            var temp = gra.Clone();
            temp.PotasujZakryteKarty(random);
            simulationGame = temp;
        }
        else
        {
            // Reuse allocated simulation game for performance and copy current state into it.
            simulationGame ??= gra.Clone();
            simulationGame.CopyFrom(gra);
        }

        ZarejestrujDeterminizacje(node, simulationGame);

        while (!simulationGame.CzyKoniecGry())
        {
            var ruch = rolloutAgent.WybierzRuch(simulationGame);
            simulationGame.WykonajRuch(ruch, rolloutResolver, random);
        }

        simulationGame.ZakonczGre();
        
        return simulationGame.StanGry.Zwyciezca;
    }

    void ZarejestrujDeterminizacje(MctsNode node, Gra deterGra)
    {
        var plansza = deterGra.PlanszaEpoki;
        if (plansza == null) return;

        node.ZakrytePola ??= new Dictionary<int, Dictionary<string, int>>();

        for (int i = 0; i < plansza.Pola.Count; i++)
        {
            var pole = plansza.Pola[i];
            if (!pole.CzyZakryta || pole.Karta == null) continue;

            if (!node.ZakrytePola.TryGetValue(i, out var liczniki))
            {
                liczniki = new Dictionary<string, int>();
                node.ZakrytePola[i] = liczniki;
            }

            string nazwa = pole.Karta.Nazwa;
            liczniki.TryGetValue(nazwa, out int ile);
            liczniki[nazwa] = ile + 1;
        }
    }

    void Backpropagate(MctsNode node, Gracz winner, string rootPlayerName)
    {
        while (node != null)
        {
            node.Wizyty++;

            if (winner != null && winner.Nazwa == rootPlayerName)
                node.Wygrane++;

            node = node.Rodzic;
        }
    }

    MctsNode BestUcbChild(MctsNode node)
    {
        MctsNode best = null;
        double bestValue = double.MinValue;
        double logNodeVisits = Math.Log(node.Wizyty + 1);

        // przejdz po dzieciach i wybierz tego z najwyższym UCB
        foreach (var child in node.Dzieci)
        {
            double ucb;
            if (child.Wizyty == 0) ucb = double.MaxValue;
            else ucb = (child.Wygrane / child.Wizyty) + C * Math.Sqrt(logNodeVisits / child.Wizyty);

            if (ucb > bestValue)
            {
                bestValue = ucb;
                best = child;
            }
        }
        return best;
    }

    Ruch NajlepszyRuch(MctsNode root)
    {
        if (root.Dzieci.Any())
        {
            return root.Dzieci
                .OrderByDescending(c => c.Wizyty)
                .ThenByDescending(c => c.Wygrane / (double)Math.Max(1, c.Wizyty))
                .First()
                .Ruch!;
        }

        if (root.NieprzetestowaneRuchy.Any())
            return root.NieprzetestowaneRuchy[0];

        throw new InvalidOperationException("MCTS nie znalazl zadnego ruchu.");
    }

    public static void PrintTreeStats(MctsNode root)
    {
        int totalNodes = 0;
        int maxDepth = 0;
        var nodesByDepth = new Dictionary<int, (int count, int totalVisits)>();

        var stack = new Stack<(MctsNode node, int depth)>();
        stack.Push((root, 0));

        while (stack.Count > 0)
        {
            var (node, depth) = stack.Pop();
            totalNodes++;
            maxDepth = Math.Max(maxDepth, depth);

            if (!nodesByDepth.ContainsKey(depth))
                nodesByDepth[depth] = (0, 0);
            var (c, v) = nodesByDepth[depth];
            nodesByDepth[depth] = (c + 1, v + node.Wizyty);

            foreach (var child in node.Dzieci)
                stack.Push((child, depth + 1));
        }

        Console.WriteLine($"\n── Tree stats ({Iterations} iteracji) ──");
        Console.WriteLine($"Węzłów łącznie: {totalNodes}, max głębokość: {maxDepth}");
        for (int d = 0; d <= maxDepth; d++)
        {
            var (count, visits) = nodesByDepth.GetValueOrDefault(d);
            double avgVisits = count > 0 ? (double)visits / count : 0;
            string bar = new string('█', Math.Min(count, 40));
            Console.WriteLine($"  głęb {d}: {count,4} węzłów  śr.wizyt: {avgVisits,5:F1}  {bar}");
        }
    }
}