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
            SymbolNaukowy.MoŸdzierz,
            Surowiec.Kamieñ
        );

        Assert.Equal(TypEfektu.Surowiec, efekt.TypEfektu);
        Assert.Equal(2, efekt.Surowce[Surowiec.Drewno]);
        Assert.Equal(1, efekt.Surowce[Surowiec.Glina]);
        Assert.Equal(0, efekt.Wartosc);
        Assert.Equal("Testowy efekt", efekt.Tekst);
        Assert.Equal(SymbolNaukowy.MoŸdzierz, efekt.SymbolNaukowy);
        Assert.Equal(Surowiec.Kamieñ, efekt.Surowiec);
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
            wartoœæ: 5
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("5 Punkty Zwyciêstwa", wynik);
    }
    [Fact]
    public void Wypisz_Efekt_WyborSurowca()
    {
        var efekt = new Efekt(
            TypEfektu.WyborSurowca,
            new Dictionary<Surowiec, int> { { Surowiec.Szk³o, 1 }, { Surowiec.Papirus, 1 } }
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("Wybór - 1xSzk³o lub 1xPapirus", wynik);
    }
    [Fact]
    public void Wypisz_Efekt_Monety()
    {
        var efekt = new Efekt(
            TypEfektu.Monety,
            wartoœæ: 3
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("3 Monet", wynik);
    }
    [Fact]
    public void Wypisz_Efekt_SymbolNaukowy()
    {
        var efekt = new Efekt(
            TypEfektu.SymbolNaukowy,
            symbolNaukowy: SymbolNaukowy.Ko³o
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("SN - Ko³o", wynik);
    }
    [Fact]
    public void Wypisz_Efekt_MonetyZaKarty()
    {
        var efekt = new Efekt(
            TypEfektu.MonetyZaKarty,
            wartoœæ: 2,
            tekst: "Budowla"
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("2 monet za ka¿d¹ kartê Budowla", wynik);
    }
    [Fact]
    public void Wypisz_Efekt_PunktyZaKarty()
    {
        var efekt = new Efekt(
            TypEfektu.PunktyZaKarty,
            wartoœæ: 1,
            tekst: "Monety"
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("1 punktów za ka¿de 3 monety u gracza co ma ich wiêcej", wynik);
    }
    [Fact]
    public void Wypisz_Efekt_RozegrajTurePonownie()
    {
        var efekt = new Efekt(
            TypEfektu.RozegrajTurePonownie
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("Rozegraj turê ponownie", wynik);
    }
    [Fact]
    public void Wypisz_Efekt_PrzeciwnikOdkladaMonety()
    {
        var efekt = new Efekt(
            TypEfektu.PrzeciwnikOdkladaMonety,
            wartoœæ: 4
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("Przeciwnik odk³ada 4 monet", wynik);
    }
    [Fact]
    public void Wypisz_Efekt_DarmowaBudowlaZOdrzuconychKart()
    {
        var efekt = new Efekt(
            TypEfektu.DarmowaBudowlaZOdrzuconychKart
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("Mo¿esz zbudowaæ darmowo budowlê z odrzuconych kart", wynik);
    }
    [Fact]
    public void Wypisz_Efekt_OdlozKartePrzeciwnika()
    {
        var efekt = new Efekt(
            TypEfektu.OdlozKartePrzeciwnika,
            tekst: "Br¹zowy"
        );
        var wynik = efekt.Wypisz();
        Assert.Equal("Odk³adasz 1 Br¹zowy kartê przeciwnika", wynik);
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
        Assert.Equal("Efekt: Surowiec, Surowce: [Drewno, 2], Wartoœæ: 0, Tekst: Testowy efekt", wynik);
    }
}