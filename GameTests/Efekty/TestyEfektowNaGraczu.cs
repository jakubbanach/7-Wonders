using Xunit.Abstractions;

public class TestyEfektowNaGraczu
{
    private readonly ITestOutputHelper _output;

    public TestyEfektowNaGraczu(ITestOutputHelper output)
    {
        _output = output;
    }
    [Fact]
    public void Efekt_Surowiec()
    {
        Gracz gracz = new Gracz("TestowyGracz");

        var efekt = new Efekt(
            TypEfektu.Surowiec,
            surowce: new Dictionary<Surowiec, int> { { Surowiec.Drewno, 2 }, { Surowiec.Glina, 1 } }
        );

        efekt.ZastosujEfekt(gracz);
        Assert.Equal(2, gracz.WypiszLiczbeSurowca(Surowiec.Drewno));
        Assert.Equal(1, gracz.WypiszLiczbeSurowca(Surowiec.Glina));
        Assert.Equal(0, gracz.WypiszLiczbeSurowca(Surowiec.Kamień));
        Assert.Equal(7, gracz.WypiszLiczbeSurowca(Surowiec.Monety)); // Początkowe monety
    }

    [Fact]
    public void Efekt_WyborSurowca_Powinien_Zmniejszyc_Koszt()
    {
        Gracz gracz = new Gracz("TestowyGracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");

        var efekt = new Efekt(
            TypEfektu.WyborSurowca,
            new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 }, { Surowiec.Glina, 1 }, { Surowiec.Kamień, 1 } }
        );

        var karta = ZbiorKart.TaliaEpokiI.First(k => k.Nazwa == "Stajnie"); 
        var koszt = karta.ObliczKoszt(gracz, przeciwnik);

        Assert.Equal(2, koszt);
        gracz.DodajEfekt(efekt);

        koszt = karta.ObliczKoszt(gracz, przeciwnik);
        Assert.Equal(0, koszt);
    }

    [Fact]
    public void Efekt_WyborSurowca_2_karty()
    {
        Gracz gracz = new Gracz("TestowyGracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");

        var efekt = new Efekt(
            TypEfektu.WyborSurowca,
            new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 }, { Surowiec.Glina, 1 }, { Surowiec.Kamień, 1 } }
        );

        var karta = ZbiorKart.TaliaEpokiII.First(k => k.Nazwa == "Biblioteka"); 

        var koszt = karta.ObliczKoszt(gracz, przeciwnik);

        Assert.Equal(6, koszt);
        
        gracz.DodajEfekt(efekt);
        koszt = karta.ObliczKoszt(gracz, przeciwnik);   
        Assert.Equal(4, koszt);
        
        gracz.DodajEfekt(efekt);
        koszt = karta.ObliczKoszt(gracz, przeciwnik);
        Assert.Equal(2, koszt);
    }

    [Fact]
    public void Efekt_WyborSurowca()
    {
        Gracz gracz = new Gracz("TestowyGracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");

        var efekt = new Efekt(
            TypEfektu.WyborSurowca, 
            new Dictionary<Surowiec, int> {{ Surowiec.Drewno, 1 }, { Surowiec.Glina, 1 }, { Surowiec.Kamień, 1 }}
        );

        // Symulacja wyboru najlepszego surowca do danej karty
        var karta = ZbiorKart.TaliaEpokiII.First(k => k.Nazwa == "Biblioteka"); // koszt: 1 drewno, 1 kamien, 1 szklo
        _output.WriteLine($"Testowana karta: {karta.Nazwa} (Koszt: {karta.WypiszKoszt()})");
        var koszt = karta.ObliczKoszt(gracz, przeciwnik);
        _output.WriteLine($"Koszt przed efektem: {koszt}");
        Assert.Equal(6, koszt);

        przeciwnik.DodajSurowiec(Surowiec.Drewno, 1);

        // Koszt się zwiększa o 1, ponieważ przeciwnik ma więcej drewna
        koszt = karta.ObliczKoszt(gracz, przeciwnik);
        _output.WriteLine($"Koszt po dodaniu drewna przeciwnikowi: {koszt}");
        Assert.Equal(7, koszt);

        gracz.DodajEfekt(efekt);
        // silnik powinien wybrać drewno, bo koszt drewna jest większy niż koszt kamienia

        koszt = karta.ObliczKoszt(gracz, przeciwnik);
        _output.WriteLine($"Koszt po zastosowaniu efektu: {koszt}");
        Assert.Equal(4, koszt); // drewno teraz jest darmowe (więc płacimy za 2 surowce)
    }
    [Fact]
    public void Efekt_WyborSurowca_NadmiarPosiadanychSurowcow()
    {
        Gracz gracz = new Gracz("TestowyGracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");

        var efekt = new Efekt(
            TypEfektu.WyborSurowca, 
            new Dictionary<Surowiec, int> {{ Surowiec.Drewno, 1 }, { Surowiec.Glina, 1 }, { Surowiec.Kamień, 1 }}
        );

        // Symulacja wyboru najlepszego surowca do danej karty
        var karta = ZbiorKart.TaliaEpokiII.First(k => k.Nazwa == "Posąg"); // koszt: 2 glina
        _output.WriteLine($"Testowana karta: {karta.Nazwa} (Koszt: {karta.WypiszKoszt()})");
        var koszt = karta.ObliczKoszt(gracz, przeciwnik);
        _output.WriteLine($"Koszt przed efektem: {koszt}");
        Assert.Equal(4, koszt);

        gracz.DodajSurowiec(Surowiec.Glina, 2);
        
        koszt = karta.ObliczKoszt(gracz, przeciwnik);
        _output.WriteLine($"Koszt po dodaniu 2x gliny graczowi: {koszt}");
        Assert.Equal(0, koszt);

        gracz.DodajEfekt(efekt);
        // silnik nic nie powinien wybierać, bo gracz ma już wystarczająco surowca, żeby zbudować kartę

        koszt = karta.ObliczKoszt(gracz, przeciwnik);
        _output.WriteLine($"Koszt po zastosowaniu efektu: {koszt}");
        Assert.Equal(0, koszt); // drewno teraz jest darmowe (więc płacimy za 2 surowce)
    }
    [Fact]
    public void Efekt_WyborSurowca_NadmiarPosiadanychSurowcow_Monety()
    {
        Gracz gracz = new Gracz("TestowyGracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");

        var efekt = new Efekt(
            TypEfektu.WyborSurowca, 
            new Dictionary<Surowiec, int> {{ Surowiec.Drewno, 1 }, { Surowiec.Glina, 1 }, { Surowiec.Kamień, 1 }}
        );

        // Symulacja wyboru najlepszego surowca do danej karty
        var karta = ZbiorKart.TaliaEpokiII.First(k => k.Nazwa == "Forum"); // koszt: 3 monety, 1 glina
        _output.WriteLine($"Testowana karta: {karta.Nazwa} (Koszt: {karta.WypiszKoszt()})");
        var koszt = karta.ObliczKoszt(gracz, przeciwnik);
        _output.WriteLine($"Koszt przed efektem: {koszt}");
        Assert.Equal(5, koszt);

        gracz.DodajSurowiec(Surowiec.Glina, 1);
        
        koszt = karta.ObliczKoszt(gracz, przeciwnik);
        _output.WriteLine($"Koszt po dodaniu gliny graczowi: {koszt}");
        Assert.Equal(3, koszt);

        gracz.DodajEfekt(efekt);
        // silnik nic nie powinien wybierać, bo gracz ma już wystarczająco surowca, żeby zbudować kartę

        koszt = karta.ObliczKoszt(gracz, przeciwnik);
        _output.WriteLine($"Koszt po zastosowaniu efektu: {koszt}");
        Assert.Equal(3, koszt); // drewno teraz jest darmowe (więc płacimy za 2 surowce)
    }

    [Fact]
    public void Efekt_ZmianaCenySurowca()
    {
        Gracz gracz = new Gracz("TestowyGracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");

        var efekt = new Efekt(
            TypEfektu.ZmianaCenySurowca,
            surowiec: Surowiec.Kamień, 
            wartość: 1
        );

        // Symulacja wyboru najlepszego surowca do danej karty
        var karta = ZbiorKart.TaliaEpokiI.First(k => k.Nazwa == "Łaźnie"); // koszt: 1 kamien
        _output.WriteLine($"Testowana karta: {karta.Nazwa} (Koszt: {karta.WypiszKoszt()})");
        var koszt = karta.ObliczKoszt(gracz, przeciwnik);
        _output.WriteLine($"Koszt przed efektem: {koszt}");
        Assert.Equal(2, koszt);

        gracz.DodajEfekt(efekt);

        koszt = karta.ObliczKoszt(gracz, przeciwnik);
        _output.WriteLine($"Koszt po zastosowaniu efektu: {koszt}");
        Assert.Equal(1, koszt);
    }
    [Fact]
    public void Efekt_ZmianaCenySurowca_i_Efekt_WyborSurowca()
    {
        Gracz gracz = new Gracz("TestowyGracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");

        var efektZmiana = new Efekt(
            TypEfektu.ZmianaCenySurowca,
            surowiec: Surowiec.Kamień, 
            wartość: 1
        );

        var efektWybor = new Efekt(
            TypEfektu.WyborSurowca,
            new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 }, { Surowiec.Glina, 1 }, { Surowiec.Kamień, 1 } }
        );

        var karta = ZbiorKart.TaliaEpokiII.First(k => k.Nazwa == "Biblioteka"); // koszt: 1 drewno, 1 kamien, 1 szklo
        _output.WriteLine($"Testowana karta: {karta.Nazwa} (Koszt: {karta.WypiszKoszt()})");
        var koszt = karta.ObliczKoszt(gracz, przeciwnik);
        _output.WriteLine($"Koszt przed efektem: {koszt}");
        Assert.Equal(6, koszt);

        przeciwnik.DodajSurowiec(Surowiec.Kamień, 1);

        // Koszt się zwiększa o 1, ponieważ przeciwnik ma więcej drewna
        koszt = karta.ObliczKoszt(gracz, przeciwnik);
        _output.WriteLine($"Koszt po dodaniu drewna przeciwnikowi: {koszt}");
        Assert.Equal(7, koszt);

        gracz.DodajEfekt(efektZmiana);

        koszt = karta.ObliczKoszt(gracz, przeciwnik);
        _output.WriteLine($"Koszt po zastosowaniu efektu zmiany ceny: {koszt}");
        Assert.Equal(5, koszt); // kamień jest za 1 (więc płacimy za 2 surowce i 1 za kamień)

        gracz.DodajEfekt(efektWybor);
        koszt = karta.ObliczKoszt(gracz, przeciwnik);
        _output.WriteLine($"Koszt po zastosowaniu efektu wyboru surowca: {koszt}");
        Assert.Equal(3, koszt); // drewno jest darmowe, więc płacimy tylko za drewno i szkło
    }

    [Fact]
    public void Efekt_MonetyPrzeciwnikaZaMaterialy()
    {
        Gracz gracz = new Gracz("Gracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");

        var efekt = new Efekt(TypEfektu.MonetyPrzeciwnikaZaMaterialy);

        // Symulacja wyboru najlepszego surowca do danej karty
        var karta = ZbiorKart.TaliaEpokiI.First(k => k.Nazwa == "Łaźnie"); // koszt: 1 kamien
        _output.WriteLine($"Testowana karta: {karta.Nazwa} (Koszt: {karta.WypiszKoszt()})");
        var koszt = karta.ObliczKoszt(gracz, przeciwnik);
        _output.WriteLine($"Koszt przed efektem: {koszt}");
        Assert.Equal(2, koszt);

        gracz.DodajEfekt(efekt);

        przeciwnik.ZbudujKarte(karta, gracz);
        _output.WriteLine($"Monety przeciwnika po zbudowaniu karty: {przeciwnik.WypiszLiczbeSurowca(Surowiec.Monety)}");
        Assert.Equal(7 - 2, przeciwnik.WypiszLiczbeSurowca(Surowiec.Monety)); // Przeciwnik traci monety za surowce;
        Assert.Equal(7 + 2, gracz.WypiszLiczbeSurowca(Surowiec.Monety)); // Gracz dostaje monety przeciwnika za surowce;
    }
    [Fact]
    public void Efekt_MonetyPrzeciwnikaZaMaterialy_Monety()
    {
        Gracz gracz = new Gracz("Gracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");

        var efekt = new Efekt(TypEfektu.MonetyPrzeciwnikaZaMaterialy);

        // Symulacja wyboru najlepszego surowca do danej karty
        var karta = ZbiorKart.TaliaEpokiII.First(k => k.Nazwa == "Karawanseraj"); // koszt: 2 monety, 1 szkło, 1 papirus
        karta.OznaczJakoNiezagrana(); // Upewniamy się, że karta jest niezagrana, aby można było ją zbudować
        _output.WriteLine($"Testowana karta: {karta.Nazwa} (Koszt: {karta.WypiszKoszt()})");
        var koszt = karta.ObliczKoszt(gracz, przeciwnik);
        _output.WriteLine($"Koszt przed efektem: {koszt}");
        Assert.Equal(6, koszt);

        gracz.DodajEfekt(efekt);

        przeciwnik.ZbudujKarte(karta, gracz);
        _output.WriteLine($"Monety przeciwnika po zbudowaniu karty: {przeciwnik.WypiszLiczbeSurowca(Surowiec.Monety)}");
        Assert.Equal(7 - 6, przeciwnik.WypiszLiczbeSurowca(Surowiec.Monety)); // Przeciwnik traci monety za surowce;
        Assert.Equal(7 + 4, gracz.WypiszLiczbeSurowca(Surowiec.Monety)); // Gracz dostaje monety przeciwnika za surowce;
    }

    [Fact]
    public void Efekt_PunktyZwyciestwa()
    {
        Gracz gracz = new Gracz("TestowyGracz");
        var efekt = new Efekt(
            TypEfektu.PunktyZwyciestwa,
            wartość: 5
        );
        efekt.ZastosujEfekt(gracz);
        Assert.Equal(5, gracz.PunktyZwyciestwa);
    }
    [Fact]
    public void Efekt_Monety()
    {
        Gracz gracz = new Gracz("TestowyGracz");
        var efekt = new Efekt(
            TypEfektu.Monety,
            wartość: 3
        );
        efekt.ZastosujEfekt(gracz);
        Assert.Equal(10, gracz.WypiszLiczbeSurowca(Surowiec.Monety)); // Początkowe 7 + 3 z efektu
    }
    [Fact]
    public void Efekt_PrzeciwnikOdkladaMonety()
    {
        Gracz gracz = new Gracz("Gracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");
        var efekt = new Efekt(
            TypEfektu.PrzeciwnikOdkladaMonety,
            wartość: 2
        );
        efekt.ZastosujEfekt(gracz, przeciwnik);
        Assert.Equal(7 - 2, przeciwnik.WypiszLiczbeSurowca(Surowiec.Monety)); // Przeciwnik traci monety;
    }
    [Fact]
    public void Efekt_PunktyMilitarne()
    {
        Gracz gracz = new Gracz("GraczA");
        Gracz przeciwnik = new Gracz("GraczB");
        var pionKonfliktu = new PionKonfliktu(0);
        var zetonyPostepu = ZbiorZetonowPostepu.ZetonyPostepu;
        var wybraneZetony = zetonyPostepu.OrderBy(x => Guid.NewGuid()).Take(5).ToList();
        var strefy = ZbiorStref.Strefy.ToList();

        var plansza = new PlanszaKonfliktu(pionKonfliktu, wybraneZetony, strefy, gracze: new[] { gracz, przeciwnik });
        var efekt = new Efekt(
            TypEfektu.PunktyMilitarne,
            wartość: 2
        );
        efekt.ZastosujEfekt(gracz, przeciwnik, plansza);
        Assert.Equal(2, plansza.PionKonfliktu.PobierzPozycje());
    }
    [Fact]
    public void Efekt_BialySymbol()
    {
        Gracz gracz = new Gracz("TestowyGracz");
        var efekt = new Efekt(
            TypEfektu.BialySymbol,
            tekst: "TestowySymbol"
        );
        efekt.ZastosujEfekt(gracz);
        Assert.Contains("TestowySymbol", gracz.BialeSymbole);
    }
    [Fact]
    public void Efekt_SymbolNaukowy()
    {
        Gracz gracz = new Gracz("TestowyGracz");
        var efekt = new Efekt(
            TypEfektu.SymbolNaukowy,
            symbolNaukowy: SymbolNaukowy.Koło
        );
        efekt.ZastosujEfekt(gracz);
        Assert.Contains(SymbolNaukowy.Koło, gracz.SymboleNaukowe);
    }
    [Fact]
    public void Efekt_MonetyZaKarty_Cuda()
    {
        Gracz gracz = new Gracz("Gracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");
        var kartaCudu = ZbiorKart.TaliaKartyCudow.First();
        gracz.DodajKarteCudu(kartaCudu);
        _output.WriteLine($"Testowana karta cudu: {kartaCudu.Nazwa}");

        var efekt = new Efekt(
            TypEfektu.MonetyZaKarty, 
            tekst: "Cuda", 
            wartość: 2
        );
        efekt.ZastosujEfekt(gracz, przeciwnik);

        Assert.Equal(7 + 2, gracz.WypiszLiczbeSurowca(Surowiec.Monety)); // Początkowe 7 + 2 monety za każdą z kart cudu
    }
    [Fact]
    public void Efekt_MonetyZaKarty_Cuda_PrzeciwnikMaWiecejKart()
    {
        Gracz gracz = new Gracz("Gracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");
        var kartaCudu = ZbiorKart.TaliaKartyCudow.First();
        var drugaKartaCudu = ZbiorKart.TaliaKartyCudow.Skip(1).First();
        var trzeciaKartaCudu = ZbiorKart.TaliaKartyCudow.Skip(2).First();
        gracz.DodajKarteCudu(kartaCudu);
        _output.WriteLine($"Testowana karta cudu: {kartaCudu.Nazwa}");

        przeciwnik.DodajKarteCudu(drugaKartaCudu);
        przeciwnik.DodajKarteCudu(trzeciaKartaCudu);

        var efekt = new Efekt(
            TypEfektu.MonetyZaKarty, 
            tekst: "Cuda", 
            wartość: 2
        );
        efekt.ZastosujEfekt(gracz, przeciwnik);

        Assert.Equal(7 + 4, gracz.WypiszLiczbeSurowca(Surowiec.Monety)); // Początkowe 7 + 2 monety za każdą z 2 kart cudu przeciwnika
    }
    [Fact]
    public void Efekt_MonetyZaKarty_Kolor()
    {
        Gracz gracz = new Gracz("Gracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");
        var talia = ZbiorKart.TaliaEpokiII;

        var zolteKarty = talia.Where(k => k.KolorKarty == KolorKarty.Żółty).Take(3).ToList();
        gracz.DodajMonety(10); // Dodajemy trochę monet, żeby gracz mógł zbudować karty
        foreach (var karta in zolteKarty)
        {
            gracz.ZbudujKarte(karta, przeciwnik);
            _output.WriteLine($"Zbudowana karta: {karta.Nazwa} (Kolor: {karta.KolorKarty})");
        }

        var efekt = new Efekt(
            TypEfektu.MonetyZaKarty, 
            tekst: "Żółty", 
            wartość: 1
        );

        var monetyPrzed = gracz.WypiszLiczbeSurowca(Surowiec.Monety);
        efekt.ZastosujEfekt(gracz, przeciwnik);

        Assert.Equal(3, gracz.PobierzZbudowaneKarty().Count(k => k.KolorKarty == KolorKarty.Żółty));
        Assert.Equal(monetyPrzed + 3, gracz.WypiszLiczbeSurowca(Surowiec.Monety)); // Początkowe + 3 monety za każdą z 3 żółtych kart
    }
    [Fact]
    public void Efekt_MonetyZaKarty_Kolor_PrzeciwnikMaWiecejKart()
    {
        Gracz gracz = new Gracz("Gracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");

        var zieloneKarty = ZbiorKart.TaliaEpokiI.Where(k => k.KolorKarty == KolorKarty.Zielony).Take(3).ToList();
        var zieloneKartyPrzeciwnika = ZbiorKart.TaliaEpokiII.Where(k => k.KolorKarty == KolorKarty.Zielony).Take(4).ToList();
        gracz.DodajMonety(10); // Dodajemy trochę monet, żeby gracz mógł zbudować karty
        przeciwnik.DodajMonety(20); // Dodajemy trochę monet, żeby przeciwnik mógł zbudować karty
        foreach (var karta in zieloneKarty)
        {
            karta.OznaczJakoNiezagrana(); // Upewniamy się, że karta jest niezagrana, aby można było ją zbudować
            gracz.ZbudujKarte(karta, przeciwnik);
            _output.WriteLine($"Zbudowana karta: {karta.Nazwa} (Kolor: {karta.KolorKarty})");
        }
        foreach (var karta in zieloneKartyPrzeciwnika)
        {
            karta.OznaczJakoNiezagrana(); // Upewniamy się, że karta jest niezagrana, aby można było ją zbudować
            przeciwnik.ZbudujKarte(karta, gracz);
            _output.WriteLine($"Zbudowana karta przeciwnika: {karta.Nazwa} (Kolor: {karta.KolorKarty})");
        }

        var efekt = new Efekt(
            TypEfektu.MonetyZaKarty, 
            tekst: "Zielony", 
            wartość: 1
        );

        var monetyPrzed = gracz.WypiszLiczbeSurowca(Surowiec.Monety);
        efekt.ZastosujEfekt(gracz, przeciwnik);

        Assert.Equal(3, gracz.PobierzZbudowaneKarty().Count(k => k.KolorKarty == KolorKarty.Zielony));
        Assert.Equal(4, przeciwnik.PobierzZbudowaneKarty().Count(k => k.KolorKarty == KolorKarty.Zielony));
        Assert.Equal(monetyPrzed + 4, gracz.WypiszLiczbeSurowca(Surowiec.Monety)); // Początkowe + 4 monety za każdą z 4 zielonych kart przeciwnika
    }
    [Fact]
    public void Efekt_MonetyZaKarty_KolorBrazowyISzary()
    {
        Gracz gracz = new Gracz("Gracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");
        var talia = ZbiorKart.TaliaEpokiII;

        var karty = talia.Where(k => k.KolorKarty == KolorKarty.Brązowy).Take(3).ToList().Concat
            (talia.Where(k => k.KolorKarty == KolorKarty.Szary).Take(2)).ToList();
        gracz.DodajMonety(10); // Dodajemy trochę monet, żeby gracz mógł zbudować karty
        foreach (var karta in karty)
        {
            gracz.ZbudujKarte(karta, przeciwnik);
            _output.WriteLine($"Zbudowana karta: {karta.Nazwa} (Kolor: {karta.KolorKarty})");
        }

        var efekt = new Efekt(
            TypEfektu.MonetyZaKarty,
            tekst: "Brązowy i Szary",
            wartość: 1
        );

        var monetyPrzed = gracz.WypiszLiczbeSurowca(Surowiec.Monety);
        efekt.ZastosujEfekt(gracz, przeciwnik);

        Assert.Equal(3, gracz.PobierzZbudowaneKarty().Count(k => k.KolorKarty == KolorKarty.Brązowy));
        Assert.Equal(2, gracz.PobierzZbudowaneKarty().Count(k => k.KolorKarty == KolorKarty.Szary));
        Assert.Equal(monetyPrzed + 5, gracz.WypiszLiczbeSurowca(Surowiec.Monety)); // Początkowe + 5 monety za każdą z kart brązowych i szarych
    }

    [Fact]
    public void Test_Koszt_MniejMaterialowNaNiebieskieKarty()
    {
        Gracz gracz = new Gracz("Gracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");

        var efekt = new Efekt(
            TypEfektu.MniejMaterialowNaNiebieskieKarty, 
            wartość: 2 // oznacza że nie są brane 2 najdroższe surowce do kosztu karty, ale tylko jeśli karta jest niebieska
        );

        // Symulacja wyboru najlepszego surowca do danej karty
        var karta = ZbiorKart.TaliaEpokiII.First(k => k.Nazwa == "Gmach Sądu"); // koszt: 2 drewno, 1 szkło (niebieska karta)
        _output.WriteLine($"Testowana karta: {karta.Nazwa}, Kolor: {karta.KolorKarty} (Koszt: {karta.WypiszKoszt()})");
        var koszt = karta.ObliczKoszt(gracz, przeciwnik, karta);
        _output.WriteLine($"Koszt przed efektem: {koszt}");
        Assert.Equal(6, koszt);

        gracz.DodajEfekt(efekt);

        koszt = karta.ObliczKoszt(gracz, przeciwnik, karta);
        _output.WriteLine($"Koszt po zastosowaniu efektu: {koszt}");
        Assert.Equal(2, koszt);
    }
    [Fact]
    public void Test_Koszt_MniejMaterialowNaNiebieskieKarty2()
    {
        Gracz gracz = new Gracz("Gracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");

        var efekt = new Efekt(
            TypEfektu.MniejMaterialowNaNiebieskieKarty, 
            wartość: 2 // oznacza że nie są brane 2 najdroższe surowce do kosztu karty, ale tylko jeśli karta jest niebieska
        );

        // Symulacja wyboru najlepszego surowca do danej karty
        var karta = ZbiorKart.TaliaEpokiIII.First(k => k.Nazwa == "Pałac"); // koszt: 1 drewno, 1 glina, 1 kamień, 2 szkło (niebieska karta)
        _output.WriteLine($"Testowana karta: {karta.Nazwa}, Kolor: {karta.KolorKarty} (Koszt: {karta.WypiszKoszt()})");
        var koszt = karta.ObliczKoszt(gracz, przeciwnik);
        _output.WriteLine($"Koszt przed efektem: {koszt}");
        Assert.Equal(10, koszt);

        gracz.DodajSurowiec(Surowiec.Drewno, 1);
        przeciwnik.DodajSurowiec(Surowiec.Szkło, 2);
        koszt = karta.ObliczKoszt(gracz, przeciwnik, karta);
        _output.WriteLine($"Koszt po dodaniu surowców efektu: {koszt}");
        Assert.Equal(12, koszt);

        gracz.DodajEfekt(efekt);

        koszt = karta.ObliczKoszt(gracz, przeciwnik, karta);
        _output.WriteLine($"Koszt po zastosowaniu efektu: {koszt}");
        Assert.Equal(4, koszt);
    }
    [Fact]
    public void Test_Koszt_MniejMaterialowNaNiebieskieKarty3()
    {
        Gracz gracz = new Gracz("Gracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");

        var efekt = new Efekt(
            TypEfektu.MniejMaterialowNaNiebieskieKarty, 
            wartość: 2 // oznacza że nie są brane 2 najdroższe surowce do kosztu karty, ale tylko jeśli karta jest niebieska
        );

        // Symulacja wyboru najlepszego surowca do danej karty
        var karta = ZbiorKart.TaliaEpokiII.First(k => k.Nazwa == "Świątynia"); // koszt: 1 drewno, 1 papier (niebieska karta)
        _output.WriteLine($"Testowana karta: {karta.Nazwa}, Kolor: {karta.KolorKarty} (Koszt: {karta.WypiszKoszt()})");
        var koszt = karta.ObliczKoszt(gracz, przeciwnik, karta);
        _output.WriteLine($"Koszt przed efektem: {koszt}");
        Assert.Equal(4, koszt);

        gracz.DodajSurowiec(Surowiec.Drewno, 1);
        koszt = karta.ObliczKoszt(gracz, przeciwnik, karta);
        _output.WriteLine($"Koszt po dodaniu surowców efektu: {koszt}");
        Assert.Equal(2, koszt);

        gracz.DodajEfekt(efekt);

        koszt = karta.ObliczKoszt(gracz, przeciwnik, karta);
        _output.WriteLine($"Koszt po zastosowaniu efektu: {koszt}");
        Assert.Equal(0, koszt);
    }
    [Fact]
    public void Test_Koszt_MniejMaterialowZaCuda()
    {
        Gracz gracz = new Gracz("Gracz");
        Gracz przeciwnik = new Gracz("Przeciwnik");

        var efekt = new Efekt(
            TypEfektu.MniejMaterialowNaCuda,
            wartość: 2 // oznacza że nie są brane 2 najdroższe surowce do kosztu karty, ale tylko jeśli karta jest niebieska
        );

        // Symulacja wyboru najlepszego surowca do danej karty
        var karta = ZbiorKart.TaliaEpokiII.First(k => k.Nazwa == "Gmach Sądu"); // koszt: 3 glina, 1 szkło (niebieska karta)
        var kartaCudu = ZbiorKart.TaliaKartyCudow.First(k => k.Nazwa == "Kolos Rodyjski");
        
        karta.OznaczJakoNiezagrana();
        kartaCudu.OznaczJakoNiezagrana();

        _output.WriteLine($"Testowana karta: {kartaCudu.Nazwa} (Koszt: {kartaCudu.WypiszKoszt()})");

        var koszt = kartaCudu.ObliczKoszt(gracz, przeciwnik, karta);
        _output.WriteLine($"Koszt przed efektem: {koszt}");
        Assert.Equal(8, koszt);

        gracz.DodajEfekt(efekt);

        koszt = kartaCudu.ObliczKoszt(gracz, przeciwnik, kartaCudu: kartaCudu);
        _output.WriteLine($"Koszt po zastosowaniu efektu: {koszt}");
        Assert.Equal(4, koszt);

        przeciwnik.DodajSurowiec(Surowiec.Glina, 1);
        koszt = kartaCudu.ObliczKoszt(gracz, przeciwnik, kartaCudu: kartaCudu);
        _output.WriteLine($"Koszt po dodaniu 1x gliny przeciwnikowi: {koszt}");

        Assert.Equal(5, koszt); //zostanie koszt za 1 glinę (2+1) i 1 szkło (2)
        przeciwnik.DodajSurowiec(Surowiec.Szkło, 2);

        koszt = kartaCudu.ObliczKoszt(gracz, przeciwnik, kartaCudu: kartaCudu);
        _output.WriteLine($"Koszt po dodaniu jeszcze 2x szkła przeciwnikowi: {koszt}");
        Assert.Equal(6, koszt); //zostanie koszt za 2 gliny (2+1), bo szkło jest droższe przez przeciwnika (2+2)
    }

    [Fact]
    public void Test_DodatkoweMilitariaZaCzerwoneKarty()
    {
        Gracz gracz = new Gracz("GraczA");
        Gracz przeciwnik = new Gracz("GraczB");
        var pionKonfliktu = new PionKonfliktu(0);
        var zetonyPostepu = ZbiorZetonowPostepu.ZetonyPostepu;
        var wybraneZetony = zetonyPostepu.OrderBy(x => Guid.NewGuid()).Take(5).ToList();
        var strefy = ZbiorStref.Strefy.ToList();

        var plansza = new PlanszaKonfliktu(pionKonfliktu, wybraneZetony, strefy, gracze: new[] { gracz, przeciwnik });

        var efekt = new Efekt(
            TypEfektu.DodatkoweMilitariaZaCzerwoneKarty
        );
        var czerwonaKarta = ZbiorKart.TaliaEpokiI.Where(k => k.KolorKarty == KolorKarty.Czerwony).Take(2).ToList();
        foreach (var karta in czerwonaKarta)
        {
            karta.OznaczJakoNiezagrana(); // Upewniamy się, że karta jest niezagrana, aby można było ją zbudować
        }

        _output.WriteLine($"Testowana karta: {czerwonaKarta[0].Nazwa}, Kolor: {czerwonaKarta[0].KolorKarty} (Koszt: {czerwonaKarta[0].WypiszKoszt()})");
        gracz.ZbudujKarte(czerwonaKarta[0], przeciwnik, plansza);
        
        Assert.Equal(1, plansza.PionKonfliktu.PobierzPozycje());
        Assert.Equal("Strefa 1 dla A", plansza.PobierzStrefeDlaPozycji(plansza.PionKonfliktu.PobierzPozycje()).Nazwa);

        gracz.DodajEfekt(efekt);

        _output.WriteLine($"Testowana karta: {czerwonaKarta[1].Nazwa}, Kolor: {czerwonaKarta[1].KolorKarty} (Koszt: {czerwonaKarta[1].WypiszKoszt()})");
        gracz.ZbudujKarte(czerwonaKarta[1], przeciwnik, plansza);

        Assert.Equal(3, plansza.PionKonfliktu.PobierzPozycje());
        Assert.Equal("Strefa 2 dla A", plansza.PobierzStrefeDlaPozycji(plansza.PionKonfliktu.PobierzPozycje()).Nazwa);
    }
    [Fact]
    public void Test_DodatkoweMilitariaZaCzerwoneKarty_Cud()
    {
        Gracz gracz = new Gracz("GraczA");
        Gracz przeciwnik = new Gracz("GraczB");
        var pionKonfliktu = new PionKonfliktu(0);
        var zetonyPostepu = ZbiorZetonowPostepu.ZetonyPostepu;
        var wybraneZetony = zetonyPostepu.OrderBy(x => Guid.NewGuid()).Take(5).ToList();
        var strefy = ZbiorStref.Strefy.ToList();

        var plansza = new PlanszaKonfliktu(pionKonfliktu, wybraneZetony, strefy, gracze: new[] { gracz, przeciwnik });

        var efekt = new Efekt(
            TypEfektu.DodatkoweMilitariaZaCzerwoneKarty
        );
        var czerwonaKarta = ZbiorKart.TaliaEpokiI.Where(k => k.KolorKarty == KolorKarty.Czerwony).Take(3).ToList();
        var kartaCudu = ZbiorKart.TaliaKartyCudow.First(k => k.Nazwa == "Kolos Rodyjski");

        _output.WriteLine($"Testowana karta: {kartaCudu.Nazwa} (Koszt: {kartaCudu.WypiszKoszt()})");
        gracz.DodajEfekt(efekt);
        gracz.DodajKarteCudu(kartaCudu);
        gracz.DodajSurowiec(Surowiec.Glina, 3);
        gracz.DodajSurowiec(Surowiec.Szkło, 1);
        gracz.ZbudujCud(czerwonaKarta[0], przeciwnik, kartaCudu, plansza);

        Assert.Equal(2, plansza.PionKonfliktu.PobierzPozycje());
        Assert.Equal("Strefa 1 dla A", plansza.PobierzStrefeDlaPozycji(plansza.PionKonfliktu.PobierzPozycje()).Nazwa);
        Assert.Equal(3, gracz.PunktyZwyciestwa);
    }
}
