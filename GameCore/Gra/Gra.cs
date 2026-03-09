using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Gra
{
    private readonly Gracz[] gracze;
    private readonly PlanszaKonfliktu planszaKonfliktu;
    private readonly PlanszaEpoki planszaEpoki;
    private readonly StanGry stanGry;

    private int idAktywnegoGracza = 0;

    public Gra(
        Gracz[] gracze,
        PlanszaKonfliktu planszaKonfliktu,
        PlanszaEpoki planszaEpoki,
        StanGry stanGry)
    {
        this.gracze = gracze;
        this.planszaKonfliktu = planszaKonfliktu;
        this.planszaEpoki = planszaEpoki;
        this.stanGry = stanGry;
    }

    public Gracz[] Gracze => gracze;
    public PlanszaKonfliktu PlanszaKonfliktu => planszaKonfliktu;
    public PlanszaEpoki PlanszaEpoki => planszaEpoki;
    public StanGry StanGry => stanGry;
    public Gracz AktywnyGracz => gracze[idAktywnegoGracza];

    public Gracz Przeciwnik =>
        gracze.First(g => g != AktywnyGracz);

    public static Gra StworzNowaGre(string nazwa1="Gracz 1", string nazwa2="Gracz 2")
    {
        var gracze = InicjalizacjaGraczy(nazwa1, nazwa2);
        var planszaKonfliktu = InicjalizacjaPlanszy(gracze);
        var planszaEpoki = UtworzPlanszeEpoki(Epoka.EpokaI);
        var cuda = InicjalizacjaKartCudow();

        return new Gra(
            gracze,
            planszaKonfliktu,
            planszaEpoki,
            new StanGry());
    }

    public void WykonajRuch(PoleKarty poleKarty, TypRuchu typRuchu)
    {
        Ruch ruch = new Ruch(AktywnyGracz, Przeciwnik, poleKarty.Karta, typRuchu);
        ruch.Wykonaj(planszaKonfliktu);
        planszaEpoki.UsunPole(poleKarty);
        if (CzyKoniecGry())
        {
            Console.WriteLine("Gra zakończona!");
            return;
        }
        // czy tutaj nie dać logiki wykonania ponownego ruchu - efekt RozegrajTurePonownie
        ZmienTure(); 
    }

    private void ZmienTure()
    {
        idAktywnegoGracza = (idAktywnegoGracza + 1) % gracze.Length;
    }

    public IReadOnlyList<Karta> DostepneKarty()
    {
        // Logika zwracająca dostępne karty dla aktywnego gracza
        return new List<Karta>();
    }

    public IReadOnlyList<TypRuchu> DostepneRuchy(Karta karta)
    {
        // Logika zwracająca dostępne ruchy dla danej karty
        return new List<TypRuchu>();
    }

    public bool CzyKoniecEpoki()
    {
        if (planszaEpoki.DostepneKarty.Any())
            return false;
        // zmiana epoki
        return true;
    }

    public bool CzyKoniecGry()
    {
        stanGry.CzyZwyciestwoMilitarne(gracze, planszaKonfliktu.PionKonfliktu.PobierzPozycje());
        stanGry.CzyZwyciestwoNaukowe(gracze);
        if (stanGry.CzyZakonczona)
            return true;
        return false;
    }

    public Epoka Epoka => planszaEpoki.Epoka;

    public int PozycjaKonfliktu => planszaKonfliktu.PionKonfliktu.PobierzPozycje();

    public IReadOnlyList<ZetonPostepu> DostepneZetonyPostepu()
    {
        return planszaKonfliktu.ZetonyPostepu;
    }

    private static Gracz[] InicjalizacjaGraczy(string nazwa1 = "Gracz 1", string nazwa2 = "Gracz 2")
    {
        return new Gracz[]
        {
            new Gracz(nazwa1),
            new Gracz(nazwa2)
        };
    }
    private static PlanszaKonfliktu InicjalizacjaPlanszy(Gracz[] gracze)
    {
        var pionKonfliktu = new PionKonfliktu(0);
        var zetonyPostepu = ZbiorZetonowPostepu.ZetonyPostepu
            .OrderBy(x => Guid.NewGuid())
            .Take(5)
            .ToList();
        var strefy = ZbiorStref.Strefy.ToList();

        return new PlanszaKonfliktu(pionKonfliktu, zetonyPostepu, strefy, gracze);
    }
    private static PlanszaEpoki UtworzPlanszeEpoki(Epoka epoka)
    {
        var taliaKart = epoka switch
        {
            Epoka.EpokaI => ZbiorKart.TaliaEpokiI,
            Epoka.EpokaII => ZbiorKart.TaliaEpokiII,
            Epoka.EpokaIII => ZbiorKart.TaliaEpokiIII,
            _ => throw new ArgumentException("Nieznana epoka")
        };
        return ZbiorPlanszEpok.Utworz(epoka, taliaKart.ToList());
    }
    private static List<KartaCudu> InicjalizacjaKartCudow()
    {
        return ZbiorKart.TaliaKartyCudow
            .OrderBy(x => Guid.NewGuid())
            .Take(8) // 4 w pierwszej fazie wyboru, 4 w drugiej fazie wyboru
            .ToList();
    }
}