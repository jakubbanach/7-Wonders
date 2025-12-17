using System.Collections.Generic;

public class ZetonPostepu
{
    public string Nazwa { get; protected set; }
    public List<Efekt> Efekty { get; protected set; }

    public ZetonPostepu(string nazwa, List<Efekt> efekty)
    {
        Nazwa = nazwa;
        Efekty = efekty;
    }
}
