using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PoleKarty
{
    public Karta? Karta { get; set; }
    public bool CzyZakryta { get; private set; }

    // karty, ktore MUSZA zniknac, aby ta byla dostepna
    public List<PoleKarty> BlokujacePola { get; set; }

    public PoleKarty(Karta karta, bool czyZakryta)
    {
        Karta = karta;
        CzyZakryta = czyZakryta;
        BlokujacePola = new List<PoleKarty>();
    }

    private PoleKarty(PoleKarty pole)
    {
        Karta = pole.Karta?.Clone();
        CzyZakryta = pole.CzyZakryta;
        BlokujacePola = new List<PoleKarty>();
    }

    public PoleKarty Clone(Dictionary<PoleKarty, PoleKarty>? contextMap = null)
    {
        if (contextMap != null && contextMap.TryGetValue(this, out var existing))
            return existing;

        var cloned = new PoleKarty(this);
        
        if (contextMap != null)
            contextMap[this] = cloned;

        if (contextMap != null)
        {
            cloned.BlokujacePola = BlokujacePola.Select(bp => bp.Clone(contextMap)).ToList();
        }
        else
        {
            cloned.BlokujacePola = BlokujacePola.Select(p => p.Clone()).ToList();
        }

        return cloned;
    }

    public bool CzyDostepna =>
        !CzyZakryta &&
        BlokujacePola.All(p => p.Karta == null);

    public void Odkryj()
    {
        CzyZakryta = false;
    }

    public void UsunKarte()
    {
        Karta = null;
    }

    //public void DodajBlokujacePole(PoleKarty pole)
    //{
    //    BlokujacePola.Add(pole);
    //}
}
