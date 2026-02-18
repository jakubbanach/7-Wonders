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
        _output.WriteLine($"Testowana karta: {karta.Nazwa} (Koszt: {string.Join(", ", karta.Koszt.Select(k => $"{k.Value} {k.Key}"))})");
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
        _output.WriteLine($"Testowana karta: {karta.Nazwa} (Koszt: {string.Join(", ", karta.Koszt.Select(k => $"{k.Value} {k.Key}"))})");
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
        _output.WriteLine($"Testowana karta: {karta.Nazwa} (Koszt: {string.Join(", ", karta.Koszt.Select(k => $"{k.Value} {k.Key}"))})");
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
    public void Efekt_PunktyMilitarne()
    {
        Gracz gracz = new Gracz("GraczA");
        var pionKonfliktu = new PionKonfliktu(0);
        var zetonyPostepu = ZbiorZetonowPostepu.ZetonyPostepu;
        var wybraneZetony = zetonyPostepu.OrderBy(x => Guid.NewGuid()).Take(5).ToList();
        var strefy = ZbiorStref.Strefy.ToList();

        var plansza = new PlanszaKonfliktu(pionKonfliktu, wybraneZetony, strefy);
        var efekt = new Efekt(
            TypEfektu.PunktyMilitarne,
            wartość: 2
        );
        efekt.ZastosujEfekt(gracz, plansza);
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
        Assert.Contains(SymbolNaukowy.Koło, gracz.symboleNaukowe);
    }
    //[Fact]
    //public void Efekt_MonetyZaKarty_Cuda()
    //{
    //    Gracz gracz = new Gracz("TestowyGracz");
    //    Gracz przeciwnik = new Gracz("Przeciwnik");
    //    var kartaCudu = ZbiorKart.TaliaKartyCudow.First();
    //    gracz.DodajKarteCudu(kartaCudu);
    //    _output.WriteLine($"Testowana karta cudu: {kartaCudu.Nazwa}");

    //    // nazwa karty z efektem za cuda to "Arena"
    //    var talia = ZbiorKart.TaliaEpokiIII;
    //    var kartaZEfektem = talia.First(k => k.Nazwa == "Arena");
        
    //    _output.WriteLine($"Testowana karta: {kartaZEfektem.Nazwa}");
    //    gracz.ZbudujKarte(kartaZEfektem, przeciwnik);

    //    Assert.Equal(7 + 2, gracz.WypiszLiczbeSurowca(Surowiec.Monety)); // Początkowe 7 + 2 monety za każdą z 2 kart cudu
    //}
    //[Fact]
    //public void Efekt_MonetyZaKarty_Kolor()
    //{
    //    Gracz gracz = new Gracz("TestowyGracz");
    //    Gracz przeciwnik = new Gracz("Przeciwnik");
    //    var talia = ZbiorKart.TaliaEpokiII;

    //    var zolteKarty = talia.Where(k => k.KolorKarty == KolorKarty.Żółty).Take(3).ToList();
    //    foreach (var karta in zolteKarty)
    //    {
    //        gracz.ZbudujKarte(karta, przeciwnik);
    //        _output.WriteLine($"Zbudowana karta: {karta.Nazwa} (Kolor: {karta.KolorKarty})");
    //    }

    //    var kartaZEfektem = ZbiorKart.TaliaEpokiIII.First(k => k.Nazwa == "Latarnia Morska");
    //    _output.WriteLine($"Testowana karta: {kartaZEfektem.Nazwa} (Kolor: {kartaZEfektem.KolorKarty})");

    //    var monetyPrzed = gracz.WypiszLiczbeSurowca(Surowiec.Monety);
    //    gracz.ZbudujKarte(kartaZEfektem, przeciwnik);

    //    Assert.Equal(3, zolteKarty.Count);
    //    Assert.Equal(4, gracz.PobierzZbudowaneKarty().Count(k => k.KolorKarty == KolorKarty.Żółty));
    //    Assert.Equal(monetyPrzed + 4, gracz.WypiszLiczbeSurowca(Surowiec.Monety)); // Początkowe + 4 monety za każdą z 4 żółtych kart
    //}
}
