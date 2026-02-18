using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit.Abstractions;

public class TestyGracza
{
    private readonly ITestOutputHelper _output;

    public TestyGracza(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Inicjalizacja_Gracza()
    {
        var gracz = new Gracz("TestowyGracz");
        Assert.Equal("TestowyGracz", gracz.Nazwa);
        Assert.Empty(gracz.KartyCudow);
        Assert.Empty(gracz.ZbudowaneKarty);
        foreach (var surowiec in Enum.GetValues<Surowiec>())
        {
            if (surowiec == Surowiec.Monety)
            {
                Assert.Equal(7, gracz.Surowce[surowiec]);
                continue;
            }
            Assert.Equal(0, gracz.Surowce[surowiec]);
        }
    }

    [Fact]
    public void DodajMonety_Do_Gracza()
    {
        var gracz = new Gracz("TestowyGracz");
        gracz.DodajMonety(5);
        Assert.Equal(12, gracz.Surowce[Surowiec.Monety]);
    }

    [Fact]
    public void DodajSurowiec_Do_Gracza()
    {
        var gracz = new Gracz("TestowyGracz");
        gracz.DodajSurowiec(Surowiec.Drewno, 3);
        Assert.Equal(3, gracz.Surowce[Surowiec.Drewno]);
    }

    [Fact]
    public void ZbudujKarte_Dla_Gracza()
    {
        var gracz = new Gracz("TestowyGracz");
        var przeciwnik = new Gracz("Przeciwnik");
        var karta = ZbiorKart.TaliaEpokiI.First();
        gracz.ZbudujKarte(karta, przeciwnik);
        Assert.Contains(karta, gracz.ZbudowaneKarty);
    }
    [Fact]
    public void DodajKarteCudu_Dla_Gracza()
    {
        var gracz = new Gracz("TestowyGracz");
        var kartaCudu = ZbiorKart.TaliaKartyCudow.First();
        gracz.DodajKarteCudu(kartaCudu);
        Assert.Contains(kartaCudu, gracz.KartyCudow);
        _output.WriteLine(gracz.WypiszKartyCudu());
        Assert.Equal("Piramida Cheopsa", gracz.WypiszKartyCudu());
        gracz.DodajKarteCudu(ZbiorKart.TaliaKartyCudow.Skip(1).First());
        Assert.Equal("Piramida Cheopsa, Wiszące Ogrody Semiramidy", gracz.WypiszKartyCudu());
    }
    [Fact]
    public void ZbudujKarteCudu_Wystarczajace_Surowce()
    {
        var gracz = new Gracz("TestowyGracz");
        var przeciwnik = new Gracz("Przeciwnik");
        // karta cudu to ta z nazwą "Piramida Cheopsa"
        var karta = ZbiorKart.TaliaEpokiI.First();
        var kartaCudu = ZbiorKart.TaliaKartyCudow.
            First(k => k.Nazwa == "Piramida Cheopsa");
        gracz.DodajKarteCudu(kartaCudu);
        gracz.DodajSurowiec(Surowiec.Drewno, 3);
        gracz.DodajSurowiec(Surowiec.Papirus, 1);
        gracz.ZbudujCud(karta, przeciwnik, kartaCudu);
        Assert.True(kartaCudu.CzyZagrana);
    }
    [Fact]
    public void TestEfektow()
    {
        var gracz = new Gracz("TestowyGracz");
        var karta = ZbiorKart.TaliaEpokiI.First(k => k.Nazwa == "Warsztat");
        gracz.ZbudujKarte(karta, new Gracz("Przeciwnik"));
        Assert.Contains(TypEfektu.SymbolNaukowy, gracz.PobierzEfekty().Select(e => e.TypEfektu));
        Assert.Contains(TypEfektu.PunktyZwyciestwa, gracz.PobierzEfekty().Select(e => e.TypEfektu));
    }
    [Fact]
    public void ZbudujKarteCudu_Brak_Niektorych_Surowcow()
    {
        var gracz = new Gracz("TestowyGracz");
        var przeciwnik = new Gracz("Przeciwnik");
        // karta cudu to ta z nazwą "Piramida Cheopsa"
        var karta = ZbiorKart.TaliaEpokiI.First();
        var kartaCudu = ZbiorKart.TaliaKartyCudow.
            First(k => k.Nazwa == "Piramida Cheopsa");
        gracz.DodajKarteCudu(kartaCudu);
        gracz.DodajSurowiec(Surowiec.Drewno, 2);
        gracz.DodajSurowiec(Surowiec.Papirus, 1);
        _output.WriteLine("Koszt karty:");
        _output.WriteLine(kartaCudu.WypiszKoszt());

        _output.WriteLine("Surowce gracza:");
        _output.WriteLine(
            string.Join(", ", gracz.Surowce.Select(s => $"{s.Key}:{s.Value}"))
        );
        gracz.ZbudujCud(karta, przeciwnik, kartaCudu);
        _output.WriteLine("Surowce gracza:");
        _output.WriteLine(
            string.Join(", ", gracz.Surowce.Select(s => $"{s.Key}:{s.Value}"))
        );
        Assert.True(kartaCudu.CzyZagrana);
    }
    [Fact]
    public void ZbudujKarteCudu_Nie_Mozna_Kupic_Karty()
    {
        var gracz = new Gracz("TestowyGracz");
        var przeciwnik = new Gracz("Przeciwnik");
        // karta cudu to ta z nazwą "Piramida Cheopsa"
        var karta = ZbiorKart.TaliaEpokiI.First();
        var kartaCudu = ZbiorKart.TaliaKartyCudow.
            First(k => k.Nazwa == "Piramida Cheopsa");
        _output.WriteLine("Koszt karty:");
        _output.WriteLine(kartaCudu.WypiszKoszt());

        _output.WriteLine("Surowce gracza:");
        _output.WriteLine(
            string.Join(", ", gracz.Surowce.Select(s => $"{s.Key}:{s.Value}"))
        );
        gracz.DodajKarteCudu(kartaCudu);
        var ex = Assert.Throws<InvalidOperationException>(() => gracz.ZbudujCud(karta, przeciwnik, kartaCudu));
        Assert.Equal("Nie można zbudować cudu.", ex.Message);
        
        _output.WriteLine("Surowce gracza:");
        _output.WriteLine(
            string.Join(", ", gracz.Surowce.Select(s => $"{s.Key}:{s.Value}"))
        );
    }
}