using System;
using System.Collections.Generic;
using System.Linq;

public class LearningIsMctsAgent : IAgent, IPolicyTargetProvider, IDisposable
{
    private readonly IRandom random;
    private readonly IPolicyModel policyModel;
    private readonly bool ownsPolicyModel;
    private readonly RandomAgent rolloutAgent;
    private readonly SimulationDecisionResolver rolloutResolver;

    private float[]? lastPolicyTarget;

    public string Name { get; set; } = "Learning-IS-MCTS";
    public int Iterations { get; set; } = 300;
    public float CPuct { get; set; } = 1.414f;
    public bool UsePolicyPriors { get; set; } = true;
    public bool UseValueEvaluation { get; set; } = true;
    public int RolloutDepth { get; set; } = 0;

    public LearningIsMctsAgent(IRandom random, string modelPath, int iterations = 300, float cPuct = 1.414f)
        : this(random, new OnnxPolicyModel(modelPath), iterations, cPuct, ownsPolicyModel: true)
    {
    }

    public LearningIsMctsAgent(IRandom random, IPolicyModel policyModel, int iterations = 300, float cPuct = 1.414f, bool ownsPolicyModel = false)
    {
        this.random = random ?? throw new ArgumentNullException(nameof(random));
        this.policyModel = policyModel ?? throw new ArgumentNullException(nameof(policyModel));
        this.ownsPolicyModel = ownsPolicyModel;
        Iterations = iterations;
        CPuct = cPuct;

        rolloutAgent = new RandomAgent(random);
        rolloutResolver = new SimulationDecisionResolver(rolloutAgent);
    }

