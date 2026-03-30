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
    public List<SymbolNaukowy> SymboleNaukowe { get; protected set; } = new List<SymbolNaukowy>();
    public List<String> BialeSymbole { get; protected set; } = new List<String>();
    public List<Efekt> Efekty { get; protected set; } = new List<Efekt>();
    public int PunktyZwyciestwa { get; protected set; }
    public List<ZetonPostepu> ZetonyPostepu { get; protected set; } = new List<ZetonPostepu>();

    public struct WynikBudowy
    {
        public bool MoznaZagrac;
        public int Koszt;
    }

    public Gracz(string nazwa)
    {
        Nazwa = nazwa;
        //Monety = 7;
        KartyCudow = new List<KartaCudu>(4);
        ZbudowaneKarty = new List<Karta>();
        Surowce = new Dictionary<Surowiec, int>();
        InicjalizujSurowce();
        PunktyZwyciestwa = 0;
    }

    private Gracz(Gracz gracz)
    {
        Nazwa = gracz.Nazwa;
        KartyCudow = gracz.KartyCudow.Select(k => k.Clone()).ToList();
        ZbudowaneKarty = gracz.ZbudowaneKarty.Select(k => k.Clone()).ToList();
        Surowce = new Dictionary<Surowiec, int>(gracz.Surowce);
        SymboleNaukowe = new List<SymbolNaukowy>(gracz.SymboleNaukowe);
        BialeSymbole = new List<string>(gracz.BialeSymbole);
        Efekty = new List<Efekt>(gracz.Efekty);
        PunktyZwyciestwa = gracz.PunktyZwyciestwa;
        ZetonyPostepu = new List<ZetonPostepu>(gracz.ZetonyPostepu);
    }

    public Gracz Clone()
    {
        return new Gracz(this);
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
    public int Monety()
    {
        return Surowce.TryGetValue(Surowiec.Monety, out var monety) ? monety : 0;
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

    public string WypiszKartyCuduSzczegolowo()
    {
        return string.Join("\n", KartyCudow.Select(k => $"{k.Nazwa} (Koszt: {k.WypiszKoszt()}), (Efekty:{k.WypiszEfekty()})"));
    }

    public int WypiszLiczbeSurowca(Surowiec surowiec)
    {
        return Surowce.ContainsKey(surowiec) ? Surowce[surowiec] : 0;
    }

    public void UsunKarte(Karta karta)
    {
        ZbudowaneKarty.Remove(karta);
    }

    public List<Karta> PobierzZbudowaneKarty()
    {
        // Implementacja pobierania dostêpnych kart
        return ZbudowaneKarty;
    }

    public List<KartaCudu> PobierzKartyCudu()
    {
        return KartyCudow;
    }

    public void DodajEfekt(Efekt efekt)
    {
        Efekty.Add(efekt);
    }

    public void UsunEfekt(Efekt efekt)
    {
        Efekty.Remove(efekt);
    }

    public void DodajSymbolNaukowy(SymbolNaukowy symbol)
    {
        SymboleNaukowe.Add(symbol);
    }
    public void DodajBialySymbol(string symbol)
    {
        BialeSymbole.Add(symbol);
    }
    public void DodajZetonPostepu(ZetonPostepu zeton)
    {
        ZetonyPostepu.Add(zeton);
    }

    public void DodajPunktyZwyciestwa(int punkty)
    {
        PunktyZwyciestwa += punkty;
    }

    public List<Efekt> PobierzEfekty()
    {
        return Efekty;
    }

    public List<KartaCudu> PobierzZbudowaneKartyCudow()
    {
        // Implementacja pobierania zagranych kart cudów
        return KartyCudow.Where(k => k.CzyZagrana).ToList();
    }

    public WynikBudowy CzyMoznaZbudowacKarte(Karta karta, Gracz przeciwnik, PlanszaKonfliktu planszaKonfliktu = null)
    {
        if (karta.CzyZagrana || karta.CzyOdrzucona)
            return new WynikBudowy { MoznaZagrac = false, Koszt = 0 };        

        //obs³uga darmowej budowy -> bialy symbol
        if (!string.IsNullOrEmpty(karta.DarmowaBudowa))
        {
            foreach (var bialySymbol in BialeSymbole)
            {
                if (bialySymbol == karta.DarmowaBudowa)
                {
                    // TODO: Uwzglêdniæ efekt kiedy gracz dostaje monety za budowê karty z bia³ym symbolem

                    // CZY DODAWAC TUTAJ MONETY????
                    var efektMonetyZaBudoweZBialymSymbolem = Efekty.FirstOrDefault(e => e.TypEfektu == TypEfektu.MonetyZaBudoweZBialymSymbolem);
                    if (efektMonetyZaBudoweZBialymSymbolem != null)
                    {
                        this.DodajMonety(efektMonetyZaBudoweZBialymSymbolem.Wartosc);
                    }
                    karta.OznaczJakoZagrana();
                    ZbudowaneKarty.Add(karta);
                    foreach (var efekt in karta.Efekty)
                    {
                        efekt.ZastosujEfekt(this, przeciwnik, planszaKonfliktu, karta);
                    }
                    return new WynikBudowy { MoznaZagrac = true, Koszt = 0 };
                }
            }
        }

        int koszt = karta.ObliczKoszt(this, przeciwnik, karta: karta);
        int monety = Surowce.TryGetValue(Surowiec.Monety, out var m) ? m : 0;
        return new WynikBudowy { MoznaZagrac = koszt <= monety, Koszt = koszt };
    }

    public void ZbudujKarte(Karta karta, Gracz przeciwnik, PlanszaKonfliktu planszaKonfliktu=null)
    {
        if (karta == null)
            throw new ArgumentNullException(nameof(karta));
        
        var wynikBudowy = CzyMoznaZbudowacKarte(karta, przeciwnik, planszaKonfliktu);
        if (wynikBudowy.MoznaZagrac)
        {
            DodajMonety(-wynikBudowy.Koszt);
            if (przeciwnik.Efekty.Any(e => e.TypEfektu == TypEfektu.MonetyPrzeciwnikaZaMaterialy))
            {
                var kosztSurowcaMonety = karta.Koszt.TryGetValue(Surowiec.Monety, out var kosztMonet) ? kosztMonet : 0;
                przeciwnik.DodajMonety(wynikBudowy.Koszt - kosztSurowcaMonety);
            }
            karta.OznaczJakoZagrana();

            ZbudowaneKarty.Add(karta);

            foreach (var efekt in karta.Efekty)
            {
                efekt.ZastosujEfekt(this, przeciwnik, planszaKonfliktu, karta);
                DodajEfekt(efekt);
            }
        }

        else
        {
            throw new InvalidOperationException("Nie mo¿na zbudowaæ tej karty.");
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

    public WynikBudowy CzyMoznaZbudowacCud(Karta karta, Gracz przeciwnik, KartaCudu kartaCudu, PlanszaKonfliktu planszaKonfliktu=null)
    {
        if (kartaCudu.CzyZagrana)
            return new WynikBudowy { MoznaZagrac = false, Koszt = 0 };

        int koszt = karta.ObliczKoszt(this, przeciwnik, kartaCudu: kartaCudu);
        int monety = Surowce.TryGetValue(Surowiec.Monety, out var m) ? m : 0;

        return new WynikBudowy { MoznaZagrac = koszt <= monety, Koszt = koszt };
    }
    public void ZbudujCud(Karta karta, Gracz przeciwnik, KartaCudu kartaCudu, PlanszaKonfliktu planszaKonfliktu=null)
    {
        if (karta == null)
            throw new ArgumentNullException(nameof(karta));
        if (kartaCudu == null)
            throw new ArgumentNullException(nameof(kartaCudu));

        var wynikBudowy = CzyMoznaZbudowacCud(karta, przeciwnik, kartaCudu, planszaKonfliktu);
        if (wynikBudowy.MoznaZagrac)
        {
            DodajMonety(-wynikBudowy.Koszt);

            //chyba potrzebne?
            if (przeciwnik.Efekty.Any(e => e.TypEfektu == TypEfektu.MonetyPrzeciwnikaZaMaterialy))
            {
                var kosztSurowcaMonety = karta.Koszt.TryGetValue(Surowiec.Monety, out var kosztMonet) ? kosztMonet : 0;
                przeciwnik.DodajMonety(wynikBudowy.Koszt - kosztSurowcaMonety);
            }

            kartaCudu.OznaczJakoZagrana();
            karta.OznaczJakoZagrana(); // wsuwanie karty pod karte cudu

            foreach (var efekt in kartaCudu.Efekty)
            {
                efekt.ZastosujEfekt(this, przeciwnik, planszaKonfliktu);
                DodajEfekt(efekt);
            }
        }

        else
        {
            throw new InvalidOperationException("Nie mo¿na zbudowaæ tego cudu.");
        }
    }


    public string WypiszStan()
    {
        var kartyCuduOpis = string.Join(",\n\t",
            KartyCudow.Select(cud => cud.WypiszOpis()));

        var zbudowaneKartyCuduOpis = string.Join(", ",
            PobierzZbudowaneKartyCudow().Select(cud => cud.WypiszOpis()));


        var surowceOpis = string.Join(", ", Surowce.Select(s => $"{s.Key}: {s.Value}"));
        
        var kartyOpis = string.Join(", ",ZbudowaneKarty
                .GroupBy(k => k.KolorKarty)
                .OrderBy(g => g.Key)
                .Select(g => $"{g.Count()}x {g.Key}"));


        var efektyOpis = string.Join(", ", ZbudowaneKarty
            .Select(k => k.WypiszEfekty(poZagraniu: true))
            .Concat(PobierzZbudowaneKartyCudow().
                Select(k => k.WypiszEfekty(poZagraniu: true)))
            .Where(e => e != "Brak efektów"));
        return $"Gracz: {Nazwa}\n" +
            $"Karty Cudów: \n\t{kartyCuduOpis}\n" +
            $"Surowce: {surowceOpis}\n" +
            $"Zbudowane Karty: {kartyOpis}\n" +
            $"Zbudowane Cuda: {zbudowaneKartyCuduOpis}\n" +
            $"Efekty: {efektyOpis}";
    }
}
