using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

public class SimulationResult
{
    public int TotalGames { get; set; }
    public int Agent1Wins { get; set; }
    public int Agent2Wins { get; set; }
    public int Agent1MaxPoints { get; set; }
    public int Agent2MaxPoints { get; set; }
    public int Agent1MinPoints { get; set; }
    public int Agent2MinPoints { get; set; }
    public double Agent1AveragePoints { get; set; }
    public double Agent2AveragePoints { get; set; }
    //public HeuristicWeights Agent1Weights { get; set; }
    //public HeuristicWeights Agent2Weights { get; set; }
    public List<VictoryTypeStat> VictoryTypeCounts { get; set; }
    //public List<MatchResult> MatchResults { get; set; }

    // używane w trakcie symulacji
    [JsonIgnore]
    public Dictionary<(string Agent, TypZwyciestwa Type), int> VictoryTypeCountsInternal
        = new Dictionary<(string Agent, TypZwyciestwa Type), int>();

    public void PrepareForSerialization()
    {
        VictoryTypeCounts = VictoryTypeCountsInternal
            .Select(kvp => new VictoryTypeStat
            {
                Agent = kvp.Key.Agent,
                TypZwyciestwa = kvp.Key.Type,
                Liczba = kvp.Value
            })
            .ToList();
    }
}