using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

public class TestKoniecGry
{
    private readonly ITestOutputHelper _output;

    public TestKoniecGry(ITestOutputHelper output)
    {
        _output = output;
    }
    [Fact]
    public void Test_CzyZwyciestwoMilitarne_Zwyciestwo()
    {
        var pionKonfliktu = new PionKonfliktu(0);
        var zetonyPostepu = ZbiorZetonowPostepu.ZetonyPostepu;
        var wybraneZetony = zetonyPostepu.OrderBy(x => Guid.NewGuid()).Take(5).ToList();
        var strefy = ZbiorStref.Strefy.ToList();
        var gracze = new[] { new Gracz("GraczA"), new Gracz("GraczB") };

        var plansza = new PlanszaKonfliktu(pionKonfliktu, wybraneZetony, strefy, gracze);

        var stanGry = new StanGry();

        plansza.PrzesunPion(9, gracze[0]);
        stanGry.CzyZwyciestwoMilitarne(gracze, pionKonfliktu.PobierzPozycje());

        Assert.True(stanGry.CzyZakonczona);
        Assert.Equal(gracze[0], stanGry.Zwyciezca);
        Assert.Equal(TypZwyciestwa.Militarne, stanGry.TypZwyciestwa);
    }
    [Fact]
    public void Test_CzyZwyciestwoMilitarne_BrakZwyciestwa()
    {
        var pionKonfliktu = new PionKonfliktu(0);
        var zetonyPostepu = ZbiorZetonowPostepu.ZetonyPostepu;
        var wybraneZetony = zetonyPostepu.OrderBy(x => Guid.NewGuid()).Take(5).ToList();
        var strefy = ZbiorStref.Strefy.ToList();
        var gracze = new[] { new Gracz("GraczA"), new Gracz("GraczB") };

        var plansza = new PlanszaKonfliktu(pionKonfliktu, wybraneZetony, strefy, gracze);

        var stanGry = new StanGry();

        plansza.PrzesunPion(1, gracze[1]);
        plansza.PrzesunPion(9, gracze[0]);
        stanGry.CzyZwyciestwoMilitarne(gracze, pionKonfliktu.PobierzPozycje());

        Assert.False(stanGry.CzyZakonczona);
        Assert.Equal(TypZwyciestwa.Brak, stanGry.TypZwyciestwa);
    }
    [Fact]
    public void Test_Zwyciestwo_Naukowe()
    {
        var gracze = new[] { new Gracz("GraczA"), new Gracz("GraczB") };

        var stanGry = new StanGry();

        foreach (var symbol in Enum.GetValues<SymbolNaukowy>())
        {
            gracze[0].SymboleNaukowe.Add(symbol);
        }

        stanGry.CzyZwyciestwoNaukowe(gracze, null);

        Assert.True(stanGry.CzyZakonczona);
        Assert.Equal(gracze[0], stanGry.Zwyciezca);
        Assert.Equal(TypZwyciestwa.Naukowe, stanGry.TypZwyciestwa);
    }
    [Fact]
    public void Test_Zwyciestwo_Naukowe_BrakZwyciestwa()
    {
        var gracze = new[] { new Gracz("GraczA"), new Gracz("GraczB") };

        var stanGry = new StanGry();

        foreach (var symbol in Enum.GetValues<SymbolNaukowy>())
        {
            if (symbol == SymbolNaukowy.Kolo || symbol == SymbolNaukowy.Globus) // Pomijamy jeden symbol, aby nie osiągnąć zwycięstwa
                continue;
            gracze[0].SymboleNaukowe.Add(symbol);
        }

        stanGry.CzyZwyciestwoNaukowe(gracze, null);

        Assert.False(stanGry.CzyZakonczona);
        Assert.Equal(TypZwyciestwa.Brak, stanGry.TypZwyciestwa);
    }
    [Fact]
    public void Test_ZliczaniePunktow_Militaria()
    {
        var pionKonfliktu = new PionKonfliktu(0);
        var zetonyPostepu = ZbiorZetonowPostepu.ZetonyPostepu;
        var wybraneZetony = zetonyPostepu.OrderBy(x => Guid.NewGuid()).Take(5).ToList();
        var strefy = ZbiorStref.Strefy.ToList();
        var gracze = new[] { new Gracz("GraczA"), new Gracz("GraczB") };

        var planszaKonfliktu = new PlanszaKonfliktu(pionKonfliktu, wybraneZetony, strefy, gracze);

        var stanGry = new StanGry();
        var pozycjaPionu = 7; // Przykładowa pozycja, która daje przewagę militarną

        stanGry.PunktyPrzewagaMilitarna(gracze, planszaKonfliktu.PobierzStrefeDlaPozycji(pozycjaPionu), pozycjaPionu);

        Assert.Equal(10 ,stanGry.GetPunktyGracza1());
        Assert.Equal(0 ,stanGry.GetPunktyGracza2());
        Assert.Equal(TypZwyciestwa.Brak, stanGry.TypZwyciestwa);
    }
    [Fact]
    public void Test_ZliczaniePunktow_Monety()
    {
        var gracze = new[] { new Gracz("GraczA"), new Gracz("GraczB") };
        var stanGry = new StanGry();
        gracze[0].DodajMonety(8); // Łącznie 15 monet
        gracze[1].DodajMonety(-2); // Łącznie 5 monet

        stanGry.PunktyMonety(gracze);

        _output.WriteLine($"Monety Gracza 1: {gracze[0].WypiszLiczbeSurowca(Surowiec.Monety)}");
        _output.WriteLine($"Monety Gracza 2: {gracze[1].WypiszLiczbeSurowca(Surowiec.Monety)}");

        Assert.Equal(5, stanGry.GetPunktyGracza1());
        Assert.Equal(1, stanGry.GetPunktyGracza2());
        Assert.Equal(TypZwyciestwa.Brak, stanGry.TypZwyciestwa);
    }
    [Fact]
    public void Test_ZliczaniePunktow_ZetonyPostepu()
    {
        var gracze = new[] { new Gracz ("GraczA"), new Gracz("GraczB") };
        var stanGry = new StanGry();
        
        var zetonyPostepu = ZbiorZetonowPostepu.ZetonyPostepu;
        var zetonFilizofia = zetonyPostepu.FirstOrDefault(z => z.Nazwa == "Filozofia");
        var zetonRolnictwo = zetonyPostepu.FirstOrDefault(z => z.Nazwa == "Rolnictwo");
        
        foreach(Gracz gracz in gracze)
        {
            gracz.DodajMonety(-7); //zerujemy monety, aby nie wpływały na wynik testu
        }

        if (zetonFilizofia != null)
        {
            foreach (var efekt in zetonFilizofia.Efekty)
            {
                _output.WriteLine($"Efekt z Zetonu Filozofia: {efekt.TypEfektu}");
                efekt.ZastosujEfekt(gracze[0]);
            }
        }
        if (zetonRolnictwo != null)
        {
            foreach (var efekt in zetonRolnictwo.Efekty)
            {
                _output.WriteLine($"Efekt z Zetonu Rolnictwo: {efekt.TypEfektu}");
                efekt.ZastosujEfekt(gracze[1]); 
            }
        }
        //stanGry.ZliczaniePunktow_ZetonyPostepu(gracze);
        stanGry.PunktyZwyciestwa(gracze);
        _output.WriteLine($"Punkty Gracza 1: {stanGry.GetPunktyGracza1()}");
        _output.WriteLine($"Punkty Gracza 2: {stanGry.GetPunktyGracza2()}");

        Assert.Equal(7, stanGry.GetPunktyGracza1());
        Assert.Equal(4, stanGry.GetPunktyGracza2());

        stanGry.PunktyMonety(gracze);
        _output.WriteLine($"Monety Gracza 1: {gracze[0].WypiszLiczbeSurowca(Surowiec.Monety)}");
        _output.WriteLine($"Monety Gracza 2: {gracze[1].WypiszLiczbeSurowca(Surowiec.Monety)}");

        _output.WriteLine($"Punkty Gracza 1 po monetach: {stanGry.GetPunktyGracza1()}");
        _output.WriteLine($"Punkty Gracza 2 po monetach: {stanGry.GetPunktyGracza2()}");
        Assert.Equal(7, stanGry.GetPunktyGracza1());
        Assert.Equal(6, stanGry.GetPunktyGracza2());
    }
    [Fact]
    public void Test_ZliczaniePunktow_Efekty()
    {
        var stanGry = new StanGry();
        Gracz gracz = new Gracz("Gracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");
        var gracze = new[] { gracz, przeciwnik };

        var zolteKarty = ZbiorKart.TaliaEpokiII.Where(k => k.KolorKarty == KolorKarty.Zolty).Take(3).ToList();
        gracz.DodajMonety(20);
        przeciwnik.DodajMonety(20);
        foreach (var karta in zolteKarty)
        {
            karta.OznaczJakoNiezagrana();
            gracz.ZbudujKarte(karta, przeciwnik);
            _output.WriteLine($"Zbudowana karta: {karta.Nazwa} (Kolor: {karta.KolorKarty})");
        }
        var zieloneKarty = ZbiorKart.TaliaEpokiII.Where(k => k.KolorKarty == KolorKarty.Zielony).Take(4).ToList();
        foreach (var karta in zieloneKarty)
        {
            karta.OznaczJakoNiezagrana();
            przeciwnik.ZbudujKarte(karta, gracz);
            _output.WriteLine($"Zbudowana karta: {karta.Nazwa} (Kolor: {karta.KolorKarty})");
        }

        var efekt = new Efekt(
            TypEfektu.PunktyZaKarty,
            tekst: "Zolty",
            wartosc: 1
        );
        var efekt2 = new Efekt(
            TypEfektu.PunktyZaKarty,
            tekst: "Zielony",
            wartosc: 1
        );
        gracz.DodajEfekt(efekt);
        przeciwnik.DodajEfekt(efekt2);

        stanGry.PunktyEfekty(gracze);
        _output.WriteLine($"Punkty Gracza 1: {stanGry.GetPunktyGracza1()}");
        _output.WriteLine($"Punkty Gracza 2: {stanGry.GetPunktyGracza2()}");

        Assert.Equal(3, stanGry.GetPunktyGracza1());
        Assert.Equal(4, stanGry.GetPunktyGracza2());
    }
    [Fact]
    public void Test_ZliczaniePunktow_Efekty2()
    {
        var stanGry = new StanGry();
        Gracz gracz = new Gracz("Gracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");
        var gracze = new[] { gracz, przeciwnik };

        gracz.DodajMonety(20);
        przeciwnik.DodajMonety(20);

        var cuda = ZbiorKart.TaliaKartyCudow.Take(7).ToList();
        int iloscCudow = 0;
        foreach (var kartaCudu in cuda)
        {
            kartaCudu.OznaczJakoNiezagrana();
            if (iloscCudow % 2 == 0 )
                gracz.DodajKarteCudu(kartaCudu);
            else
                przeciwnik.DodajKarteCudu(kartaCudu);
            
            kartaCudu.OznaczJakoZagrana();
            _output.WriteLine($"Zbudowana karta: {kartaCudu.Nazwa} (Koszt: {kartaCudu.WypiszKoszt()})");
            iloscCudow++;
        }

        var efekt = new Efekt(
            TypEfektu.PunktyZaKarty,
            tekst: "Monety",
            wartosc: 1
        );
        var efekt2 = new Efekt(
            TypEfektu.PunktyZaKarty,
            tekst: "Cuda",
            wartosc: 2
        );
        gracz.DodajEfekt(efekt);
        przeciwnik.DodajEfekt(efekt2);

        stanGry.PunktyEfekty(gracze);
        _output.WriteLine($"Punkty Gracza 1: {stanGry.GetPunktyGracza1()}");
        _output.WriteLine($"Punkty Gracza 2: {stanGry.GetPunktyGracza2()}");

        Assert.Equal(9, stanGry.GetPunktyGracza1());
        Assert.Equal(8, stanGry.GetPunktyGracza2());

        // powrot do stanu cudu sprzed gry
        foreach (var kartaCudu in cuda)
        {
            kartaCudu.OznaczJakoNiezagrana();
        }
    }
    [Fact]
    public void Test_ZliczaniePunktow_Efekty3()
    {
        var stanGry = new StanGry();
        Gracz gracz = new Gracz("Gracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");
        var gracze = new[] { gracz, przeciwnik };

        var zetonyPostepu = ZbiorZetonowPostepu.ZetonyPostepu.Take(4).ToList();
        foreach (var zeton in zetonyPostepu)
        {
            gracz.DodajZetonPostepu(zeton);
             _output.WriteLine($"Dodany zeton postepu dla Gracza 1: {zeton.Nazwa}");
        }

        var efekt = new Efekt(TypEfektu.KoniecGry3PunktyZaZetonPostepu);

        gracz.DodajEfekt(efekt);

        stanGry.PunktyEfekty(gracze);
        _output.WriteLine($"Punkty Gracza 1: {stanGry.GetPunktyGracza1()}");
        _output.WriteLine($"Punkty Gracza 2: {stanGry.GetPunktyGracza2()}");

        Assert.Equal(12, stanGry.GetPunktyGracza1());
        Assert.Equal(0, stanGry.GetPunktyGracza2());
    }
    [Fact]
    public void Test_Zwyciestwo_Punktowe()
    {
        var pionKonfliktu = new PionKonfliktu(0);
        var zetonyPostepu = ZbiorZetonowPostepu.ZetonyPostepu;
        var wybraneZetony = zetonyPostepu.OrderBy(x => Guid.NewGuid()).Take(5).ToList();
        var strefy = ZbiorStref.Strefy.ToList();
        
        var gracze = new[] { new Gracz("GraczA"), new Gracz("GraczB") };
        var plansza = new PlanszaKonfliktu(pionKonfliktu, wybraneZetony, strefy, gracze);
        
        var stanGry = new StanGry();
        
        var nazwyKartGracza0 = new[] { "Kamieniolom", "Tawerna", "Wytwornia Papirusu", "Warsztat", "Oltarz", "Laznie" };
        var nazwyKartGracza1 = new[] { "Glinianka", "Huta Szkla", "Apteka", "Garnizon", "Skryptorium", "Teatr" };

        foreach (var nazwaKarty in nazwyKartGracza0)
        {
            var karta = ZbiorKart.TaliaEpokiI.FirstOrDefault(k => k.Nazwa == nazwaKarty);
            if (karta != null)
            {
                karta.OznaczJakoNiezagrana();
                gracze[0].ZbudujKarte(karta, gracze[1], plansza);
                _output.WriteLine($"Gracz 1 zbudował kartę: {karta.Nazwa}, Kolor: {karta.KolorKarty} (Koszt: {karta.WypiszKoszt()})");
                _output.WriteLine($"Monety Gracza 1 po zbudowaniu karty: {gracze[0].WypiszLiczbeSurowca(Surowiec.Monety)}");
            }
        }
        foreach (var nazwaKarty in nazwyKartGracza1)
        {
            var karta = ZbiorKart.TaliaEpokiI.FirstOrDefault(k => k.Nazwa == nazwaKarty);
            if (karta != null)
            {
                karta.OznaczJakoNiezagrana();
                gracze[1].ZbudujKarte(karta, gracze[0], plansza);
                _output.WriteLine($"Gracz 2 zbudował kartę: {karta.Nazwa}, Kolor: {karta.KolorKarty} (Koszt: {karta.WypiszKoszt()})");
                _output.WriteLine($"Monety Gracza 2 po zbudowaniu karty: {gracze[1].WypiszLiczbeSurowca(Surowiec.Monety)}");
            }
        }

        stanGry.CzyZwyciestwoPunktowe(gracze, plansza);
        Assert.True(stanGry.CzyZakonczona);
        Assert.Equal(gracze[0], stanGry.Zwyciezca);
        Assert.Equal(TypZwyciestwa.Punktowe, stanGry.TypZwyciestwa);

        _output.WriteLine($"Punkty Gracza 1: {stanGry.GetPunktyGracza1()}");
        _output.WriteLine($"Punkty Gracza 2: {stanGry.GetPunktyGracza2()}");

        _output.WriteLine($"Pozycja Pionu Konfliktu: {pionKonfliktu.PobierzPozycje()}");

        Assert.Equal(-1, pionKonfliktu.PobierzPozycje());

        Assert.Equal(10, stanGry.GetPunktyGracza1());
        Assert.Equal(7, stanGry.GetPunktyGracza2());
    }
    [Fact]
    public void Test_Zwyciestwo_Punktowe_WarunekDodatkowy() // TODO: SPRAWDZIC KARTY
    {
        IRandom random = new RandomAdapter(12345);
        var gra = Gra.StworzNowaGre(random: random);
        var gracze = gra.Gracze;
        var plansza = gra.PlanszaKonfliktu;
        var pionKonfliktu = gra.PlanszaKonfliktu.PionKonfliktu;
        var stanGry = gra.StanGry;

        var nazwyKartGracza0 = new[] { "Kamieniolom", "Wytwornia Papirusu", "Warsztat", "Oltarz", "Laznie" };
        var nazwyKartGracza1 = new[] { "Glinianka", "Huta Szkla", "Apteka", "Garnizon", "Skryptorium", "Teatr", "Biblioteka" };

        foreach (var nazwaKarty in nazwyKartGracza0)
        {
            var karta = ZbiorKart.TaliaEpokiI.FirstOrDefault(k => k.Nazwa == nazwaKarty);
            if (karta != null)
            {
                karta.OznaczJakoNiezagrana();
                gracze[0].ZbudujKarte(karta, gracze[1], plansza);
                _output.WriteLine($"Gracz 1 zbudował kartę: {karta.Nazwa}, Kolor: {karta.KolorKarty} (Koszt: {karta.WypiszKoszt()})");
                _output.WriteLine($"Monety Gracza 1 po zbudowaniu karty: {gracze[0].WypiszLiczbeSurowca(Surowiec.Monety)}");
            }
        }
        foreach (var nazwaKarty in nazwyKartGracza1)
        {
            var karta = ZbiorKart.TaliaEpokiI.FirstOrDefault(k => k.Nazwa == nazwaKarty);
            if (karta == null)
                karta = ZbiorKart.TaliaEpokiII.FirstOrDefault(k => k.Nazwa == nazwaKarty);
            if (karta != null)
            {
                karta.OznaczJakoNiezagrana();
                gracze[1].ZbudujKarte(karta, gracze[0], plansza);
                _output.WriteLine($"Gracz 2 zbudował kartę: {karta.Nazwa}, Kolor: {karta.KolorKarty} (Koszt: {karta.WypiszKoszt()})");
                _output.WriteLine($"Monety Gracza 2 po zbudowaniu karty: {gracze[1].WypiszLiczbeSurowca(Surowiec.Monety)}");
            }
        }

        stanGry.CzyZwyciestwoPunktowe(gracze, plansza);
        Assert.True(stanGry.CzyZakonczona);
        Assert.Equal(gracze[0].Nazwa, stanGry.Zwyciezca.Nazwa);
        Assert.Equal(TypZwyciestwa.Punktowe, stanGry.TypZwyciestwa);

        _output.WriteLine($"Punkty Gracza 1: {stanGry.GetPunktyGracza1()}");
        _output.WriteLine($"Punkty Gracza 2: {stanGry.GetPunktyGracza2()}");

        _output.WriteLine($"Pozycja Pionu Konfliktu: {pionKonfliktu.PobierzPozycje()}");

        Assert.Equal(-1, pionKonfliktu.PobierzPozycje());

        Assert.Equal(9, stanGry.GetPunktyGracza1());
        Assert.Equal(9, stanGry.GetPunktyGracza2());
    }
    [Fact]
    public void Test_Zwyciestwo_Punktowe_Remis()
    {
        var pionKonfliktu = new PionKonfliktu(0);
        var zetonyPostepu = ZbiorZetonowPostepu.ZetonyPostepu;
        var wybraneZetony = zetonyPostepu.OrderBy(x => Guid.NewGuid()).Take(5).ToList();
        var strefy = ZbiorStref.Strefy.ToList();

        var gracze = new[] { new Gracz("GraczA"), new Gracz("GraczB") };
        var plansza = new PlanszaKonfliktu(pionKonfliktu, wybraneZetony, strefy, gracze);

        var stanGry = new StanGry();

        var nazwyKartGracza0 = new[] { "Kamieniolom", "Tawerna", "Wytwornia Papirusu", "Warsztat", "Oltarz" };
        var nazwyKartGracza1 = new[] { "Glinianka", "Huta Szkla", "Apteka", "Garnizon", "Skryptorium", "Teatr" };

        foreach (var nazwaKarty in nazwyKartGracza0)
        {
            var karta = ZbiorKart.TaliaEpokiI.FirstOrDefault(k => k.Nazwa == nazwaKarty);
            if (karta != null)
            {
                karta.OznaczJakoNiezagrana();
                gracze[0].ZbudujKarte(karta, gracze[1], plansza);
                _output.WriteLine($"Gracz 1 zbudował kartę: {karta.Nazwa}, Kolor: {karta.KolorKarty} (Koszt: {karta.WypiszKoszt()})");
                _output.WriteLine($"Monety Gracza 1 po zbudowaniu karty: {gracze[0].WypiszLiczbeSurowca(Surowiec.Monety)}");
            }
        }
        foreach (var nazwaKarty in nazwyKartGracza1)
        {
            var karta = ZbiorKart.TaliaEpokiI.FirstOrDefault(k => k.Nazwa == nazwaKarty);
            if (karta != null)
            {
                karta.OznaczJakoNiezagrana();
                gracze[1].ZbudujKarte(karta, gracze[0], plansza);
                _output.WriteLine($"Gracz 2 zbudował kartę: {karta.Nazwa}, Kolor: {karta.KolorKarty} (Koszt: {karta.WypiszKoszt()})");
                _output.WriteLine($"Monety Gracza 2 po zbudowaniu karty: {gracze[1].WypiszLiczbeSurowca(Surowiec.Monety)}");
            }
        }

        stanGry.CzyZwyciestwoPunktowe(gracze, plansza);
        Assert.True(stanGry.CzyZakonczona);
        Assert.Null(stanGry.Zwyciezca);
        Assert.Equal(TypZwyciestwa.Brak, stanGry.TypZwyciestwa);

        _output.WriteLine($"Punkty Gracza 1: {stanGry.GetPunktyGracza1()}");
        _output.WriteLine($"Punkty Gracza 2: {stanGry.GetPunktyGracza2()}");

        _output.WriteLine($"Pozycja Pionu Konfliktu: {pionKonfliktu.PobierzPozycje()}");

        Assert.Equal(-1, pionKonfliktu.PobierzPozycje());

        Assert.Equal(7, stanGry.GetPunktyGracza1());
        Assert.Equal(7, stanGry.GetPunktyGracza2());
    }
}

