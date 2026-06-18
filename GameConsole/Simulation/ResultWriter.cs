using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class ResultWriter
{
    public static void Save(MatchResult result, string path)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        options.Converters.Add(new JsonStringEnumConverter());
        var json = JsonSerializer.Serialize(result, options);

        File.WriteAllText(path, json);
    }

    public static void Save(SimulationResult result, string path)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        options.Converters.Add(new JsonStringEnumConverter());
        var json = JsonSerializer.Serialize(result, options);

        File.WriteAllText(path, json);
    }

    public static void Save(List<SimulationResult> results, string path)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        var json = JsonSerializer.Serialize(results, options);
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Saves training data in binary .npz format (optimized for neural network training).
    /// </summary>
    public static void SaveBinaryTrainingData(List<MatchResult> matches, string path)
    {
        BinaryTrainingDataWriter.SaveTrainingDataNpz(matches, path);
    }

    /// <summary>
    /// Saves training data in binary .npz format with explicit output path.
    /// </summary>
    public static void SaveBinaryTrainingDataNpz(SimulationResult result, string path)
    {
        if (result.MatchResults == null)
            return;

        BinaryTrainingDataWriter.SaveTrainingDataNpz(result.MatchResults, path);
    }
}