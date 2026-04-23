using System;
using System.Collections.Generic;
using System.Text;

public class RandomAdapter : IRandom
{
    private readonly Random random;

    public RandomAdapter(int seed)
    {
        random = new Random(seed);
    }
    public int Next() => random.Next();
    public int Next(int maxValue) => random.Next(maxValue);
    public int Next(int minValue, int maxValue) => random.Next(minValue, maxValue);
    public double NextDouble() => random.NextDouble();
    public double NextDouble(double maxValue) => random.NextDouble() * maxValue;
    public double NextDouble(double minValue, double maxValue) => minValue + random.NextDouble() * (maxValue - minValue);
}