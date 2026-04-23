using System;
using System.Collections.Generic;
using System.Linq;

public class MctsAgent : IAgent
{
    public string Name { get; set; } = "MCTS";
    public int Iterations { get; set; } = 300;

    private readonly IRandom random;
    private MctsNode? currentRoot;
    //private Gra simulationGame;

    public MctsAgent(IRandom random)
    {
        this.random = random;
    }

    public Ruch WybierzRuch(Gra gra)
    {
        if (gra == null) throw new ArgumentNullException(nameof(gra));

        var root = new MctsNode(gra.Clone(), null);

        if (!root.NieprzetestowaneRuchy.Any() && !root.Dzieci.Any())
            throw new InvalidOperationException("MCTS nie znalazl zadnego legalnego ruchu.");

        for (int i = 0; i < Iterations; i++)
            RunIteration(root, gra.AktywnyGracz.Nazwa);

        var najlepszy = NajlepszyRuch(root);
        root = root.Dzieci.FirstOrDefault(c => c.Ruch == najlepszy);

        return najlepszy;
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
        //var winner = Simulate(node.Gra, simulationGame);
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

    //Gracz Simulate(Gra gra, Gra simulationGame)
    Gracz Simulate(Gra gra)
    {
        var simulationGame = gra.Clone();
        //simulationGame.CopyFrom(gra);
        var randomAgent = new RandomAgent(random);
        var decisionResolver = new SimulationDecisionResolver(randomAgent);
        simulationGame.PotasujZakryteKarty(random);

        while (!simulationGame.CzyKoniecGry())
        {
            var ruch = randomAgent.WybierzRuch(simulationGame);
            simulationGame.WykonajRuch(ruch, decisionResolver, random);
        }

        simulationGame.ZakonczGre();
        return simulationGame.StanGry.Zwyciezca;
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
        const double C = 1.414;
        MctsNode best = null;
        double bestValue = double.MinValue;
        double logNodeVisits = Math.Log(node.Wizyty + 1);

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

    //private MctsNode? ZnajdzNowyKorzen(Gra aktualnaGra, Ruch ostatniRuchPrzeciwnika)
    //{
    //    if (currentRoot == null) return null;

    //    // Szukamy wśród dzieci starego korzenia tego, który powstał przez 'ostatniRuchPrzeciwnika'
    //    var potencjalnyNowyKorzen = currentRoot.Dzieci
    //        .FirstOrDefault(child => child.Ruch != null && child.Ruch.Equals(ostatniRuchPrzeciwnika));

    //    // Opcjonalnie: sprawdź, czy stan gry faktycznie się zgadza (sanity check)
    //    // if (potencjalnyNowyKorzen != null && !SaTakieSame(potencjalnyNowyKorzen.Gra, aktualnaGra)) 
    //    //    return null; 

    //    return potencjalnyNowyKorzen;
    //}
}