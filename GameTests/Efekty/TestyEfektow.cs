public class TestyEfektow
{
    [Fact]
    public void Inicjalizacja_Efektu()
    {
        var efekt = new Efekt(
            TypEfektu.Surowiec,
            new Dictionary<Surowiec, int> { { Surowiec.Drewno, 2 }, { Surowiec.Glina, 1 } },
            0,
            "Testowy efekt",
            SymbolNaukowy.Mozdzierz,
            Surowiec.Kamien
        );

        Assert.Equal(TypEfektu.Surowiec, efekt.TypEfektu);
        Assert.Equal(2, efekt.Surowce[Surowiec.Drewno]);
        Assert.Equal(1, efekt.Surowce[Surowiec.Glina]);
        Assert.Equal(0, efekt.Wartosc);
        Assert.Equal("Testowy efekt", efekt.Tekst);
        Assert.Equal(SymbolNaukowy.Mozdzierz, efekt.SymbolNaukowy);
        Assert.Equal(Surowiec.Kamien, efekt.Surowiec);
    }
    [Fact]
    public void Wypisz_Efekt_Surowiec()
    {
        var efekt = new Efekt(
            TypEfektu.Surowiec,
            new Dictionary<Surowiec, int> { { Surowiec.Drewno, 2 }, { Surowiec.Glina, 1 } }
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("2xDrewno + 1xGlina", wynik);
    }
    [Fact]
    public void Wypisz_Efekt_PunktyZwyciestwa()
    {
        var efekt = new Efekt(
            TypEfektu.PunktyZwyciestwa,
            wartosc: 5
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("5 Punkty Zwyciestwa", wynik);
    }
    [Fact]
    public void Wypisz_Efekt_WyborSurowca()
    {
        var efekt = new Efekt(
            TypEfektu.WyborSurowca,
            new Dictionary<Surowiec, int> { { Surowiec.Szklo, 1 }, { Surowiec.Papirus, 1 } }
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("Wybor - 1xSzklo lub 1xPapirus", wynik);
    }
    [Fact]
    public void Wypisz_Efekt_Monety()
    {
        var efekt = new Efekt(
            TypEfektu.Monety,
            wartosc: 3
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("3 Monet", wynik);
    }
    [Fact]
    public void Wypisz_Efekt_SymbolNaukowy()
    {
        var efekt = new Efekt(
            TypEfektu.SymbolNaukowy,
            symbolNaukowy: SymbolNaukowy.Kolo
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("SN - Kolo", wynik);
    }
    [Fact]
    public void Wypisz_Efekt_MonetyZaKarty()
    {
        var efekt = new Efekt(
            TypEfektu.MonetyZaKarty,
            wartosc: 2,
            tekst: "Budowla"
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("2 monet za kazda karte Budowla", wynik);
    }
    [Fact]
    public void Wypisz_Efekt_PunktyZaKarty()
    {
        var efekt = new Efekt(
            TypEfektu.PunktyZaKarty,
            wartosc: 1,
            tekst: "Monety"
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("1 punktow za kazde 3 monety u gracza co ma ich wiecej", wynik);
    }
    [Fact]
    public void Wypisz_Efekt_RozegrajTurePonownie()
    {
        var efekt = new Efekt(
            TypEfektu.RozegrajTurePonownie
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("Rozegraj ture ponownie", wynik);
    }
    [Fact]
    public void Wypisz_Efekt_PrzeciwnikOdkladaMonety()
    {
        var efekt = new Efekt(
            TypEfektu.PrzeciwnikOdkladaMonety,
            wartosc: 4
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("Przeciwnik odklada 4 monet", wynik);
    }
    [Fact]
    public void Wypisz_Efekt_DarmowaBudowlaZOdrzuconychKart()
    {
        var efekt = new Efekt(
            TypEfektu.DarmowaBudowlaZOdrzuconychKart
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("Mozesz zbudowac darmowo budowle z odrzuconych kart", wynik);
    }
    [Fact]
    public void Wypisz_Efekt_OdlozKartePrzeciwnika()
    {
        var efekt = new Efekt(
            TypEfektu.OdlozKartePrzeciwnika,
            tekst: "Brazowy"
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("Odkladasz 1 Brazowy karte przeciwnika", wynik);
    }
    [Fact]
    public void ToString_Efekt()
    {
        var efekt = new Efekt(
            TypEfektu.Surowiec,
            new Dictionary<Surowiec, int> { { Surowiec.Drewno, 2 } },
            0,
            "Testowy efekt"
        );
        var wynik = efekt.ToString();
        Assert.Equal("Efekt: Surowiec, Surowce: [Drewno, 2], Wartosc: 0, Tekst: Testowy efekt", wynik);
    }
}