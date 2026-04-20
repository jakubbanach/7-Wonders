using System.Collections.Generic;

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
    public Dictionary<TypZwyciestwa, int> VictoryTypeCounts { get; set; }
    public List<MatchResult> MatchResults { get; set; }
}