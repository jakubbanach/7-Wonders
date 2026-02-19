using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
public  class TestyPlanszyEpok
{
    private readonly ITestOutputHelper _output;

    public TestyPlanszyEpok(ITestOutputHelper output)
    {
        _output = output;
    }
    [Fact]  
    public void Test_InicjalizacjaPlanszyEpokiI()
    {
        var taliaKart = ZbiorKart.TaliaEpokiI
            .OrderBy(x => System.Guid.NewGuid())
            .Take(20)
            .ToList();
        var planszaEpokiI = ZbiorPlanszEpok.Utworz(Epoka.EpokaI, taliaKart);
        _output.WriteLine("Dostępne karty na planszy Epoki I:");
        foreach (var pole in planszaEpokiI.DostepneKarty)
        {
            _output.WriteLine(pole.Karta!.Nazwa);
        }
        Assert.Equal(2, planszaEpokiI.DostepneKarty.Count());
        foreach (var pole in planszaEpokiI.WidoczneKarty)
        {
            _output.WriteLine($"Karta: {pole.Karta!.Nazwa}, Czy dostępna: {pole.CzyDostepna}");
        }
    }
    [Fact]
    public void Test_InicjalizacjaPlanszyEpokiII()
    {
        var taliaKart = ZbiorKart.TaliaEpokiII
            .OrderBy(x => System.Guid.NewGuid())
            .Take(20)
            .ToList();
        var planszaEpokiII = ZbiorPlanszEpok.Utworz(Epoka.EpokaII, taliaKart);
        _output.WriteLine("Dostępne karty na planszy Epoki II:");
        foreach (var pole in planszaEpokiII.DostepneKarty)
        {
            _output.WriteLine(pole.Karta!.Nazwa);
        }
        Assert.Equal(6, planszaEpokiII.DostepneKarty.Count());
        foreach (var pole in planszaEpokiII.WidoczneKarty)
        {
            _output.WriteLine($"Karta: {pole.Karta!.Nazwa}, Czy dostępna: {pole.CzyDostepna}");
        }
    }
    [Fact]
    public void Test_BudowaKartyZPlanszyEpokiI()
    {
        var taliaKart = ZbiorKart.TaliaEpokiI
            .OrderBy(x => System.Guid.NewGuid())
            .Take(20)
            .ToList();
        foreach (var kartaEpoki in taliaKart)
        {
            kartaEpoki.OznaczJakoNiezagrana();
        }
        var planszaEpokiI = ZbiorPlanszEpok.Utworz(Epoka.EpokaI, taliaKart);
        var gracz = new Gracz("TestowyGracz");
        var przeciwnik = new Gracz("Przeciwnik");
        var dostepneKarty = planszaEpokiI.DostepneKarty.ToList();
        var kartaDoZbudowania = dostepneKarty[0];
        var karta = kartaDoZbudowania.Karta!;

        _output.WriteLine("Dostępne karty na planszy Epoki I:");
        foreach (var pole in planszaEpokiI.DostepneKarty)
        {
            _output.WriteLine(pole.Karta!.Nazwa);
        }

        planszaEpokiI.UsunPole(kartaDoZbudowania);
        gracz.ZbudujKarte(karta, przeciwnik);
        
        Assert.Contains(karta, gracz.ZbudowaneKarty);
        _output.WriteLine($"Gracz zbudował kartę: {karta.Nazwa}");
        
        Assert.DoesNotContain(kartaDoZbudowania, planszaEpokiI.DostepneKarty);
        
        _output.WriteLine("Dostępne karty na planszy Epoki I:");
        foreach (var pole in planszaEpokiI.DostepneKarty.ToList())
        {
            _output.WriteLine(pole.Karta!.Nazwa);
        }
        Assert.Equal(2, planszaEpokiI.DostepneKarty.ToList().Count());
    }

    //[Fact]
    //public void Test_PrzejscieCalejEpoki()
    //{
    //    var rng = new Random(42);
    //    var przetasowane = ZbiorPlanszEpok.Tasuj((List<Karta>)ZbiorKart.TaliaEpokiI, rng);

    //    var taliaKartEpokiI = przetasowane.Take(20).ToList();
    //    var kartyOdrzucone = przetasowane.Skip(20).Take(3).ToList();

