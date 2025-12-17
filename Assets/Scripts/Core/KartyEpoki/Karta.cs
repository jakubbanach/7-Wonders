using System.Collections.Generic;

public class Karta
{
    public string Nazwa { get; protected set; }
    public Epoka Epoka { get; protected set; }
    public Dictionary<Surowiec, int> Koszt { get; protected set; }
    public List<Efekt> Efekty { get; protected set; }
    public KolorKarty KolorKarty { get; protected set; }
    public string DarmowaBudowa { get; set; } = "";
    public bool CzyWidoczna { get; protected set; } = false;
    public bool CzyZagrana { get; protected set; } = false;

    public Karta(string nazwa, Epoka epoka, Dictionary<Surowiec, int> koszt, List<Efekt> efekty,
        KolorKarty kolorKarty, string darmowaBudowa="", bool czyWidoczna=false, bool czyZagrana = false)
    {
        Nazwa = nazwa;
        Epoka = epoka;
        Koszt = koszt;
        Efekty = efekty;
        KolorKarty = kolorKarty;
        DarmowaBudowa = darmowaBudowa;
        CzyWidoczna = czyWidoczna;
        CzyZagrana = czyZagrana;
    }

    public void UstawWidocznosc(bool widoczna)
    {
        CzyWidoczna = widoczna;
    }

    public void UstawZagranie(bool zagrana)
    {
        CzyZagrana = zagrana;
    }

    public void DodajEfekt(Efekt efekt)
    {
        Efekty.Add(efekt);
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

}
