using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.Json;

public static class ResultWriter
{
    public static void Save(MatchResult result, string path)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(result, options);

        File.WriteAllText(path, json);
    }
}