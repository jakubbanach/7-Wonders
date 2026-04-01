using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

public class TestyGra
{
    private readonly ITestOutputHelper _output;

    public TestyGra(ITestOutputHelper output)
    {
        _output = output;
    }
    [Fact]
    public void Test_InicjalizacjaGry()
    {
        var pionKonfliktu = new PionKonfliktu(0);
        var zetonyPostepu = ZbiorZetonowPostepu.ZetonyPostepu;
        var wybraneZetony = zetonyPostepu.OrderBy(x => Guid.NewGuid()).Take(5).ToList();
        var strefy = ZbiorStref.Strefy.ToList();
        var gracze = new[] { new Gracz("GraczA"), new Gracz("GraczB") };
        var planszaKonfliktu = new PlanszaKonfliktu(pionKonfliktu, wybraneZetony, strefy, gracze);
        var planszaEpoki = new PlanszaEpoki(Epoka.EpokaI);
        var stan = new StanGry();
        var gra = new Gra(gracze, planszaKonfliktu, planszaEpoki, stan);
        _output.WriteLine("Testowanie inicjalizacji gry...");
        Assert.NotNull(gra);
        Assert.Equal(2, gra.Gracze.Length);
        Assert.NotNull(gra.PlanszaKonfliktu);
        Assert.NotNull(gra.PlanszaEpoki);
        Assert.NotNull(gra.StanGry);
    }
    [Fact]
    public void Test_InicjalizacjaGry_StworzNowaGre()
    {
        var gra = Gra.StworzNowaGre();
        _output.WriteLine("Testowanie inicjalizacji gry...");
        Assert.NotNull(gra);
        Assert.Equal(2, gra.Gracze.Length);
        Assert.NotNull(gra.PlanszaKonfliktu);
        Assert.NotNull(gra.PlanszaEpoki);
        Assert.NotNull(gra.StanGry);
    }
    [Fact]
    public void Test_DostepneKarty()
    {
        var gra = Gra.StworzNowaGre();

        _output.WriteLine("Testowanie dostępnych kart na planszy");
        _output.WriteLine($"Aktualna epoka: {gra.Epoka}");

        var dostepneKarty = gra.DostepneKarty();
        _output.WriteLine("Dostępne karty:");
        foreach (var karta in dostepneKarty)
        {
            _output.WriteLine($"- {karta.Nazwa} (Epoka: {karta.Epoka})");
        }

        Assert.NotEmpty(dostepneKarty);
        Assert.Equal(6, dostepneKarty.Count);
    }
    [Fact]
    public void Test_DostepneRuchy()
    {
        var gra = Gra.StworzNowaGre();
        _output.WriteLine("Testowanie dostępnych ruchów...");
        var dostepneRuchy = gra.DostepneRuchy();
        _output.WriteLine("Dostępne ruchy:");
        foreach (var ruch in dostepneRuchy)
        {
            _output.WriteLine($"- {ruch.TypRuchu}, {ruch.KartaDoZagrania.Nazwa}, {ruch.KartaDoZagrania.CzyWidoczna}, {ruch.KartaDoZagrania.CzyOdrzucona}, {ruch.KartaDoZagrania.CzyZagrana}");
        }
        Assert.NotEmpty(dostepneRuchy);

        var ruchyZbudujKarte = dostepneRuchy.Where(p => p.TypRuchu == TypRuchu.ZbudujKarte).ToList();
        var ruchyOdrzucKarte = dostepneRuchy.Where(p => p.TypRuchu == TypRuchu.OdrzucKarte).ToList();
        var ruchyZbudujCud = dostepneRuchy.Where(p => p.TypRuchu == TypRuchu.ZbudujCud).ToList();

        Assert.Equal(6, ruchyZbudujKarte.Count); // zakladajac, ze kazda z 6 kart jest do zakupu
        Assert.Equal(6, ruchyOdrzucKarte.Count);
        Assert.Empty(ruchyZbudujCud); // zakladajac, ze na poczatku gry nie ma dostepnych materialow do budowy cudów

        gra.AktywnyGracz.DodajMonety(40); // Dodajemy monety, aby umożliwić budowę cudów
        dostepneRuchy = gra.DostepneRuchy();
        ruchyZbudujCud = dostepneRuchy.Where(p => p.TypRuchu == TypRuchu.ZbudujCud).ToList();
        Assert.Equal(24, ruchyZbudujCud.Count); // zakładając, że mamy 4 cuda i każdy z nich można zbudować na 6 kartach (łącznie 24 ruchy)
    }
    [Fact]
    public void Test_DostepneRuchy_BrakMonet()
    {
        var gra = Gra.StworzNowaGre();
        _output.WriteLine("Testowanie dostępnych ruchów...");
        gra.AktywnyGracz.DodajMonety(-7); // Usuwamy monety, aby sprawdzić, czy brak monet wpływa na dostępność ruchów
        var dostepneRuchy = gra.DostepneRuchy();
        _output.WriteLine("Dostępne ruchy:");
        foreach (var ruch in dostepneRuchy)
        {
            _output.WriteLine($"- {ruch.TypRuchu}, {ruch.KartaDoZagrania.Nazwa}, {ruch.KartaDoZagrania.CzyWidoczna}, {ruch.KartaDoZagrania.CzyOdrzucona}, {ruch.KartaDoZagrania.CzyZagrana}");
        }
        Assert.NotEmpty(dostepneRuchy);

        var ruchyZbudujKarte = dostepneRuchy.Where(p => p.TypRuchu == TypRuchu.ZbudujKarte).ToList();
        var ruchyOdrzucKarte = dostepneRuchy.Where(p => p.TypRuchu == TypRuchu.OdrzucKarte).ToList();
        var ruchyZbudujCud = dostepneRuchy.Where(p => p.TypRuchu == TypRuchu.ZbudujCud).ToList();

        Assert.Empty(ruchyZbudujKarte); // zakladajac, ze kazda z 6 kart jest do zakupu
        Assert.Equal(6, ruchyOdrzucKarte.Count);
        Assert.Empty(ruchyZbudujCud); // zakladajac, ze na poczatku gry nie ma dostepnych materialow do budowy cudów
    }

}
