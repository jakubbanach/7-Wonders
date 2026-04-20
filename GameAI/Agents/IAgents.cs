using System;
using System.Collections.Generic;
using System.Linq;

public interface IAgent
{
    string Name { get; set;}

    Ruch WybierzRuch(Gra gra);
    T WybierzAkcjePosrednia<T>(Gra gra, DecyzjaKontekst<T> decyzja);

    public static T WybierzNajlepszyLosowy<T>(
        IEnumerable<T> items,
        Func<T, double> eval,
        IRandom random)
    {
        var scored = items
            .Select(x => (el: x, wynik: eval(x)))
            .ToList();

        var best = scored.Max(x => x.wynik);

        var bestItems = scored
            .Where(x => x.wynik == best)
            .Select(x => x.el)
            .ToList();

        return bestItems[random.Next(bestItems.Count)];
    }
}