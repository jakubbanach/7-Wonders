using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class KartaBazowa
{
    public string Nazwa { get; protected set; }
    public Dictionary<Surowiec, int> Koszt { get; protected set; }
    public List<Efekt> Efekty { get; protected set; }

    public KartaBazowa(string nazwa, Dictionary<Surowiec, int> koszt, List<Efekt> efekty)
    {
        Nazwa = nazwa;
        Koszt = koszt;
        Efekty = efekty;
    }

    private Dictionary<Surowiec, int> ObliczBrakujaceSurowce(Dictionary<Surowiec, int> posiadaneSurowce, Dictionary<Surowiec, int> kosztZKarty)
    {
        var brakujaceSurowce = new Dictionary<Surowiec, int>();

        foreach (var surowiec in kosztZKarty)
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

    private void ZastosowanieEfektowWyboruSurowca(
        List<Efekt> efektyWyboruSurowca,
        Dictionary<Surowiec, int> brakujaceSurowce,
        Dictionary<Surowiec, int> przeciwnikSurowce,
        List<Efekt> efektyGracza)
    {
        foreach (var efekt in efektyWyboruSurowca)
        {
            var najlepszy = brakujaceSurowce
                .Where(kv =>
                    kv.Value > 0 &&
                    efekt.Surowce.ContainsKey(kv.Key))
                .OrderByDescending(kv =>
                    ObliczKosztJednegoSurowca(
                        kv.Key,
                        przeciwnikSurowce,
                        efektyGracza))
                .FirstOrDefault();

            if (najlepszy.Key != default)
            {
                brakujaceSurowce[najlepszy.Key]--;
            }
        }
    }

    private void ZastosujMniejMaterialow(
        Dictionary<Surowiec, int> brakujaceSurowce,
        Dictionary<Surowiec, int> przeciwnikSurowce,
        List<Efekt> efektyGracza,
        Karta? karta = null,
        KartaCudu? kartaCudu = null)
    {
        bool aktywny = false;
        if (karta != null)
        {
            aktywny = efektyGracza.Any(e =>
            e.TypEfektu == TypEfektu.MniejMaterialowNaNiebieskieKarty
            && karta.KolorKarty == KolorKarty.Niebieski);
        }
        else if (kartaCudu != null)
        {
            aktywny = efektyGracza.Any(e =>
                e.TypEfektu == TypEfektu.MniejMaterialowNaCuda
            );
        }

        if (!aktywny)
            return;

        var brakujaceJednostki = brakujaceSurowce
            .Where(kv => kv.Value > 0)
            .SelectMany(kv => Enumerable.Repeat(kv.Key, kv.Value))
            .ToList();

        if (!brakujaceJednostki.Any())
            return;

        var posortowane = brakujaceJednostki
            .OrderByDescending(s =>
                ObliczKosztJednegoSurowca(s, przeciwnikSurowce, efektyGracza))
            .ToList();

        var doUsuniecia = posortowane.Take(2);

        foreach (var surowiec in doUsuniecia)
        {
            brakujaceSurowce[surowiec]--;
        }
    }
    private int ObliczKosztJednegoSurowca(
        Surowiec surowiec,
        Dictionary<Surowiec, int> przeciwnikSurowce,
        List<Efekt> efektyGracza)
    {
        // efekt: zmiana ceny konkretnego surowca
        if (efektyGracza.Any(e =>
                e.TypEfektu == TypEfektu.ZmianaCenySurowca
                && e.Surowiec == surowiec))
        {
            return 1;
        }

        var iloscPrzeciwnika = przeciwnikSurowce.GetValueOrDefault(surowiec);
        return 2 + iloscPrzeciwnika;
    }


    public int ObliczKoszt(Gracz gracz, Gracz przeciwnik, Karta? karta = null, KartaCudu? kartaCudu = null)
    {
        if (kartaCudu != null && karta != null)
        {
            throw new ArgumentException("Nie można podać jednocześnie karty i karty cudu.");
        }
        
        Dictionary<Surowiec, int> kosztZKarty = Koszt;
        if (kartaCudu != null)
            kosztZKarty = kartaCudu.Koszt;

        if (kosztZKarty == null || kosztZKarty.Count == 0)
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

        var brakujaceSurowce = ObliczBrakujaceSurowce(posiadaneSurowce, kosztZKarty);
        // zastosowanie efektów zmiany ceny surowca
        ZastosowanieEfektowWyboruSurowca(efektyWyboruSurowca, brakujaceSurowce, przeciwnikSurowce, efektyGracza);
        // zastosowanie efektów mniej materiałów na niebieskie karty i cuda
        if (karta != null)
            ZastosujMniejMaterialow(brakujaceSurowce, przeciwnikSurowce, efektyGracza, karta: karta);
        else if (kartaCudu != null)
            ZastosujMniejMaterialow(brakujaceSurowce, przeciwnikSurowce, efektyGracza, kartaCudu: kartaCudu);


        foreach (var surowiec in brakujaceSurowce)
        {
            if (surowiec.Key == Surowiec.Monety)
            {
                kosztMonet += surowiec.Value;
            }
            else if (surowiec.Value > 0)
            {
                kosztMonet += surowiec.Value * ObliczKosztJednegoSurowca(surowiec.Key, przeciwnikSurowce, efektyGracza);
            }
        }
        return kosztMonet;
    }
}