    public Ruch WybierzRuch(Gra gra)
    {
        if (gra == null) throw new ArgumentNullException(nameof(gra));

        lastPolicyTarget = null;

        var rootGame = gra.Clone();
        var root = MctsNode.CreateIS(rootGame, rodzic: null, ruch: null);

        if (rootGame.DostepneRuchy().Count == 0)
            throw new InvalidOperationException("Learning IS-MCTS: brak legalnych ruchow.");

        string rootPlayerName = gra.AktywnyGracz.Nazwa;

        try
        {
            for (int i = 0; i < Iterations; i++)
                RunIteration(root, rootPlayerName);

            lastPolicyTarget = BuildPolicyTarget(root);
            return NajlepszyRuch(root);
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

    public bool TryGetPolicyTarget(out float[] policyTarget)
    {
        if (lastPolicyTarget != null)
        {
            policyTarget = (float[])lastPolicyTarget.Clone();
            return true;
        }

        policyTarget = Array.Empty<float>();
        return false;
    }

    public void Dispose()
    {
        if (ownsPolicyModel && policyModel is IDisposable disposable)
            disposable.Dispose();
    }

    void RunIteration(MctsNode root, string rootPlayerName)
    {
        var deterGra = root.Gra.Clone();
        deterGra.PotasujZakryteKarty(random);

        var node = Select(root, deterGra);
        node = Expand(node, deterGra);

        if (RolloutDepth > 0)
            PlayRandomPrefix(deterGra, RolloutDepth);

        var value = Evaluate(deterGra, rootPlayerName);
        Backpropagate(node, value);
    }

    MctsNode Select(MctsNode node, Gra deterGra)
    {
        while (!deterGra.CzyKoniecGry())
        {
            var legalneRuchy = deterGra.DostepneRuchy();
            var legalneDzieci = node.Dzieci
                .Where(c => c.Ruch != null && legalneRuchy.Any(m => RuchyRowne(m, c.Ruch!)))
                .ToList();

            bool saNiesprobowane = legalneRuchy
                .Any(m => !node.Dzieci.Any(c => c.Ruch != null && RuchyRowne(m, c.Ruch!)));

            if (saNiesprobowane || legalneDzieci.Count == 0)
                break;

            EnsurePolicyPriors(node);

            foreach (var child in legalneDzieci)
                child.Dostepnosc++;

            node = BestPuctChild(node, legalneDzieci);
            deterGra.WykonajRuch(node.Ruch!, rolloutResolver, random);
            ZarejestrujDeterminizacje(node, deterGra);
        }

        return node;
    }

    MctsNode Expand(MctsNode node, Gra deterGra)
    {
        if (deterGra.CzyKoniecGry())
            return node;

        var legalneRuchy = deterGra.DostepneRuchy();
        var niesprobowane = legalneRuchy
            .Where(m => !node.Dzieci.Any(c => c.Ruch != null && RuchyRowne(m, c.Ruch!)))
            .ToList();

        if (niesprobowane.Count == 0)
            return node;

        EnsurePolicyPriors(node);

        var ruch = SelectExpansionMove(node, deterGra, niesprobowane);
        var actionIndex = GameStateEncoder.GetActionIndex(deterGra, ruch);

        deterGra.WykonajRuch(ruch, rolloutResolver, random);
        var child = MctsNode.CreateIS(deterGra.Clone(), rodzic: node, ruch: ruch);
        child.ActionIndex = actionIndex;
        child.PolicyPrior = GetPolicyPrior(node, actionIndex);
        child.Dostepnosc = 1;
        ZarejestrujDeterminizacje(child, deterGra);
        node.Dzieci.Add(child);

        return child;
    }

    Ruch SelectExpansionMove(MctsNode node, Gra gra, List<Ruch> moves)
    {
        if (!UsePolicyPriors || node.PolicyPriors == null)
            return moves[random.Next(moves.Count)];

        double total = 0d;
        var weights = new double[moves.Count];

        for (int i = 0; i < moves.Count; i++)
        {
            int actionIndex = GameStateEncoder.GetActionIndex(gra, moves[i]);
            weights[i] = Math.Max(0d, GetPolicyPrior(node, actionIndex));
            total += weights[i];
        }

        if (total <= 0d)
            return moves[random.Next(moves.Count)];

        double sample = random.NextDouble() * total;
        double cdf = 0d;
        for (int i = 0; i < moves.Count; i++)
        {
            cdf += weights[i];
            if (sample <= cdf)
                return moves[i];
        }

        return moves[moves.Count - 1];
    }

    MctsNode BestPuctChild(MctsNode parent, List<MctsNode> legalneDzieci)
    {
        MctsNode? best = null;
        double bestValue = double.MinValue;

        foreach (var child in legalneDzieci)
        {
            double q = child.Wizyty > 0 ? child.Wygrane / child.Wizyty : 0d;
            double availability = Math.Sqrt(Math.Max(1, child.Dostepnosc));
            double prior = UsePolicyPriors ? child.PolicyPrior : 1d / Math.Max(1, legalneDzieci.Count);
            double u = CPuct * prior * availability / (1d + child.Wizyty);
            double puct = q + u;

            if (puct > bestValue)
            {
                bestValue = puct;
                best = child;
            }
        }

        return best!;
    }

    float Evaluate(Gra gra, string rootPlayerName)
    {
        if (gra.CzyKoniecGry())
        {
            gra.ZakonczGre();
            return EvaluateTerminal(gra, rootPlayerName);
        }

        if (!UseValueEvaluation)
            return RolloutToTerminal(gra, rootPlayerName);

        var state = GameStateEncoder.Encode(gra);
        float value = policyModel.GetValueEstimate(state);

        if (!string.Equals(gra.AktywnyGracz.Nazwa, rootPlayerName, StringComparison.Ordinal))
            value = -value;

        return Clamp(value, -1f, 1f);
    }

    float RolloutToTerminal(Gra gra, string rootPlayerName)
    {
        while (!gra.CzyKoniecGry())
        {
            var ruch = rolloutAgent.WybierzRuch(gra);
            gra.WykonajRuch(ruch, rolloutResolver, random);
        }

        gra.ZakonczGre();
        return EvaluateTerminal(gra, rootPlayerName);
    }

    void PlayRandomPrefix(Gra gra, int depth)
    {
        for (int i = 0; i < depth && !gra.CzyKoniecGry(); i++)
        {
            var ruch = rolloutAgent.WybierzRuch(gra);
            gra.WykonajRuch(ruch, rolloutResolver, random);
        }
    }

    static float EvaluateTerminal(Gra gra, string rootPlayerName)
    {
        var stan = gra.StanGry;

        if (stan.Zwyciezca == null)
            return 0f;

        return string.Equals(stan.Zwyciezca.Nazwa, rootPlayerName, StringComparison.Ordinal)
            ? 1f
            : -1f;
    }

    void Backpropagate(MctsNode node, float value)
    {
        while (node != null)
        {
            node.Wizyty++;
            node.Wygrane += value;
            node = node.Rodzic;
        }
    }

    void EnsurePolicyPriors(MctsNode node)
    {
        if (!UsePolicyPriors || node.PolicyPriors != null)
            return;

        var encoding = GameStateEncoder.EncodePolicy(node.Gra);
        var logits = policyModel.GetPolicyLogits(encoding.State, encoding.ActionMask);
        node.PolicyPriors = MaskedSoftmax(logits, encoding.ActionMask);
    }

    float GetPolicyPrior(MctsNode node, int actionIndex)
    {
        if (node.PolicyPriors == null || actionIndex < 0 || actionIndex >= node.PolicyPriors.Length)
            return 0f;

        return node.PolicyPriors[actionIndex];
    }

    float[] MaskedSoftmax(float[] logits, float[] mask)
    {
        if (logits.Length != mask.Length)
            throw new ArgumentException("Logits and mask must have the same length.");

        var priors = new float[logits.Length];
        float max = float.NegativeInfinity;

        for (int i = 0; i < logits.Length; i++)
        {
            if (mask[i] > 0.5f && logits[i] > max)
                max = logits[i];
        }

        if (float.IsNegativeInfinity(max))
            return priors;

        double sum = 0d;
        for (int i = 0; i < logits.Length; i++)
        {
            if (mask[i] <= 0.5f)
                continue;

            priors[i] = (float)Math.Exp(logits[i] - max);
            sum += priors[i];
        }

        if (sum <= 0d)
            return priors;

        for (int i = 0; i < priors.Length; i++)
            priors[i] = mask[i] > 0.5f ? priors[i] / (float)sum : 0f;

        return priors;
    }

    float[] BuildPolicyTarget(MctsNode root)
    {
        var target = new float[ActionSpace.TotalPrimaryActions];
        float totalVisits = root.Dzieci.Sum(child => child.Wizyty);

        if (totalVisits <= 0f)
            return target;

        foreach (var child in root.Dzieci)
        {
            if (child.ActionIndex >= 0 && child.ActionIndex < target.Length)
                target[child.ActionIndex] = child.Wizyty / totalVisits;
        }

        return target;
    }

    Ruch NajlepszyRuch(MctsNode root)
    {
        if (!root.Dzieci.Any())
            throw new InvalidOperationException("Learning IS-MCTS: brak dzieci korzenia po iteracjach.");

        return root.Dzieci
            .OrderByDescending(c => c.Wizyty)
            .ThenByDescending(c => c.Wizyty > 0 ? c.Wygrane / c.Wizyty : double.MinValue)
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

    static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}
