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
    public void Test_Zwyciestwo_Militarne()
    {
        var pionKonfliktu = new PionKonfliktu(0);
        var zetonyPostepu = ZbiorZetonowPostepu.ZetonyPostepu;
        var wybraneZetony = zetonyPostepu.OrderBy(x => Guid.NewGuid()).Take(5).ToList();
        var strefy = ZbiorStref.Strefy.ToList();

        var plansza = new PlanszaKonfliktu(pionKonfliktu, wybraneZetony, strefy);

        plansza.PrzesunPion(9, new Gracz("GraczA"));
        Assert.Equal(9, plansza.PionKonfliktu.PobierzPozycje());
        Assert.Equal("Zwyciestwo A", plansza.PobierzStrefeDlaPozycji(plansza.PionKonfliktu.PobierzPozycje()).Nazwa);
    }
    [Fact]
    public void Test_Zwyciestwo_Naukowe()
    {

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

