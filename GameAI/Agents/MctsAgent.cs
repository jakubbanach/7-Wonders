using System;
using System.Collections.Generic;
using System.Linq;

public class MctsAgent : IAgent
{
    public string Name { get; set; } = "MCTS";
    public int Iterations { get; set; } = 300;

    private readonly IRandom random;

    public MctsAgent(IRandom random)
    {
        this.random = random;
    }

    public Ruch WybierzRuch(Gra gra)
    {
        if (gra == null) throw new ArgumentNullException(nameof(gra));

        var rootPlayerName = gra.AktywnyGracz.Nazwa;
        var root = new MctsNode(gra.Clone(), null);

        if (!root.NieprzetestowaneRuchy.Any() && !root.Dzieci.Any())
            throw new InvalidOperationException("MCTS nie znalazl zadnego legalnego ruchu.");

        for (int i = 0; i < Iterations; i++)
            RunIteration(root, rootPlayerName);

        return NajlepszyRuch(root);
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
        var winner = Simulate(node.Gra);
        Backpropagate(node, winner, rootPlayerName);
    }

    MctsNode Select(MctsNode node)
    {
        while (!node.Gra.CzyKoniecGry() &&
               node.NieprzetestowaneRuchy.Count == 0 &&
               node.Dzieci.Any())
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

        var move = node.NieprzetestowaneRuchy[index];
        node.NieprzetestowaneRuchy.RemoveAt(index);

        var newGame = node.Gra.Clone();
        var resolver = new SimulationDecisionResolver(new RandomAgent(random));
        newGame.WykonajRuch(move, resolver, random);

        var child = new MctsNode(newGame, node, move);
        node.Dzieci.Add(child);

        return child;
    }

    Gracz Simulate(Gra gra)
    {
        var simulation = gra;
        var randomAgent = new RandomAgent(random);
        var decisionResolver = new SimulationDecisionResolver(randomAgent);

        while (!simulation.CzyKoniecGry())
        {
            var ruch = randomAgent.WybierzRuch(simulation);
            simulation.WykonajRuch(ruch, decisionResolver, random);
        }

        simulation.ZakonczGre();
        return simulation.StanGry.Zwyciezca;
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
        const double C = 1.4;

        return node.Dzieci
            .OrderByDescending(child =>
                child.Wizyty == 0
                    ? double.PositiveInfinity
                    : (child.Wygrane / (double)child.Wizyty) +
                      C * Math.Sqrt(Math.Log(node.Wizyty + 1) / child.Wizyty))
            .First();
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
}