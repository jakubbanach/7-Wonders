using System;

public class MatchTimingStat
{
    public int GameNumber { get; set; }
    public int Seed { get; set; }
    public string Agent1Name { get; set; } = null!;
    public string Agent2Name { get; set; } = null!;
    public int Turns { get; set; }
    public long ElapsedMilliseconds { get; set; }
}