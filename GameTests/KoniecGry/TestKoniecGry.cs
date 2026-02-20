using System;
using System.Collections.Generic;
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

        stanGry.CzyZwyciestwoNaukowe(gracze);

        Assert.True(stanGry.CzyZakonczona);
        Assert.Equal(gracze[0], stanGry.Zwyciezca);
        Assert.Equal(TypZwyciestwa.Naukowe, stanGry.TypZwyciestwa);
    }
    [Fact]
    public void Test_ZliczaniePunktow_Militaria()
    {
    }
    [Fact]
    public void Test_ZliczaniePunktow_Monety()
    {
    }
    [Fact]
    public void Test_ZliczaniePunktow_ZetonyPostepu()
    {
    }
    [Fact]
    public void Test_ZliczaniePunktow_Cuda()
    {
    }
    [Fact]
    public void Test_ZliczaniePunktow_Karty()
    {
    }
    [Fact]
    public void Test_ZliczaniePunktow_Efekty()
    {
    }
    [Fact]
    public void Test_Zwyciestwo_Punktowe()
    {
    }
}

