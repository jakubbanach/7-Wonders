using System;
using System.Collections.Generic;
using System.Linq;

public class PuctValueMctsAgent : IAgent, IPolicyTargetProvider, IDisposable
{
    private const float DefaultCPuct = 1.414f;

    private readonly IRandom random;
    private readonly IPolicyModel policyModel;
    private readonly bool ownsPolicyModel;
    private readonly RandomAgent rolloutAgent;
    private readonly SimulationDecisionResolver rolloutResolver;

    private float[]? lastPolicyTarget;

    public string Name { get; set; } = "PUCT+Value";
    public int Iterations { get; set; } = 300;
    public bool UseRootDeterminization { get; set; }
    public float CPuct { get; set; } = DefaultCPuct;

    public PuctValueMctsAgent(IRandom random, string modelPath, int iterations = 300, float cPuct = DefaultCPuct, bool useRootDeterminization = false)
        : this(random, new OnnxPolicyModel(modelPath), iterations, cPuct, useRootDeterminization, ownsPolicyModel: true)
    {
    }

    public PuctValueMctsAgent(IRandom random, IPolicyModel policyModel, int iterations = 300, float cPuct = DefaultCPuct, bool useRootDeterminization = false, bool ownsPolicyModel = false)
    {
        this.random = random ?? throw new ArgumentNullException(nameof(random));
        this.policyModel = policyModel ?? throw new ArgumentNullException(nameof(policyModel));
        this.ownsPolicyModel = ownsPolicyModel;
        rolloutAgent = new RandomAgent(random);
        rolloutResolver = new SimulationDecisionResolver(rolloutAgent);
        Iterations = iterations;
        CPuct = cPuct;
        UseRootDeterminization = useRootDeterminization;
    }

    public Ruch WybierzRuch(Gra gra)
    {
        if (gra == null) throw new ArgumentNullException(nameof(gra));

        lastPolicyTarget = null;

        var rootGame = gra.Clone();
        if (UseRootDeterminization)
            rootGame.PotasujZakryteKarty(random);

        var root = MctsNode.Create(rootGame, null);
        EnsurePolicyPriors(root);

        if (!root.NieprzetestowaneRuchy.Any() && !root.Dzieci.Any())
            throw new InvalidOperationException("PUCT+Value nie znalazl zadnego legalnego ruchu.");

        try
        {
            for (int i = 0; i < Iterations; i++)
                RunIteration(root, gra.AktywnyGracz.Nazwa);

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
        var node = Select(root);
        node = Expand(node);
        var value = Evaluate(node.Gra, rootPlayerName);
        Backpropagate(node, value);
    }

    MctsNode Select(MctsNode node)
    {
        while (!node.Gra.CzyKoniecGry() &&
               node.NieprzetestowaneRuchy.Count == 0 &&
               node.Dzieci.Count > 0)
        {
            node = BestPuctChild(node);
        }

        return node;
    }

    MctsNode Expand(MctsNode node)
    {
        if (node.Gra.CzyKoniecGry() || !node.NieprzetestowaneRuchy.Any())
            return node;

        EnsurePolicyPriors(node);

        int index = random.Next(node.NieprzetestowaneRuchy.Count);
        var move = node.NieprzetestowaneRuchy[index];
        node.NieprzetestowaneRuchy.RemoveAt(index);

        var newGame = node.Gra.Clone();
        var actionIndex = GameStateEncoder.GetActionIndex(node.Gra, move);
        newGame.WykonajRuch(move, rolloutResolver, random);

        var child = MctsNode.Create(newGame, node, move);
        child.ActionIndex = actionIndex;
        child.PolicyPrior = actionIndex >= 0 && node.PolicyPriors != null && actionIndex < node.PolicyPriors.Length
            ? node.PolicyPriors[actionIndex]
            : 0f;
        node.Dzieci.Add(child);

        return child;
    }

    float Evaluate(Gra gra, string rootPlayerName)
    {
        var encoding = GameStateEncoder.Encode(gra);
        var value = policyModel.GetValueEstimate(encoding);

        if (!string.Equals(gra.AktywnyGracz.Nazwa, rootPlayerName, StringComparison.Ordinal))
            value = -value;

        return value;
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

    MctsNode BestPuctChild(MctsNode node)
    {
        MctsNode best = null!;
        double bestValue = double.MinValue;
        double sqrtVisits = Math.Sqrt(Math.Max(1, node.Wizyty));

        foreach (var child in node.Dzieci)
        {
            double q = child.Wizyty > 0 ? child.Wygrane / child.Wizyty : 0d;
            double u = CPuct * child.PolicyPrior * sqrtVisits / (1d + child.Wizyty);
            double puct = q + u;

            if (puct > bestValue)
            {
                bestValue = puct;
                best = child;
            }
        }

        return best;
    }

    void EnsurePolicyPriors(MctsNode node)
    {
        if (node.PolicyPriors != null)
            return;

        var encoding = GameStateEncoder.EncodePolicy(node.Gra);
        var logits = policyModel.GetPolicyLogits(encoding.State, encoding.ActionMask);
        node.PolicyPriors = MaskedSoftmax(logits, encoding.ActionMask);
    }

    float[] MaskedSoftmax(float[] logits, float[] mask)
    {
        if (logits == null) throw new ArgumentNullException(nameof(logits));
        if (mask == null) throw new ArgumentNullException(nameof(mask));
        if (logits.Length != mask.Length)
            throw new ArgumentException("Logits and mask must have the same length.");

        var priors = new float[logits.Length];
        float max = float.NegativeInfinity;

        for (int i = 0; i < logits.Length; i++)
        {
            if (mask[i] <= 0.5f)
                continue;

            if (logits[i] > max)
                max = logits[i];
        }

        if (float.IsNegativeInfinity(max))
            throw new InvalidOperationException("Policy mask does not contain any legal actions.");

        double sum = 0d;
        for (int i = 0; i < logits.Length; i++)
        {
            if (mask[i] <= 0.5f)
                continue;

            priors[i] = (float)Math.Exp(logits[i] - max);
            sum += priors[i];
        }

        if (sum <= 0d)
        {
            int legalActions = mask.Count(x => x > 0.5f);
            if (legalActions == 0)
                throw new InvalidOperationException("Policy mask does not contain any legal actions.");

            float uniform = 1f / legalActions;
            for (int i = 0; i < mask.Length; i++)
            {
                if (mask[i] > 0.5f)
                    priors[i] = uniform;
            }

            return priors;
        }

        for (int i = 0; i < priors.Length; i++)
            priors[i] = mask[i] > 0.5f ? priors[i] / (float)sum : 0f;

        return priors;
    }

    float[] BuildPolicyTarget(MctsNode root)
    {
        var target = new float[ActionSpace.TotalPrimaryActions];
        float totalVisits = root.Dzieci.Sum(child => child.Wizyty);

        if (totalVisits > 0f)
        {
            foreach (var child in root.Dzieci)
            {
                if (child.ActionIndex >= 0 && child.ActionIndex < target.Length)
                    target[child.ActionIndex] = child.Wizyty / totalVisits;
            }

            return target;
        }

        if (root.PolicyPriors != null)
        {
            Array.Copy(root.PolicyPriors, target, Math.Min(root.PolicyPriors.Length, target.Length));
            return target;
        }

        return target;
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

        throw new InvalidOperationException("PUCT+Value nie znalazl zadnego ruchu.");
    }
}