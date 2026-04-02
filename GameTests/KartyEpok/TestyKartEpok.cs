using Xunit.Abstractions;

public class TestyKartEpok
{
    private readonly ITestOutputHelper _output;

    public TestyKartEpok(ITestOutputHelper output)
    {
        _output = output;
    }
    [Fact]
    public void Test_KartaEpoki_ZbudujKarte()
    {
        var surowce = new Dictionary<Surowiec, int>
        {
            { Surowiec.Drewno, 2 },
            { Surowiec.Kamien, 1 }
        };
        var efekt = new Efekt(
            TypEfektu.Surowiec, 
            new Dictionary<Surowiec, int>
            {
                { Surowiec.Monety, 3 }
            }, 
            symbolNaukowy : SymbolNaukowy.Globus
        );
        var kartaEpoki = new Karta("Warsztat", Epoka.EpokaI, new Dictionary<Surowiec, int>
        {
            { Surowiec.Drewno, 2 },
            { Surowiec.Kamien, 1 }
        }, new List<Efekt> { efekt }, KolorKarty.Brazowy);
        // Symulacja budowy karty
        bool moznaZbudowac = true;
        foreach (var koszt in kartaEpoki.Koszt)
        {
            if (!surowce.ContainsKey(koszt.Key) || surowce[koszt.Key] < koszt.Value)
            {
                moznaZbudowac = false;
                break;
            }
        }
        Assert.True(moznaZbudowac);
    }

    [Fact]
    public void Test_DarmowaBudowa()
    {
        var kartaEpoki = new Karta("Œwi¹tynia", Epoka.EpokaII, new Dictionary<Surowiec, int>(), new List<Efekt>(), KolorKarty.Szary);
        // Symulacja darmowej budowy karty
        bool moznaZbudowac = kartaEpoki.Koszt.Count == 0;
        Assert.True(moznaZbudowac);
    }
    [Fact]
    public void Test_DarmowaBudowa_BialySymbol()
    {
        var kartaEpoki = ZbiorKart.TaliaEpokiIII.First(k => k.Nazwa == "Latarnia Morska");
        // Symulacja darmowej budowy karty z bia³ym symbolem
        var gracz = new Gracz("TestowyGracz");
        var przeciwnik = new Gracz("Przeciwnik");
        gracz.BialeSymbole.Add("Wazon");
        gracz.DodajMonety(-7); // Usuniêcie monet, aby karta by³a darmowa
        gracz.ZbudujKarte(kartaEpoki, przeciwnik);
        _output.WriteLine($"Karta: {kartaEpoki.Nazwa}, Koszt: {string.Join(", ", kartaEpoki.Koszt.Select(k => $"{k.Key}: {k.Value}"))}");
        _output.WriteLine($"Czy karta ma bialy symbol jako zakup? {kartaEpoki.DarmowaBudowa}");
        _output.WriteLine($"Bia³y symbol gracza: {string.Join(", ", gracz.BialeSymbole)}");
        Assert.Contains(kartaEpoki, gracz.PobierzZbudowaneKarty());
    }

    [Fact]
    public void Test_DarmowaBudowa_BialySymbol_Efekt_MonetyZaBudoweZBialymSymbolem()
    {
        var kartaEpoki = ZbiorKart.TaliaEpokiIII.First(k => k.Nazwa == "Uniwersytet");
        // Symulacja darmowej budowy karty z bia³ym symbolem
        var gracz = new Gracz("TestowyGracz");
        var przeciwnik = new Gracz("Przeciwnik");
        gracz.BialeSymbole.Add("Harfa");
        gracz.DodajMonety(-7); // Usuniêcie monet, aby karta by³a darmowa

        var efekt = new Efekt(
            TypEfektu.MonetyZaBudoweZBialymSymbolem, 
            wartosc: 4
        );
        gracz.DodajEfekt(efekt);

        Assert.Equal(0, gracz.Surowce[Surowiec.Monety]);

        gracz.ZbudujKarte(kartaEpoki, przeciwnik);
        Assert.Contains(kartaEpoki, gracz.PobierzZbudowaneKarty());
        Assert.Equal(4, gracz.Surowce[Surowiec.Monety]);
    }

