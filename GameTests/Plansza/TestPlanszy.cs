using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
public class TestPlanszy
{
    private readonly ITestOutputHelper _output;

    public TestPlanszy(ITestOutputHelper output)
    {
        _output = output;
    }
    [Fact]
    public void Test_InicjalizacjaPlanszy()
    {
        var pionKonfliktu = new PionKonfliktu(0);
        var zetonyPostepu = ZbiorZetonowPostepu.ZetonyPostepu;
        // losuj 5 zetonow postepu
        var wybraneZetony = zetonyPostepu.OrderBy(x => Guid.NewGuid()).Take(5).ToList();
        var strefy = ZbiorStref.Strefy.ToList();
        var gracz = new Gracz("Gracz");
        var przeciwnik = new Gracz("Przeciwnik");

        var plansza = new PlanszaKonfliktu(pionKonfliktu, wybraneZetony, strefy, gracze: new[] { gracz, przeciwnik });

        _output.WriteLine($"Pion konfliktu na pozycji: {plansza.PionKonfliktu.PobierzPozycje()}");
        _output.WriteLine($"Zetony postepu: {string.Join(", ", plansza.ZetonyPostepu.Select(z => z.Nazwa))}");
        _output.WriteLine($"Strefy: {string.Join(", ", plansza.Strefy.Select(s => s.Nazwa))}");

        Assert.Equal(pionKonfliktu, plansza.PionKonfliktu);
        Assert.Equal(5, plansza.ZetonyPostepu.Count);
        var strefaStartowa = plansza.PobierzStrefeDlaPozycji(plansza.PionKonfliktu.PobierzPozycje());
        Assert.Equal("Startowa", strefaStartowa.Nazwa);
    }
    [Fact]
    public void Test_PrzesunPionKonfliktu()
    {
        var pionKonfliktu = new PionKonfliktu(0);
        var zetonyPostepu = ZbiorZetonowPostepu.ZetonyPostepu;
        var wybraneZetony = zetonyPostepu.OrderBy(x => Guid.NewGuid()).Take(5).ToList();
        var strefy = ZbiorStref.Strefy.ToList();
        var gracz = new Gracz("Gracz");
        var przeciwnik = new Gracz("Przeciwnik");

        var plansza = new PlanszaKonfliktu(pionKonfliktu, wybraneZetony, strefy, gracze: new[] { gracz, przeciwnik });
        
        plansza.PrzesunPion(1, gracz);
        Assert.Equal(1, plansza.PionKonfliktu.PobierzPozycje());
        Assert.Equal("Strefa 1 dla A", plansza.PobierzStrefeDlaPozycji(plansza.PionKonfliktu.PobierzPozycje()).Nazwa);
    }
    [Fact]
    public void Test_Zwyciestwo()
    {
        var pionKonfliktu = new PionKonfliktu(0);
        var zetonyPostepu = ZbiorZetonowPostepu.ZetonyPostepu;
        var wybraneZetony = zetonyPostepu.OrderBy(x => Guid.NewGuid()).Take(5).ToList();
        var strefy = ZbiorStref.Strefy.ToList();
        var gracz = new Gracz("Gracz");
        var przeciwnik = new Gracz("Przeciwnik");

        var plansza = new PlanszaKonfliktu(pionKonfliktu, wybraneZetony, strefy, gracze: new[] { gracz, przeciwnik });

        plansza.PrzesunPion(9, gracz);
        Assert.Equal(9, plansza.PionKonfliktu.PobierzPozycje());
        Assert.Equal("Zwyciestwo A", plansza.PobierzStrefeDlaPozycji(plansza.PionKonfliktu.PobierzPozycje()).Nazwa);
    }
    [Fact]
    public void Test_Cofniecie()
    {
        var pionKonfliktu = new PionKonfliktu(0);
        var zetonyPostepu = ZbiorZetonowPostepu.ZetonyPostepu;
        var wybraneZetony = zetonyPostepu.OrderBy(x => Guid.NewGuid()).Take(5).ToList();
        var strefy = ZbiorStref.Strefy.ToList();
        var gracz = new Gracz("Gracz");
        var przeciwnik = new Gracz("Przeciwnik");

        var plansza = new PlanszaKonfliktu(pionKonfliktu, wybraneZetony, strefy, gracze: new[] { gracz, przeciwnik });

        plansza.PrzesunPion(3, gracz);
        plansza.PrzesunPion(4, przeciwnik);
        Assert.Equal(-1, plansza.PionKonfliktu.PobierzPozycje());
        Assert.Equal("Strefa 1 dla B", plansza.PobierzStrefeDlaPozycji(plansza.PionKonfliktu.PobierzPozycje()).Nazwa);
    }
}

