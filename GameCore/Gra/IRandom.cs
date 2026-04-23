using System;
using System.Collections.Generic;
using System.Text;
public interface IRandom
{
    int Next();
    int Next(int maxValue);
    int Next(int minValue, int maxValue);
    double NextDouble();
    double NextDouble(double maxValue);
    double NextDouble(double minValue, double maxValue);
}
