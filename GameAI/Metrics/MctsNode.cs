using System.Collections.Generic;
using System.Linq;

public class MctsNode
{
    public Gra Gra;
    public MctsNode? Rodzic;
    public List<MctsNode> Dzieci = null!;

    public Ruch? Ruch;
    public int ActionIndex = -1;
    public float PolicyPrior;
    public float[]? PolicyPriors;

    public int Wizyty;
    public double Wygrane;
    public int Dostepnosc; // tylko dla ISMCTS

    public List<Ruch> NieprzetestowaneRuchy = null!;
    public Dictionary<int, Dictionary<string, int>>? ZakrytePola;

    internal MctsNode()
    {
    }

    public static MctsNode Create(Gra gra, MctsNode? rodzic, Ruch? ruch = null)
    {
        var availableMoves = gra.DostepneRuchy();
        var node = MctsNodePool.Rent();
        node.Gra = gra;
        node.Rodzic = rodzic;
        node.Ruch = ruch;
        node.ActionIndex = -1;
        node.PolicyPrior = 0f;
        node.PolicyPriors = null;
        node.Wizyty = 0;
        node.Wygrane = 0;
        node.Dzieci = ListPool<MctsNode>.Rent();
        node.NieprzetestowaneRuchy = ListPool<Ruch>.Rent(availableMoves.Count);
        node.NieprzetestowaneRuchy.AddRange(availableMoves);
        return node;
    }

    public static MctsNode CreateIS(Gra gra, MctsNode? rodzic, Ruch? ruch = null)
    {
        var node = MctsNodePool.Rent();
        node.Gra = gra;
        node.Rodzic = rodzic;
        node.Ruch = ruch;
        node.ActionIndex = -1;
        node.PolicyPrior = 0f;
        node.PolicyPriors = null;
        node.Wizyty = 0;
        node.Wygrane = 0;
        node.Dostepnosc = 0;
        node.Dzieci = ListPool<MctsNode>.Rent();
        node.NieprzetestowaneRuchy = ListPool<Ruch>.Rent(); // pusta — ISMCTS nie korzysta
        return node;
    }
}