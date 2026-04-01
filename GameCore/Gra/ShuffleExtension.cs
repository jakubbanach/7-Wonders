using System;
using System.Collections.Generic;
using System.Text;

public static class ShuffleExtensions
{
    public static void Shuffle<T>(this IList<T> list, IRandom random)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);

            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}