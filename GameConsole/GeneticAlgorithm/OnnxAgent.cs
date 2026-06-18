using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Interfejs dla modelu polityki (obsluguje zarowno heurystyczne jak i ONNX-based agenty).
/// </summary>
public interface IPolicyModel
{
    /// <summary>
    /// Zwraca logits dla glownych akcji (120 wymiarow).
    /// </summary>
    float[] GetPolicyLogits(float[] stateVector, float[] actionMask);

    /// <summary>
    /// Zwraca wartosc pozycji (estimate expected reward).
    /// </summary>
    float GetValueEstimate(float[] stateVector);

    /// <summary>
    /// Zwraca logits dla subdecyzji (zmienna wymiarowosc).
    /// </summary>
    float[] GetSubdecisionLogits(float[] stateVector, string subdecisionType);
}

/// <summary>
/// Adapter do ONNX Runtime (wymaga: dotnet add package Microsoft.ML.OnnxRuntime).
/// </summary>
public class OnnxPolicyModel : IPolicyModel
{
    private readonly Microsoft.ML.OnnxRuntime.InferenceSession session;
    private readonly string modelPath;

    public OnnxPolicyModel(string modelPath)
    {
        this.modelPath = modelPath;

        if (!File.Exists(modelPath))
            throw new FileNotFoundException($"Model file not found: {modelPath}");

        var sessionOptions = new Microsoft.ML.OnnxRuntime.SessionOptions
        {
            GraphOptimizationLevel = Microsoft.ML.OnnxRuntime.GraphOptimizationLevel.ORT_ENABLE_ALL,
            IntraOpNumThreads = Environment.ProcessorCount
        };

        this.session = new Microsoft.ML.OnnxRuntime.InferenceSession(modelPath, sessionOptions);
    }

    public float[] GetPolicyLogits(float[] stateVector, float[] actionMask)
    {
        if (stateVector == null || stateVector.Length != ActionSpace.StateVectorSize)
            throw new ArgumentException($"State vector must be {ActionSpace.StateVectorSize}-dimensional");
        if (actionMask == null || actionMask.Length != ActionSpace.TotalPrimaryActions)
            throw new ArgumentException($"Action mask must be {ActionSpace.TotalPrimaryActions}-dimensional");

        var inputs = new List<Microsoft.ML.OnnxRuntime.NamedOnnxValue>
        {
            Microsoft.ML.OnnxRuntime.NamedOnnxValue.CreateFromTensor("state",
                new Microsoft.ML.OnnxRuntime.Tensors.DenseTensor<float>(
                    stateVector, new int[] { 1, ActionSpace.StateVectorSize })),
            Microsoft.ML.OnnxRuntime.NamedOnnxValue.CreateFromTensor("action_mask",
                new Microsoft.ML.OnnxRuntime.Tensors.DenseTensor<float>(
                    actionMask, new int[] { 1, ActionSpace.TotalPrimaryActions }))
        };

        using var results = session.Run(inputs);
        var output = results.FirstOrDefault(x => x.Name == "policy_masked_logits");

        var tensor = output?.Value as Microsoft.ML.OnnxRuntime.Tensors.DenseTensor<float>;
        if (tensor == null)
            throw new InvalidOperationException("Failed to extract policy logits from ONNX output");

        return tensor.ToArray().Skip(0).Take(ActionSpace.TotalPrimaryActions).ToArray();
    }

    public float GetValueEstimate(float[] stateVector)
    {
        if (stateVector == null || stateVector.Length != ActionSpace.StateVectorSize)
            throw new ArgumentException($"State vector must be {ActionSpace.StateVectorSize}-dimensional");

        var inputs = new List<Microsoft.ML.OnnxRuntime.NamedOnnxValue>
        {
            Microsoft.ML.OnnxRuntime.NamedOnnxValue.CreateFromTensor("state",
                new Microsoft.ML.OnnxRuntime.Tensors.DenseTensor<float>(
                    stateVector, new int[] { 1, ActionSpace.StateVectorSize })),
            Microsoft.ML.OnnxRuntime.NamedOnnxValue.CreateFromTensor("action_mask",
                new Microsoft.ML.OnnxRuntime.Tensors.DenseTensor<float>(
                    Enumerable.Repeat(1f, ActionSpace.TotalPrimaryActions).ToArray(),
                    new int[] { 1, ActionSpace.TotalPrimaryActions }))
        };

        using var results = session.Run(inputs);
        var output = results.FirstOrDefault(x => x.Name == "value");

        var tensor = output?.Value as Microsoft.ML.OnnxRuntime.Tensors.DenseTensor<float>;
        if (tensor == null)
            throw new InvalidOperationException("Failed to extract value from ONNX output");

        return tensor[0];
    }

    public float[] GetSubdecisionLogits(float[] stateVector, string subdecisionType)
    {
        // W pełnym wdrażaniu: załaduj osobny ONNX model lub export z wieloma output heads
        throw new NotImplementedException(
            "Subdecision inference from ONNX requires multi-head export. " +
            "See game_network.py for architecture details.");
    }

    public void Dispose()
    {
        session?.Dispose();
    }
}

/// <summary>
/// Agent oparty na ONNX Policy Model.
/// </summary>
public class OnnxAgent : IAgent
{
    private readonly IPolicyModel policyModel;
    private readonly IRandom random;

