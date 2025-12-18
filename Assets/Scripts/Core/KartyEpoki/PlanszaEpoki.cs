using System.Collections.Generic;
using System.Linq;
using System;

public class PlanszaEpoki
{
    public Epoka Epoka { get; set; }
    public List<PoleKarty> Pola { get; set; }

    public PlanszaEpoki(Epoka epoka)
    {
        Epoka = epoka;
        Pola = new List<PoleKarty>();
    }
    public IEnumerable<PoleKarty> DostepneKarty =>
        Pola.Where(p => p.CzyDostepna && p.Karta != null);

    public IEnumerator<PoleKarty> GetEnumerator()
    {
        return Pola.GetEnumerator();
    }

    public IEnumerable<PoleKarty> WidoczneKarty =>
        Pola.Where(p => !p.CzyZakryta);

    public void UsunPole(PoleKarty pole)
    {
        pole.UsunKarte();
        foreach (var p in Pola)
        {
            if (p.BlokujacePola.Contains(pole) && p.CzyZakryta)
            {
                if (p.BlokujacePola.All(bp => bp.Karta == null))
                {
                    p.Odkryj();
                }
            }
        }
    }
}