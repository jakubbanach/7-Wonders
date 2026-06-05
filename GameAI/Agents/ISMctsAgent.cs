using System;
using System.Collections.Generic;
using System.Linq;

public class ISMctsAgent : IAgent
{
    public string Name { get; set; } = "IS-MCTS";
    public int Iterations { get; set; } = 300;

    private const double UcbC = 1.414;

    private readonly IRandom random;
    private readonly RandomAgent rolloutAgent;
    private readonly SimulationDecisionResolver rolloutResolver;

    public ISMctsAgent(IRandom random)
    {
        this.random = random;
        rolloutAgent = new RandomAgent(random);
        rolloutResolver = new SimulationDecisionResolver(rolloutAgent);
    }
    
    public Ruch WybierzRuch(Gra gra)
    {
        if (gra == null) throw new ArgumentNullException(nameof(gra));

        var rootGame = gra.Clone();
        var root = MctsNode.CreateIS(rootGame, rodzic: null, ruch: null);

        if (root.Dzieci.Count == 0 && rootGame.DostepneRuchy().Count == 0)
            throw new InvalidOperationException("IS-MCTS: brak legalnych ruchow.");

        string rootPlayerName = gra.AktywnyGracz.Nazwa;

        try
        {
            for (int i = 0; i < Iterations; i++)
                RunIteration(root, rootPlayerName);
            var najlepszy = NajlepszyRuch(root);
            //Console.Clear();
            //MctsAgent.PrintTreeStats(root);
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
            throw new InvalidOperationException("Brak opcji decyzji pośredniej.");

        return decyzja.Opcje[random.Next(decyzja.Opcje.Count)];
    }

    void RunIteration(MctsNode root, string rootPlayerName)
    {
        var deterGra = root.Gra.Clone();
        deterGra.PotasujZakryteKarty(random);

        var node = SelectIS(root, deterGra);
        node = ExpandIS(node, deterGra);
        var winner = Simulate(deterGra);
        BackpropagateIS(node, winner, rootPlayerName);
    }

    MctsNode SelectIS(MctsNode node, Gra deterGra)
    {
        while (!deterGra.CzyKoniecGry())
        {
            var ruchy = deterGra.DostepneRuchy();

            // mozliwe do osiagniecia wezly w obecnej determinizacji
            var legalneRuchy = node.Dzieci
                .Where(c => c.Ruch != null && ruchy.Any(m => RuchyRowne(m, c.Ruch!)))
                .ToList();

            // czy ktorys ruch nie zostal jeszcze przetestowany
            bool nieprzetestowaneRuchy = ruchy
                .Any(m => !node.Dzieci.Any(c => c.Ruch != null && RuchyRowne(m, c.Ruch!)));

            if (nieprzetestowaneRuchy || !legalneRuchy.Any())
                break; // przejsc do ExpandIS

            // liczymy ile razy wezel "mogl byc odwiedzony" w danej determinizacji.
            foreach (var child in legalneRuchy)
                child.Dostepnosc++;

            node = BestUcbChildIS(legalneRuchy);

            deterGra.WykonajRuch(node.Ruch!, rolloutResolver, random);
            ZarejestrujDeterminizacje(node, deterGra);
        }

        return node;
    }

    MctsNode ExpandIS(MctsNode node, Gra deterGra)
    {
        if (deterGra.CzyKoniecGry())
            return node;

        var legalneRuchy = deterGra.DostepneRuchy();

        var nieprzetestowane = legalneRuchy
            .Where(m => !node.Dzieci.Any(c => c.Ruch != null && RuchyRowne(m, c.Ruch!)))
            .ToList();

        if (!nieprzetestowane.Any())
            return node;

        var ruch = nieprzetestowane[random.Next(nieprzetestowane.Count)];

        // Wykonaj ruch na deterministycznej kopii — dalszy rollout i backprop
        // będą korzystać z tego samego stanu deterGra
        deterGra.WykonajRuch(ruch, rolloutResolver, random);
        var snapshot = deterGra.Clone();

        var child = MctsNode.CreateIS(snapshot, rodzic: node, ruch: ruch);
        ZarejestrujDeterminizacje(child, deterGra);
        child.Dostepnosc = 1;
        node.Dzieci.Add(child);

        return child;
    }

    Gracz Simulate(Gra gra)
    {
        while (!gra.CzyKoniecGry())
        {
            var ruch = rolloutAgent.WybierzRuch(gra);
            gra.WykonajRuch(ruch, rolloutResolver, random);
        }

        gra.ZakonczGre();
        return gra.StanGry.Zwyciezca;
    }

    void BackpropagateIS(MctsNode node, Gracz winner, string rootPlayerName)
    {
        while (node != null)
        {
            node.Wizyty++;

            if (winner != null && winner.Nazwa == rootPlayerName)
                node.Wygrane++;

            node = node.Rodzic;
        }
    }

    MctsNode BestUcbChildIS(List<MctsNode> legalnedzieci)
    {
        MctsNode? best = null;
        double bestValue = double.MinValue;

        foreach (var child in legalnedzieci)
        {
            double ucb;

            if (child.Wizyty == 0)
                ucb = double.MaxValue;
            else
            {
                // IS-MCTS UCB: log(Dostepnosc) zamiast log(Wizyty_rodzica).
                // Dostepnosc = ile razy węzeł był legalny gdy odwiedzano rodzica,
                // co eliminuje bias wynikający z tego że niektóre ruchy są rzadziej
                // legalne w losowanych determinizacjach.
                double logAvail = Math.Log(Math.Max(1, child.Dostepnosc));
                ucb = (child.Wygrane / child.Wizyty)
                    + UcbC * Math.Sqrt(logAvail / child.Wizyty);
            }

            if (ucb > bestValue)
            {
                bestValue = ucb;
                best = child;
            }
        }

        return best!;
    }

    Ruch NajlepszyRuch(MctsNode root)
    {
        if (!root.Dzieci.Any())
            throw new InvalidOperationException("IS-MCTS: brak dzieci korzenia po iteracjach.");

        return root.Dzieci
            .OrderByDescending(c => c.Wizyty)
            .ThenByDescending(c => c.Wizyty > 0 ? c.Wygrane / (double)c.Wizyty : 0)
            .First()
            .Ruch!;
    }

    static bool RuchyRowne(Ruch a, Ruch b)
    {
        return a.TypRuchu == b.TypRuchu
            && a.KartaDoZagrania?.Nazwa == b.KartaDoZagrania?.Nazwa
            && a.KartaCudu?.Nazwa == b.KartaCudu?.Nazwa;
    }
    void ZarejestrujDeterminizacje(MctsNode node, Gra deterGra)
    {
        var plansza = deterGra.PlanszaEpoki;
        if (plansza == null) return;

        node.ZakrytePola ??= new Dictionary<int, Dictionary<string, int>>();

        for (int i = 0; i < plansza.Pola.Count; i++)
        {
            var pole = plansza.Pola[i];
            // Interesują nas tylko pola które w oryginalnym stanie są zakryte
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
}