    public string Name { get; set; } = "OnnxAgent";

    public OnnxAgent(IPolicyModel policyModel, IRandom random)
    {
        this.policyModel = policyModel ?? throw new ArgumentNullException(nameof(policyModel));
        this.random = random ?? throw new ArgumentNullException(nameof(random));
    }

    public Ruch WybierzRuch(Gra gra)
    {
        if (gra == null)
            throw new ArgumentNullException(nameof(gra));

        // Kodowanie stanu
        var encoding = GameStateEncoder.EncodePolicy(gra);
        var stateVector = encoding.State;
        var actionMask = encoding.ActionMask;

        // Pobranie logits z modelu
        var policyLogits = policyModel.GetPolicyLogits(stateVector, actionMask);

        // Softmax
        var softmax = ComputeSoftmax(policyLogits);

        // Sampling z rozkladu
        var actionIndex = SampleFromDistribution(softmax, random);

        // Dekodowanie akcji
        return DecodeActionToMove(gra, actionIndex);
    }

    public T WybierzAkcjePosrednia<T>(Gra gra, DecyzjaKontekst<T> decyzja)
    {
        // Uproszczenie: losowy wybor z opcji
        if (decyzja.Opcje == null || decyzja.Opcje.Count == 0)
            throw new InvalidOperationException("No options available for subdecision");

        return decyzja.Opcje[random.Next(decyzja.Opcje.Count)];
    }

    private float[] ComputeSoftmax(float[] logits)
    {
        var max = logits.Max();
        var exp = logits.Select(x => (float)Math.Exp(x - max)).ToArray();
        var sum = exp.Sum();
        return exp.Select(x => x / sum).ToArray();
    }

    private int SampleFromDistribution(float[] distribution, IRandom random)
    {
        var cdf = 0f;
        var r = (float)random.NextDouble();

        for (int i = 0; i < distribution.Length; i++)
        {
            cdf += distribution[i];
            if (r < cdf)
                return i;
        }

        return distribution.Length - 1;
    }

    private Ruch DecodeActionToMove(Gra gra, int actionIndex)
    {
        // Dekoduj actionIndex (0-119) z powrotem na (slotIndex, TypRuchu, wonderIndex)
        int slotIndex = actionIndex / ActionSpace.ActionsPerSlot;
        int actionType = actionIndex % ActionSpace.ActionsPerSlot;

        var pola = gra.PlanszaEpoki.Pola;
        if (slotIndex >= pola.Count())
            throw new InvalidOperationException($"Invalid slot index: {slotIndex}");

        var karta = pola[slotIndex].Karta;
        if (karta == null)
            throw new InvalidOperationException($"No card at slot {slotIndex}");

        if (actionType == 0)
            return new Ruch(gra.AktywnyGracz, gra.Przeciwnik, karta, TypRuchu.ZbudujKarte);

        if (actionType == 1)
            return new Ruch(gra.AktywnyGracz, gra.Przeciwnik, karta, TypRuchu.OdrzucKarte);

        if (actionType >= 2 && actionType <= 5)
        {
            return new Ruch(
                gra.AktywnyGracz,
                gra.Przeciwnik,
                karta,
                TypRuchu.ZbudujCud,
                GetWonderCard(gra, actionType - 2));
        }

        throw new ArgumentException($"Invalid action type: {actionType}");
    }

    private KartaCudu GetWonderCard(Gra gra, int wonderIndex)
    {
        if (wonderIndex < 0 || wonderIndex >= 4)
            throw new ArgumentException($"Invalid wonder index: {wonderIndex}");

        var cuda = gra.AktywnyGracz.KartyCudow;
        if (wonderIndex >= cuda.Count)
            throw new InvalidOperationException(
                $"Wonder index {wonderIndex} out of range (player has {cuda.Count} wonders)");

        return cuda[wonderIndex];
    }
}

/// <summary>
/// Helper do eksportu danych treningowych z symulacji do JSON.
/// </summary>
public static class TrainingDataExporter
{
    public class GameStep
    {
        public float[] State { get; set; }
        public float[] ActionMask { get; set; }
        public int ActionTaken { get; set; }
        public float ValueTarget { get; set; }
        public float Advantage { get; set; }
        public Dictionary<string, object> Subdecisions { get; set; }
    }

    //public static void ExportToJson(List<MoveLog> moves, Gra gra, string outputPath)
    //{
    //    var steps = new List<GameStep>();

    //    foreach (var move in moves)
    //    {
    //        var encoding = GameStateEncoder.EncodePolicy(gra);

    //        var step = new GameStep
    //        {
    //            State = encoding.State,
    //            ActionMask = encoding.ActionMask,
    //            ActionTaken = 0, // TODO: Zdekoduj z move
    //            ValueTarget = 0f, // TODO: Oblicz z wyniku gry
    //            Advantage = 0f, // TODO: Oblicz GAE
    //            Subdecisions = move.Decisions
    //                .Select(d => new { d.TypDecyzji, d.Wybor })
    //                .Cast<object>()
    //                .ToList()
    //                .ToDictionary(x => x.ToString(), x => x)
    //        };

    //        steps.Add(step);
    //    }

    //    var json = System.Text.Json.JsonSerializer.Serialize(steps, 
    //        new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    //    System.IO.File.WriteAllText(outputPath, json);
    //}
}