    //    var planszaEpokiI = ZbiorPlanszEpok.Utworz(Epoka.EpokaI, taliaKartEpokiI);
    //    var gracz = new Gracz("TestowyGracz");
    //    var przeciwnik = new Gracz("Przeciwnik");
    //    while (planszaEpokiI.DostepneKarty.Any())
    //    {
    //        var dostepneKarty = planszaEpokiI.DostepneKarty.ToList();
    //        _output.WriteLine($"Dostępne karty na planszy Epoki I: {dostepneKarty.Count}");
    //        var kartaDoZbudowania = dostepneKarty[0];
    //        var karta = kartaDoZbudowania.Karta!;
    //        planszaEpokiI.UsunPole(kartaDoZbudowania);
    //        gracz.ZbudujKarte(karta, przeciwnik);
    //        _output.WriteLine($"Gracz zbudował kartę: {karta.Nazwa}");
    //        _output.WriteLine(ZbiorPlanszEpok.PlanszaDoStringa(planszaEpokiI));
    //    }
    //    Assert.Equal(20, gracz.ZbudowaneKarty.Count);
    //    Assert.Empty(planszaEpokiI.DostepneKarty);
    //    _output.WriteLine($"Gracz zbudował wszystkie karty Epoki I: {gracz.ZbudowaneKarty.Count} kart.");
    //    _output.WriteLine("Karty odrzucone:");
    //    foreach (var karta in kartyOdrzucone)
    //    {
    //        _output.WriteLine(karta.Nazwa);
    //    }
    //}
    //[Fact]
    //public void Test_PrzejscieCalejEpoki2Graczy()
    //{
    //    var rng = new Random(42);
    //    var przetasowane = ZbiorPlanszEpok.Tasuj((List<Karta>)ZbiorKart.TaliaEpokiIII, rng);
    //    var taliaKartEpokiI = przetasowane.Take(20).ToList();
    //    var planszaEpokiI = ZbiorPlanszEpok.Utworz(Epoka.EpokaIII, taliaKartEpokiI);
    //    var graczA = new Gracz("GraczA");
    //    var graczB = new Gracz("GraczB");
    //    var gracze = new List<Gracz> { graczA, graczB };
    //    int aktualnyGraczIndex = 0;

    //    while (planszaEpokiI.DostepneKarty.Any())
    //    {
    //        var aktualnyGracz = gracze[aktualnyGraczIndex];
    //        var przeciwnik = gracze[(aktualnyGraczIndex + 1) % gracze.Count];
    //        var dostepneKarty = planszaEpokiI.DostepneKarty.ToList();
    //        //_output.WriteLine($"Dostępne karty na planszy Epoki II: {dostepneKarty.Count}");

    //        var kartaDoZbudowania = dostepneKarty[0];
    //        var karta = kartaDoZbudowania.Karta!;

    //        planszaEpokiI.UsunPole(kartaDoZbudowania);
    //        aktualnyGracz.ZbudujKarte(karta, przeciwnik);

    //        _output.WriteLine($"{aktualnyGracz.Nazwa} zbudował kartę: {karta.Nazwa}");
    //        _output.WriteLine(ZbiorPlanszEpok.PlanszaDoStringa(planszaEpokiI));
    //        aktualnyGraczIndex = (aktualnyGraczIndex + 1) % gracze.Count;
    //    }
    //    Assert.Equal(10, graczA.ZbudowaneKarty.Count);
    //    Assert.Equal(10, graczB.ZbudowaneKarty.Count);
    //    Assert.Empty(planszaEpokiI.DostepneKarty);
    //    _output.WriteLine($"GraczA zbudował wszystkie swoje karty Epoki II: {graczA.ZbudowaneKarty.Count} kart.");
    //    _output.WriteLine($"GraczB zbudował wszystkie swoje karty Epoki II: {graczB.ZbudowaneKarty.Count} kart.");
    //}

    //[Fact]
    //public void Test_PrzejscieCalejEpokiLosowo()
    //{
    //    var rng = new Random(40);
    //    var przetasowane = ZbiorPlanszEpok.Tasuj((List<Karta>)ZbiorKart.TaliaEpokiII, rng);
    //    var taliaKartEpokiI = przetasowane.Take(20).ToList();
    //    var planszaEpokiI = ZbiorPlanszEpok.Utworz(Epoka.EpokaII, taliaKartEpokiI);

    //    var gracz = new Gracz("TestowyGracz");
    //    var przeciwnik = new Gracz("Przeciwnik");
    //    while (planszaEpokiI.DostepneKarty.Any())
    //    {
    //        var dostepneKarty = planszaEpokiI.DostepneKarty.ToList();
    //        _output.WriteLine($"Dostępne karty na planszy: {dostepneKarty.Count}");
    //        _output.WriteLine(new string('*', 30));
    //        var indeksKarty = rng.Next(dostepneKarty.Count);
    //        var kartaDoZbudowania = dostepneKarty[indeksKarty];
    //        var karta = kartaDoZbudowania.Karta!;
    //        planszaEpokiI.UsunPole(kartaDoZbudowania);
    //        gracz.ZbudujKarte(karta, przeciwnik);
    //        _output.WriteLine($"Gracz zbudował kartę: {karta.Nazwa}");
    //        _output.WriteLine(ZbiorPlanszEpok.PlanszaDoStringa(planszaEpokiI));
    //    }
    //    Assert.Equal(20, gracz.ZbudowaneKarty.Count);
    //    Assert.Empty(planszaEpokiI.DostepneKarty);
    //    _output.WriteLine($"Gracz zbudował wszystkie karty Epoki: {gracz.ZbudowaneKarty.Count} kart.");
    //}

    [Fact]
    public void Test_WypisaniePlanszyEpokiI()
    {
        var taliaKart = ZbiorKart.TaliaEpokiI
            .OrderBy(x => System.Guid.NewGuid())
            .Take(20)
            .ToList();
        var planszaEpokiI = ZbiorPlanszEpok.Utworz(Epoka.EpokaI, taliaKart);
        _output.WriteLine("Plansza Epoki I:");
        _output.WriteLine(ZbiorPlanszEpok.PlanszaDoStringa(planszaEpokiI));
    }
}
