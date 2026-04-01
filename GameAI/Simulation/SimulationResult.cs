using System;
using System.Collections.Generic;
using System.Text;

public class SimulationResult
{
    public int Games;

    public int Agent1Wins;
    public int Agent2Wins;
    public int Draws;

    public double AvgTurns;

    public Dictionary<TypZwyciestwa, int> VictoryTypes;
}