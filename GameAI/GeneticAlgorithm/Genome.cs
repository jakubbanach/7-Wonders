using System;

public class Genome
{
    private static readonly Random _random = new Random();

    public HeuristicWeights Weights { get; set; }
    public double Fitness { get; set; }

    public Genome(HeuristicWeights weights) => Weights = weights;

    public static HeuristicWeights Cross(HeuristicWeights p1, HeuristicWeights p2)
    {
        return new HeuristicWeights
        {
            PunktyZwyciestwa = _random.NextDouble() > 0.5 ? p1.PunktyZwyciestwa : p2.PunktyZwyciestwa,
            Wojsko = _random.NextDouble() > 0.5 ? p1.Wojsko : p2.Wojsko,
            Cuda = _random.NextDouble() > 0.5 ? p1.Cuda : p2.Cuda,
            Monety = _random.NextDouble() > 0.5 ? p1.Monety : p2.Monety,
            SymboleNaukowe = _random.NextDouble() > 0.5 ? p1.SymboleNaukowe : p2.SymboleNaukowe
        };
    }
}