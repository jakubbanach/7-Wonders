using System.Collections.Generic;
using System;
public class KartaCudu
{
    public string Nazwa { get; protected set; }
    public Dictionary<Surowiec, int> Koszt { get; protected set; }
    public List<Efekt> Efekty { get; protected set; }
    public bool CzyZagrana { get; protected set; } = false;

    public KartaCudu(string nazwa, Dictionary<Surowiec, int> koszt, List<Efekt> efekty, bool czyZagrana=false)
    {
        Nazwa = nazwa;
        Koszt = koszt;
        Efekty = efekty;
        CzyZagrana = czyZagrana;
    }

    public void OznaczJakoZagrana()
    {
        CzyZagrana = true;
    }
    public int ObliczKoszt(Dictionary<Surowiec, int> posiadaneSurowce)
    {
        int kosztMonet = 0; 
        if (Koszt == null || Koszt.Count == 0)
        {
            return 0;
        }
        foreach (var surowiec in Koszt)
        {
            if (surowiec.Key == Surowiec.Monety)
            {
                kosztMonet += surowiec.Value;
            }
            else
            {
                var posiadanaIlosc = posiadaneSurowce.ContainsKey(surowiec.Key) ? posiadaneSurowce[surowiec.Key] : 0;
                // Dodaæ jeszcze warunek dokupienia surowców za monety bazuj¹c na drugim graczu!!!
                //var IloscSurowcaPrzeciwnika = przeciwnik.Surowce(surowiec.Key); 
                kosztMonet += Math.Max(0, surowiec.Value - posiadanaIlosc) * 2; // + IloscSurowcaPrzeciwnika;
            }
        }
        return kosztMonet;
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
