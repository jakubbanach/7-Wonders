using System;
using System.Collections.Generic;
using System.Linq;

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

    private Dictionary<Surowiec, int> ObliczBrakujaceSurowce(Dictionary<Surowiec, int> posiadaneSurowce)
    {
        var brakujaceSurowce = new Dictionary<Surowiec, int>();

        foreach (var surowiec in Koszt)
        {
            if (surowiec.Key == Surowiec.Monety)
            {
                brakujaceSurowce[surowiec.Key] = surowiec.Value;
                continue;
            }

            var posiadane = posiadaneSurowce.GetValueOrDefault(surowiec.Key);
            brakujaceSurowce[surowiec.Key] = Math.Max(0, surowiec.Value - posiadane);
        }
        return brakujaceSurowce;
    }

    private void ZastosowanieEfektowWyboruSurowca(List<Efekt> efektyWyboruSurowca, Dictionary<Surowiec, int> brakujaceSurowce, Dictionary<Surowiec, int> przeciwnikSurowce, List<Efekt> efektyZmianyCeny)
    {
        foreach (var efekt in efektyWyboruSurowca)
        {
            var najlepszyWybor = efekt.Surowce.Keys
                .Where(s => brakujaceSurowce.GetValueOrDefault(s) > 0)
                .OrderByDescending(s =>
                {
                    if (efektyZmianyCeny.Any(e => e.Surowiec == s))
                        return 1; // koszt zakupu 1 sztuki, bo efekt zmienia cenê na 1 monetê
                    
                    var iloscPrzeciwnika = przeciwnikSurowce.GetValueOrDefault(s);
                    return 2 + iloscPrzeciwnika; // koszt zakupu 1 sztuki
                })
                .FirstOrDefault();

            if (najlepszyWybor != default)
            {
                brakujaceSurowce[najlepszyWybor]--;
            }
        }
    }

    public int ObliczKoszt(Gracz gracz, Gracz przeciwnik)
    {
        if (Koszt == null || Koszt.Count == 0)
        {
            return 0;
        }
        var posiadaneSurowce = gracz.Surowce;
        var posiadaneKarty = gracz.PobierzZbudowaneKarty();
        var posiadaneKartyCudu = gracz.PobierzZbudowaneKartyCudow();

        var przeciwnikSurowce = przeciwnik.Surowce;
        var przeciwnikKarty = przeciwnik.PobierzZbudowaneKarty();

        var efektyGracza = gracz.PobierzEfekty();
        var efektyWyboruSurowca = efektyGracza
            .Where(e => e.TypEfektu == TypEfektu.WyborSurowca)
            .ToList();
        var efektyZmianyCeny = efektyGracza
            .Where(e => e.TypEfektu == TypEfektu.ZmianaCenySurowca)
            .ToList();

        int kosztMonet = 0;

        //if (efektyGracza.Any(e => e.TypEfektu == TypEfektu.MniejMaterialowNaNiebieskieKarty && KolorKarty == KolorKarty.Niebieski))
        //{
        //    return 0;
        //}
        //if (efektyGracza.Any(e => e.TypEfektu == TypEfektu.MniejMaterialowNaCuda && DarmowaBudowa == "Cud"))
        //{
        //    return 0;
        //}
        //if (efektyGracza.Any(e => e.TypEfektu == TypEfektu.MonetyZaBudoweZBialymSymbolem && posiadaneKarty.Any(k => k.DarmowaBudowa.Contains("Bia³y symbol"))))
        //{
        //    return 0;
        //}

        var brakujaceSurowce = ObliczBrakujaceSurowce(posiadaneSurowce);
        ZastosowanieEfektowWyboruSurowca(efektyWyboruSurowca, brakujaceSurowce, przeciwnikSurowce, efektyZmianyCeny);

        foreach (var surowiec in brakujaceSurowce)
        {
            if (surowiec.Key == Surowiec.Monety)
            {
                kosztMonet += surowiec.Value;
            }
            else if (surowiec.Value > 0)
            {
                bool zmianaCeny = efektyZmianyCeny.Any(e => e.Surowiec == surowiec.Key);

                var iloscPrzeciwnika = przeciwnikSurowce.GetValueOrDefault(surowiec.Key);
                int kosztJednegoSurowca = zmianaCeny ? 1 : 2 + iloscPrzeciwnika;

                kosztMonet += surowiec.Value * kosztJednegoSurowca;

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
