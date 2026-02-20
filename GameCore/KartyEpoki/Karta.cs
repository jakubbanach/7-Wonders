using System;
using System.Collections.Generic;
using System.Linq;

public class Karta : KartaBazowa
{
    //public string Nazwa { get; protected set; }
    //public Dictionary<Surowiec, int> Koszt { get; protected set; }
    //public List<Efekt> Efekty { get; protected set; }
    public Epoka Epoka { get; protected set; }
    public KolorKarty KolorKarty { get; protected set; }
    public string DarmowaBudowa { get; set; } = "";
    public bool CzyWidoczna { get; protected set; } = false;
    public bool CzyOdrzucona { get; protected set; } = false;
    public bool CzyZagrana { get; protected set; } = false;

    public Karta(string nazwa, Epoka epoka, Dictionary<Surowiec, int> koszt, List<Efekt> efekty,
        KolorKarty kolorKarty, string darmowaBudowa = "", bool czyWidoczna = false, bool czyZagrana = false)
        : base(nazwa, koszt, efekty)
    {
        Epoka = epoka;
        KolorKarty = kolorKarty;
        DarmowaBudowa = darmowaBudowa;
        CzyWidoczna = czyWidoczna;
        CzyZagrana = czyZagrana;
    }

    public void UstawWidocznosc(bool widoczna)
    {
        CzyWidoczna = widoczna;
    }

    public void OznaczJakoOdrzucona()
    {
        CzyOdrzucona = true;
    }

    public void OznaczJakoNiezagrana() // przydatne do resetowania stanu kart po zakoñczeniu gry
    {
        CzyZagrana = false;
    }

    public void OznaczJakoZagrana()
    {
        CzyZagrana = true;
    }

    public void DodajEfekt(Efekt efekt)
    {
        Efekty.Add(efekt);
    }

    public string WypiszKoszt()
    {
        if (Koszt == null || Koszt.Count == 0)
        {
            return "0";
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
                .Where(e => e != null && WybraneEfekty.Contains(e.TypEfektu))
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
        return $"{Nazwa,-20} | {KolorKarty,-9} | Koszt: {WypiszKoszt(),-6} | {WypiszEfekty()}";
    }
}
