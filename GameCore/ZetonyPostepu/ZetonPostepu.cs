using System.Collections.Generic;
using System.Linq;

public class ZetonPostepu
{
    public string Nazwa { get; protected set; }
    public List<Efekt> Efekty { get; protected set; }

    public ZetonPostepu(string nazwa, List<Efekt> efekty)
    {
        Nazwa = nazwa;
        Efekty = efekty;
    }

    private ZetonPostepu(ZetonPostepu zeton)
    {
        Nazwa = zeton.Nazwa;
        Efekty = zeton.Efekty.Select(e => e.Clone()).ToList();
    }
    public ZetonPostepu Clone()
    {
        return new ZetonPostepu(this);
    }

    public override string ToString()
    {
        return Nazwa + (Efekty != null && Efekty.Count > 0 ? $" ({string.Join(", ", Efekty.Select(e => e.Wypisz()))})" : "");
    }
}
