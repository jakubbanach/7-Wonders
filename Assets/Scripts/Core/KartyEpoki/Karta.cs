using System;
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
    public bool CzyOdrzucona { get; protected set; } = false;

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

    public void OznaczJakoOdrzucona()
    {
        CzyOdrzucona = true;
    }

    public void OznaczJakoZagrana()
    {
        CzyZagrana = true;
    }

    public void DodajEfekt(Efekt efekt)
    {
        Efekty.Add(efekt);
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

    public static readonly HashSet<TypEfektu> WybraneEfekty = new()
    {
        TypEfektu.WyborSurowca,
        TypEfektu.PunktyZwyciestwa,
        TypEfektu.PunktyMilitarne,
        TypEfektu.ZmianaCenySurowca,
        TypEfektu.SymbolNaukowy,
        TypEfektu.PunktyZaKarty,
        TypEfektu.DodatkoweMilitariaZaCzerwoneKarty,
        TypEfektu.KoniecGry3PunktyZaZetonPostepu,
        TypEfektu.MniejMaterialowNaNiebieskieKarty,
        TypEfektu.MniejMaterialowNaCuda,
        TypEfektu.MonetyZaBudoweZBialymSymbolem,
        TypEfektu.MonetyPrzeciwnikaZaMaterialy,
    };
}
