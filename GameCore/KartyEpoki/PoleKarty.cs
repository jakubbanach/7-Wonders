using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PoleKarty
{
    public Karta? Karta { get; set; }
    public bool CzyZakryta { get; private set; }

    // karty, które MUSZĄ zniknąć, aby ta była dostępna
    public List<PoleKarty> BlokujacePola { get; set; }

    public PoleKarty(Karta karta, bool czyZakryta)
    {
        Karta = karta;
        CzyZakryta = czyZakryta;
        BlokujacePola = new List<PoleKarty>();
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
