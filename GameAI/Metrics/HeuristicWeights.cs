using System;
using System.Collections.Generic;
using System.Text;

public class HeuristicWeights
{
    public int PunktyZwyciestwa = 3;
    public int Cuda = 3;
    public int Monety = 1;
    public int SymboleNaukowe = 4;
    public int Wojsko = 2;

    public HeuristicWeights() { }

    public HeuristicWeights(int punktyZwyciestwa, int cuda, int monety, int symboleNaukowe, int wojsko)
    {
        PunktyZwyciestwa = punktyZwyciestwa;
        Cuda = cuda;
        Monety = monety;
        SymboleNaukowe = symboleNaukowe;
        Wojsko = wojsko;
    }
}
public static class HeuristicWeightPresets
{
    public static HeuristicWeights Balanced() => new HeuristicWeights()
    {
        PunktyZwyciestwa = 10,
        Cuda = 5,
        Monety = 2,
        SymboleNaukowe = 6,
        Wojsko = 4
    };

    public static HeuristicWeights Military() => new HeuristicWeights()
    {
        PunktyZwyciestwa = 8,
        Cuda = 2,
        Monety = 1,
        SymboleNaukowe = 2,
        Wojsko = 10
    };

}