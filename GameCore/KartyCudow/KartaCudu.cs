using System.Collections.Generic;
using System;
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

    public void OznaczJakoNiezagrana() // przydatne do resetowania stanu kart po zakoñczeniu gry
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
            return "Brak efektów";
        }

        if (poZagraniu)
        {
            var efektyList = Efekty
                .Where(e => e != null && Karta.WybraneEfekty.Contains(e.TypEfektu))
                .Select(e => e.Wypisz())
                .ToList();
            return efektyList.Count == 0
                ? "Brak efektów"
                : string.Join(", ", efektyList);
        }

        return string.Join(", ", Efekty.Select(e => e.Wypisz()));
    }
    public string WypiszOpis()
    {
        return $"{Nazwa,-30} | Koszt: {WypiszKoszt(),-6} | {WypiszEfekty()}";
    }
}
