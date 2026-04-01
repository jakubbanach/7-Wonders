using System.Collections.Generic;

public class SimulationResult
{
    public int TotalGames { get; set; }
    public int Agent1Wins { get; set; }
    public int Agent2Wins { get; set; }
    public double Agent1AveragePoints { get; set; }
    public double Agent2AveragePoints { get; set; }
    public Dictionary<TypZwyciestwa, int> VictoryTypeCounts { get; set; }
    public List<MatchResult> MatchResults { get; set; }
}