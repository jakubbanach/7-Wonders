using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Manager dla ONNX modeli z cachowaniem i load balancingiem.
/// </summary>
public class OnnxModelManager : IDisposable
{
    private readonly Dictionary<string, OnnxPolicyModel> models = new Dictionary<string, OnnxPolicyModel>();
    private readonly Dictionary<string, DateTime> lastUsed = new Dictionary<string, DateTime>();
    private readonly object lockObj = new object();
    private readonly TimeSpan cacheTimeout;

    public OnnxModelManager(TimeSpan? cacheTimeout = null)
    {
        this.cacheTimeout = cacheTimeout ?? TimeSpan.FromMinutes(5);
    }

    /// <summary>
    /// Zaladuj lub pobierz z cache model ONNX.
    /// </summary>
    public OnnxPolicyModel LoadModel(string modelPath)
    {
        lock (lockObj)
        {
            if (models.TryGetValue(modelPath, out var model))
            {
                lastUsed[modelPath] = DateTime.UtcNow;
                return model;
            }

            var newModel = new OnnxPolicyModel(modelPath);
            models[modelPath] = newModel;
            lastUsed[modelPath] = DateTime.UtcNow;

            return newModel;
        }
    }

    /// <summary>
    /// Wyczysc modele nie uzywane przez dluzszy czas.
    /// </summary>
    public void CleanupExpiredModels()
    {
        lock (lockObj)
        {
            var expired = lastUsed
                .Where(x => DateTime.UtcNow - x.Value > cacheTimeout)
                .Select(x => x.Key)
                .ToList();

            foreach (var path in expired)
            {
                models[path].Dispose();
                models.Remove(path);
                lastUsed.Remove(path);
            }
        }
    }

    /// <summary>
    /// Zwroc statystyki uzycia cache.
    /// </summary>
    public (int CachedModels, int TotalAccessTime) GetCacheStats()
    {
        lock (lockObj)
        {
            return (models.Count, lastUsed.Count);
        }
    }

    public void Dispose()
    {
        lock (lockObj)
        {
            foreach (var model in models.Values)
                model.Dispose();
            models.Clear();
        }
    }
}

/// <summary>
/// Serwer inferenceji z REST API dla ONNX modelu.
/// Wymaga: dotnet add package Kestrel
/// </summary>
public class OnnxInferenceServer
{
    private readonly OnnxModelManager modelManager;
    private readonly IPolicyModel policyModel;
    private readonly string modelPath;

    public OnnxInferenceServer(string modelPath)
    {
        this.modelPath = modelPath;
        this.modelManager = new OnnxModelManager();
        this.policyModel = modelManager.LoadModel(modelPath);
    }

    public class InferenceRequest
    {
        public float[] StateVector { get; set; }
        public float[] ActionMask { get; set; }
    }

    public class InferenceResponse
    {
        public float[] PolicyLogits { get; set; }
        public float ValueEstimate { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }

    public InferenceResponse Infer(InferenceRequest request)
    {
        if (request?.StateVector == null || request.ActionMask == null)
            throw new ArgumentException("Request must contain state vector and action mask");

        try
        {
            var policyLogits = policyModel.GetPolicyLogits(request.StateVector, request.ActionMask);
            var valueEstimate = policyModel.GetValueEstimate(request.StateVector);

            return new InferenceResponse
            {
                PolicyLogits = policyLogits,
                ValueEstimate = valueEstimate,
                Metadata = new Dictionary<string, object>
                {
                    { "timestamp", DateTime.UtcNow.ToString("O") },
                    { "model_path", modelPath }
                }
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Inference failed: {ex.Message}", ex);
        }
    }

    public async Task StartAsync(string url = "http://localhost:5000")
    {
        // Implementacja bylaby za pomoca Kestrel middleware
        // Stub tutaj - w pełnym wdrozeniu:
        // var builder = WebApplication.CreateBuilder();
        // var app = builder.Build();
        // app.MapPost("/infer", (InferenceRequest req) => Infer(req));
        // await app.RunAsync(url);
        
        throw new NotImplementedException(
            "REST server requires ASP.NET Core integration. " +
            "See game_training_service.py for Python FastAPI alternative.");
    }

    public void Dispose()
    {
        modelManager?.Dispose();
    }
}

/// <summary>
/// Konfiguracja agenta ONNX z walidacja modelu.
/// </summary>
public class OnnxAgentConfiguration
{
    public string ModelPath { get; set; }
    public int RandomSeed { get; set; } = 42;
    public float TemperatureForSampling { get; set; } = 1.0f;
    public bool UseValueEstimateForMoveSelection { get; set; } = false;

    public OnnxAgent CreateAgent(IRandom random)
    {
        if (string.IsNullOrEmpty(ModelPath))
            throw new ArgumentException("ModelPath must be specified");

        var policyModel = new OnnxPolicyModel(ModelPath);
        return new OnnxAgent(policyModel, random);
    }

    /// <summary>
    /// Waliduj konfig i model ONNX.
    /// </summary>
    public bool Validate(out string errorMessage)
    {
        if (string.IsNullOrEmpty(ModelPath))
        {
            errorMessage = "ModelPath is required";
            return false;
        }

        if (!System.IO.File.Exists(ModelPath))
        {
            errorMessage = $"Model file not found: {ModelPath}";
            return false;
        }

        if (!ModelPath.EndsWith(".onnx", StringComparison.OrdinalIgnoreCase))
        {
            errorMessage = "Model file must have .onnx extension";
            return false;
        }

        try
        {
            var model = new OnnxPolicyModel(ModelPath);
            model.Dispose();
            errorMessage = null;
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = $"Model validation failed: {ex.Message}";
            return false;
        }
    }
}

/// <summary>
/// Performance monitor dla ONNX agenta.
/// </summary>
public class OnnxAgentPerformanceMonitor
{
    private readonly List<long> inferenceTimings = new List<long>();
    private readonly object lockObj = new object();

    public void RecordInferenceTime(long milliseconds)
    {
        lock (lockObj)
        {
            inferenceTimings.Add(milliseconds);
        }
    }

    public class PerformanceStats
    {
        public long TotalInferences { get; set; }
        public double AverageLatencyMs { get; set; }
        public long MinLatencyMs { get; set; }
        public long MaxLatencyMs { get; set; }
        public double ThroughputPerSecond { get; set; }
    }

    public PerformanceStats GetStats()
    {
        lock (lockObj)
        {
            if (inferenceTimings.Count == 0)
                return new PerformanceStats();

            var sorted = inferenceTimings.OrderBy(x => x).ToList();
            var sum = sorted.Sum();
            var avg = (double)sum / sorted.Count;
            var throughput = sorted.Count > 0 ? 1000.0 / avg : 0;

            return new PerformanceStats
            {
                TotalInferences = sorted.Count,
                AverageLatencyMs = avg,
                MinLatencyMs = sorted.First(),
                MaxLatencyMs = sorted.Last(),
                ThroughputPerSecond = throughput
            };
        }
    }

    public void Reset()
    {
        lock (lockObj)
        {
            inferenceTimings.Clear();
        }
    }
}
