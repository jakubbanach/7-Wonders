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
}