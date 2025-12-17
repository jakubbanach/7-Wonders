using System.Collections.Generic;
using System.Linq;

public static class ZbiorStref
{
    public static IReadOnlyList<Strefa> Strefy { get; }
    static ZbiorStref()
    {
        Strefy = InicjalizujStrefy();
    }
    private static List<Strefa> InicjalizujStrefy()
    {
        var strefy = new List<Strefa>
        {
            new(
                "Zwyciestwo B",
                1,
                0,
                100000,
                false,
                false
            ),
            new(
                "Strefa 3 dla B",
                3,
                5,
                10,
                false,
                false
            ),
            new(
                "Strefa 2 dla B",
                3,
                2,
                5,
                false,
                false
            ),
            new(
                "Strefa 1 dla B",
                2,
                0,
                2,
                false,
                false
            ),
            new(
                "Startowa",
                1,
                0,
                0,
                true,
                true
            ),
            new(
                "Strefa 1 dla A",
                2,
                0,
                2,
                false,
                false
            ),
            new(
                "Strefa 2 dla A",
                3,
                2,
                5,
                false,
                false
            ),
            new(
                "Strefa 3 dla A",
                3,
                5,
                10,
                false,
                false
            ),
            new(
                "Zwyciestwo A",
                1,
                0,
                100000,
                false,
                false
            ),
        };
        return strefy;
    }
}