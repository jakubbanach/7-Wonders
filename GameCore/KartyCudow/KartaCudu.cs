using System.Collections.Generic;
using System;
using System.Linq;
public class KartaCudu : KartaBazowa
{
    //public string Nazwa { get; protected set; }
    //public Dictionary<Surowiec, int> Koszt { get; protected set; }
    //public List<Efekt> Efekty { get; protected set; }
    public bool CzyZagrana { get; protected set; } = false;

  
    public KartaCudu(string nazwa, Dictionary<Surowiec, int> koszt, List<Efekt> efekty, bool czyZagrana = false)
        : base(nazwa, koszt, efekty)
    { 
        CzyZagrana = czyZagrana;
    }

    private KartaCudu(KartaCudu kartaCudu)
        : base(kartaCudu.Nazwa, kartaCudu.Koszt, kartaCudu.Efekty)
    {
        CzyZagrana = kartaCudu.CzyZagrana;
    }

    public KartaCudu Clone()
    {
        return new KartaCudu(this);
    }

    public void OznaczJakoNiezagrana() // przydatne do resetowania stanu kart po zakonczeniu gry
    {
        CzyZagrana = false;
    }

    public void OznaczJakoZagrana()
    {
        CzyZagrana = true;
    }
    public string WypiszKoszt()
    {
        if (Koszt == null || Koszt.Count == 0)
        {
            return "Brak kosztu";
        }
        List<string> kosztList = new List<string>();
        foreach (var surowiec in Koszt)
        {
            kosztList.Add($"{surowiec.Value}x{surowiec.Key}");
        }
        return string.Join(" + ", kosztList);
    }
    public string WypiszEfekty(bool poZagraniu = false)
    {
        if (Efekty == null || Efekty.Count == 0)
        {
            return "Brak efektow";
        }

        if (poZagraniu)
        {
            var efektyList = Efekty
                .Where(e => e != null && WybraneEfekty.Contains(e.TypEfektu))
                .Select(e => e.Wypisz())
                .ToList();
            return efektyList.Count == 0
                ? "Brak efektow"
                : string.Join(", ", efektyList);
        }

        return string.Join(", ", Efekty.Select(e => e.Wypisz()));
    }
    public string WypiszOpis()
    {
        return $"{Nazwa,-30} | Koszt: {WypiszKoszt(),-6} | {WypiszEfekty()}";
    }
}
