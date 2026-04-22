using System.Collections.Generic;
using System.Linq;

class MctsNode
{
    public Gra Gra;
    public MctsNode? Rodzic;
    public List<MctsNode> Dzieci = new List<MctsNode>();

    public Ruch? Ruch;

    public int Wizyty;
    public double Wygrane;

    public List<Ruch> NieprzetestowaneRuchy;

    public MctsNode(Gra gra, MctsNode? rodzic, Ruch? ruch = null)
    {
        Gra = gra;
        Rodzic = rodzic;
        Ruch = ruch;
        NieprzetestowaneRuchy = gra.DostepneRuchy().ToList();
    }
}