    [Fact]
    public void Test_Koszt_SurowcePrzeciwnika()
    {
        var kartaEpoki = new Karta("TEEEEST", Epoka.EpokaI, new Dictionary<Surowiec, int>
        {
            { Surowiec.Monety, 2 },
            { Surowiec.Szklo, 1 },
            { Surowiec.Papirus, 1 }
        }, new List<Efekt>(), KolorKarty.Brazowy);
        var gracz = new Gracz("TestowyGracz");
        var przeciwnik = new Gracz("Przeciwnik");
        var koszt = kartaEpoki.ObliczKoszt(gracz, przeciwnik);
        Assert.Equal(6, koszt);
        
        przeciwnik.DodajSurowiec(Surowiec.Szklo, 1);
        koszt = kartaEpoki.ObliczKoszt(gracz, przeciwnik);
        Assert.Equal(7, koszt);
    }
    [Fact]
    public void Test_Koszt_SurowcePrzeciwnika2()
    {
        var kartaEpoki = ZbiorKart.TaliaEpokiIII.First(k => k.Nazwa == "Fortyfikacje");
        var gracz = new Gracz("Bartek");
        var przeciwnik = new Gracz("Ania");
        var koszt = kartaEpoki.ObliczKoszt(gracz, przeciwnik);
        Assert.Equal(8, koszt);
        
        gracz.DodajSurowiec(Surowiec.Kamien, 2);
        przeciwnik.DodajSurowiec(Surowiec.Glina, 1);
        koszt = kartaEpoki.ObliczKoszt(gracz, przeciwnik);
        Assert.Equal(5, koszt);
    }
    [Fact]
    public void Test_Koszt_SurowcePrzeciwnika3()
    {
        var kartaEpoki = ZbiorKart.TaliaEpokiII.First(k => k.Nazwa == "Akwedukt");
        var gracz = new Gracz("Bartek");
        var przeciwnik = new Gracz("Ania");
        var koszt = kartaEpoki.ObliczKoszt(przeciwnik, gracz);
        Assert.Equal(6, koszt);
        
        gracz.DodajSurowiec(Surowiec.Kamien, 2);
        przeciwnik.DodajSurowiec(Surowiec.Glina, 1);
        koszt = kartaEpoki.ObliczKoszt(przeciwnik, gracz);
        Assert.Equal(12, koszt);
    }
    [Fact]
    public void Test_Koszt_ZmianaCenySurowca()
    {
        var kartaEpoki = ZbiorKart.TaliaEpokiII.First(k => k.Nazwa == "Akwedukt");
        var gracz = new Gracz("Bartek");
        var przeciwnik = new Gracz("Ania");
        var koszt = kartaEpoki.ObliczKoszt(przeciwnik, gracz);
        Assert.Equal(6, koszt);
        
        gracz.DodajSurowiec(Surowiec.Kamien, 2);
        przeciwnik.DodajSurowiec(Surowiec.Glina, 1);
        koszt = kartaEpoki.ObliczKoszt(przeciwnik, gracz);
        Assert.Equal(12, koszt);
    }
    [Fact]
    public void Test_BrakMozliwosciZakupu_PrzeciwnikMaSurowce()
    {
        var kartaEpoki = new Karta("TEEEEST", Epoka.EpokaI, new Dictionary<Surowiec, int>
        {
            { Surowiec.Monety, 2 },
            { Surowiec.Szklo, 1 },
            { Surowiec.Papirus, 1 }
        }, new List<Efekt>(), KolorKarty.Brazowy);
        var gracz = new Gracz("TestowyGracz");
        var przeciwnik = new Gracz("Przeciwnik");
        przeciwnik.DodajSurowiec(Surowiec.Szklo, 1);
        gracz.DodajMonety(-1); // Usuniêcie monet, aby karta by³a darmowa
        _output.WriteLine($"Karta: {kartaEpoki.Nazwa}, Koszt: {string.Join(", ", kartaEpoki.Koszt.Select(k => $"{k.Key}: {k.Value}"))}");

        var ex = Assert.Throws<InvalidOperationException>(() => gracz.ZbudujKarte(kartaEpoki,przeciwnik));

        // Opcjonalnie sprawdzenie komunikatu
        Assert.Equal("Nie mozna zbudowac tej karty.", ex.Message);
    }

}