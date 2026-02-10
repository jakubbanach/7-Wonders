using System;
using System.Collections.Generic;
using System.Linq;

public class Gracz
{
    public string Nazwa { get; protected set; }
    //public int Monety { get; protected set; }
    public List<KartaCudu> KartyCudow { get; protected set; }
    public List<Karta> ZbudowaneKarty{ get; protected set; }
    public Dictionary<Surowiec, int> Surowce { get; protected set; }
    public List<SymbolNaukowy> symboleNaukowe { get; protected set; } = new List<SymbolNaukowy>();
    public List<String> BialeSymbole { get; protected set; } = new List<String>();

    public Gracz(string nazwa)
    {
        Nazwa = nazwa;
        //Monety = 7;
        KartyCudow = new List<KartaCudu>(4);
        ZbudowaneKarty = new List<Karta>();
        Surowce = new Dictionary<Surowiec, int>();
        InicjalizujSurowce();
    }

    private void InicjalizujSurowce()
    {
        DodajSurowiec(Surowiec.Drewno, 0);
        DodajSurowiec(Surowiec.Glina, 0);
        DodajSurowiec(Surowiec.Kamieñ, 0);
        DodajSurowiec(Surowiec.Szk³o, 0);
        DodajSurowiec(Surowiec.Papirus, 0);
        DodajMonety(7);
        //DodajSurowiec(Surowiec.Monety, 7);
    }
    public void DodajMonety(int ilosc)
    {
        //Monety += ilosc;
        if (Surowce.ContainsKey(Surowiec.Monety))
        {
            Surowce[Surowiec.Monety] += ilosc;
        }
        else
        {
            Surowce[Surowiec.Monety] = ilosc;
        }
    }

    public void DodajSurowiec(Surowiec surowiec, int ilosc)
    {
        if (Surowce.ContainsKey(surowiec))
        {
            Surowce[surowiec] += ilosc;
        }
        else
        {
            Surowce[surowiec] = ilosc;
        }
    }

    public void DodajKarteCudu(KartaCudu kartaCudu)
    {
        KartyCudow.Add(kartaCudu);
    }

    public string WypiszKartyCudu()
    {
        return string.Join(", ", KartyCudow.Select(k => k.Nazwa));
    }

    public int WypiszLiczbeSurowca(Surowiec surowiec)
    {
        return Surowce.ContainsKey(surowiec) ? Surowce[surowiec] : 0;
    }
    //public void ZbudujKarteCudu(KartaCudu kartaCudu)
    //{
    //    kartaCudu.Zagraj(Surowce);
    //}

    public void UsunKarte(Karta karta)
    {
        ZbudowaneKarty.Remove(karta);
    }

    public List<Karta> PobierzZbudowaneKarty()
    {
        // Implementacja pobierania dostêpnych kart
        return ZbudowaneKarty;
    }

    public void ZbudujKarte(Karta karta, Gracz przeciwnik)
    {
        if (karta.CzyZagrana)
            throw new InvalidOperationException("Ta karta zosta³a ju¿ zagrana.");
        //TODO: Dodaæ obs³ugê darmowej budowy
        //TODO: Uwzglêdniæ karty przeciwnika, które mog¹ podwy¿szyæ koszt
        //TODO: Dodaæ obs³ugê efektów kart, które mog¹ obni¿yæ koszt budowy
        int koszt = karta.ObliczKoszt(Surowce); 
        int monety = Surowce.TryGetValue(Surowiec.Monety, out var m) ? m : 0;
        
        if (koszt > monety)
            throw new InvalidOperationException("Nie mo¿na zbudowaæ tej karty.");

        DodajMonety(-koszt);
        karta.OznaczJakoZagrana();

        ZbudowaneKarty.Add(karta);

        foreach (var efekt in karta.Efekty)
        {
            efekt.ZastosujEfekt(this);
        }
    }

    public void OdrzucKarte(Karta karta)
    {
        if (karta == null)
            throw new ArgumentNullException(nameof(karta));

        karta.OznaczJakoOdrzucona();

        int monety = 2;

        int liczbaZoltychKart = ZbudowaneKarty
            .Count(k => k.KolorKarty == KolorKarty.¯ó³ty);

        monety += liczbaZoltychKart;

        DodajMonety(monety);
    }

    public void ZbudujCud(Karta karta, Gracz przeciwnik, KartaCudu kartaCudu)
    {
        if (kartaCudu.CzyZagrana)
            throw new InvalidOperationException("Cud ju¿ zbudowany.");

        int koszt = kartaCudu.ObliczKoszt(Surowce);
        int monety = Surowce.TryGetValue(Surowiec.Monety, out var m) ? m : 0;

        if (koszt > monety)
            throw new InvalidOperationException("Nie mo¿na zbudowaæ cudu.");

        DodajMonety(-koszt);

        kartaCudu.OznaczJakoZagrana();
        karta.OznaczJakoZagrana(); // wsuwanie karty pod karte cudu

        foreach (var efekt in kartaCudu.Efekty)
        {
            efekt.ZastosujEfekt(this);
        }
    }


    public string WypiszStan()
    {
        var surowceOpis = string.Join(", ", Surowce.Select(s => $"{s.Key}: {s.Value}"));
        var kartyOpis = string.Join(", ", ZbudowaneKarty.Select(k => k.Nazwa));
        var kartyCuduOpis = string.Join(", ", KartyCudow.Select(k => k.Nazwa));
        return $"Gracz: {Nazwa}\nSurowce: {surowceOpis}\nZbudowane Karty: {kartyOpis}\nKarty Cudów: {kartyCuduOpis}";
    }
}
