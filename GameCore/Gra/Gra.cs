using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Gra
{
    private readonly Gracz[] gracze;
    private readonly PlanszaKonfliktu planszaKonfliktu;
    private PlanszaEpoki planszaEpoki;
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

    private Gra(Gra gra)
    {
        idAktywnegoGracza = gra.idAktywnegoGracza;
        gracze = gra.gracze
            .Select(g => g.Clone())
            .ToArray();
        planszaKonfliktu = gra.planszaKonfliktu.Clone();
        planszaEpoki = gra.planszaEpoki.Clone();
        stanGry = gra.stanGry.Clone();
    }

    public Gra Clone()
    {
        return new Gra(this);
    }

    public static Gra StworzNowaGre(string nazwa1 = "Gracz 1", string nazwa2 = "Gracz 2", IRandom random = null)
    {
        var gracze = InicjalizacjaGraczy(nazwa1, nazwa2);
        var planszaKonfliktu = InicjalizacjaPlanszy(gracze, random);
        var planszaEpoki = UtworzPlanszeEpoki(Epoka.EpokaI, random);
        var cuda = InicjalizacjaKartCudow(random);
        PrzydzielenieCudow(gracze, cuda);

        return new Gra(
            gracze,
            planszaKonfliktu,
            planszaEpoki,
            new StanGry());
    }

    public void WykonajRuch(Ruch ruchNew, IRandom random)
    {
        var karta = ZnajdzDostepnaKarte(ruchNew.KartaDoZagrania.Nazwa);

        //Console.WriteLine($"Dostepne karty: {string.Join(", ", DostepneKarty().Select(k => k.Nazwa))}");
        //Console.WriteLine($"Wybrana karta: {karta.Nazwa}");

        var kartaCudu = ruchNew.KartaCudu == null ? null : ZnajdzKarteCudu(ruchNew.KartaCudu.Nazwa);

        Ruch ruch = new Ruch(AktywnyGracz, Przeciwnik, karta, ruchNew.TypRuchu, kartaCudu);
        //Console.WriteLine($"Ruch wykonuje Aktywny gracz: {AktywnyGracz.Nazwa}, Przeciwnik: {Przeciwnik.Nazwa}");
        ruch.Wykonaj(planszaKonfliktu);

        var poleKarty = planszaEpoki.ZnajdzPole(karta);
        if (poleKarty == null)
        {
            Console.WriteLine("Nie mozna znalezc karty na planszy epoki!");
            return;
        }
        planszaEpoki.UsunPole(poleKarty);
        if (CzyKoniecGry())
        {
            Console.WriteLine("Gra zakonczona!");
            return;
        }
        if (CzyKoniecEpoki())
        {
            //Console.WriteLine("Koniec epoki! Przechodzimy do kolejnej epoki.");
            if (planszaEpoki.Epoka == Epoka.EpokaIII)
            {
                //Console.WriteLine("Koniec gry! Przechodzimy do podsumowania wynikow.");
                // dopisac logike podsumowania wynikow
                ZakonczGre();
                return;
            }
            planszaEpoki = UtworzPlanszeEpoki(planszaEpoki.Epoka + 1, random);

            // dla uproszczenia na razie gracz ktory konczyl epoke, nie bedzie rozpoczynal kolejnej epoki, ale mozna to zmienic w przyszlosci
            ZmienTure();
            return;
        }

        // sprawdzamy czy efekt karty cudu pozwala na rozegranie kolejnej tury
        if (ruch.TypRuchu == TypRuchu.ZbudujCud && ruch.KartaCudu != null)
        {
            // sprawdzamy czy lista efektow karty cudu zawiera efekt pozwalajacy na rozegranie kolejnej tury
            var czyPonownaTura = ruch.KartaCudu.Efekty.Select(e => e.TypEfektu).Contains(TypEfektu.RozegrajTurePonownie);
            var czyZaBudoweCuduRozegrajTurePonownie = AktywnyGracz.Efekty.Select(e => e.TypEfektu).Contains(TypEfektu.ZaBudoweCuduRozegrajTurePonownie);
            //if (czyPonownaTura || czyZaBudoweCuduRozegrajTurePonownie)
            if (czyPonownaTura)
            {
                Console.WriteLine("Efekt karty cudu: Rozegraj ture ponownie! Aktywny gracz wykonuje kolejny ruch.");
                return;
            }
            if (czyZaBudoweCuduRozegrajTurePonownie)
            {
                Console.WriteLine("Efekt karty cudu: Za budowe cudu rozegrj ture ponownie! Aktywny gracz wykonuje kolejny ruch.");
                return;
            }
        }
        ZmienTure(); 
    }

    private Karta ZnajdzDostepnaKarte(string nazwa)
    {
        return DostepneKarty()
            .First(k => k.Nazwa == nazwa);
    }

    private KartaCudu ZnajdzKarteCudu(string nazwa)
    {
        return AktywnyGracz.KartyCudow
            .First(k => k.Nazwa == nazwa);
    }

    private void ZmienTure()
    {
        idAktywnegoGracza = (idAktywnegoGracza + 1) % gracze.Length;
    }

    public IReadOnlyList<Karta> DostepneKarty()
    {
        var karty = planszaEpoki.DostepneKarty
            .Select(p => p.Karta)
            .ToList();
        if (!karty.Any())
        {
            Console.WriteLine("Brak dostepnych kart na planszy epoki!");
            return new List<Karta>();
        }
        return karty;
    }

    public IReadOnlyList<Ruch> DostepneRuchy()
    {
        var wynik = new List<Ruch>();
        var karty = DostepneKarty();
        var kartyCudow = AktywnyGracz.KartyCudow;
        //Console.WriteLine($"Aktywny gracz: {AktywnyGracz.Nazwa}, Przeciwnik: {Przeciwnik.Nazwa}");

        foreach (var karta in karty)
        {
            if(karta.CzyWidoczna || karta.CzyOdrzucona || karta.CzyZagrana)
                continue;
            // budowa karty
            var budowaKarty = AktywnyGracz.CzyMoznaZbudowacKarte(karta, Przeciwnik);
            if (budowaKarty.MoznaZagrac)
            {
                wynik.Add(new Ruch(AktywnyGracz, Przeciwnik, karta, TypRuchu.ZbudujKarte));
            }
            // odrzucenie karty
            wynik.Add(new Ruch(AktywnyGracz, Przeciwnik, karta, TypRuchu.OdrzucKarte));
            // budowa cudu
            foreach (var kartaCudu in kartyCudow)
            {
                if (!CzyLiczbaZbudowanychCudowMniejsza7())
                    continue;
                var budowaCudu = AktywnyGracz.CzyMoznaZbudowacCud(karta, Przeciwnik, kartaCudu);
                if (budowaCudu.MoznaZagrac)
                {
                    wynik.Add(new Ruch(AktywnyGracz, Przeciwnik, karta, TypRuchu.ZbudujCud, kartaCudu));
                }
            }
        }
        return wynik;
    }

    public bool CzyLiczbaZbudowanychCudowMniejsza7()
    {
        var cudaAktywnegoGracza = AktywnyGracz.PobierzZbudowaneKartyCudow().Count;
        var cudaPrzeciwnika = Przeciwnik.PobierzZbudowaneKartyCudow().Count;
        return cudaAktywnegoGracza + cudaPrzeciwnika < 7;
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

    public void ZakonczGre()
    {
        stanGry.CzyZwyciestwoMilitarne(gracze, planszaKonfliktu.PionKonfliktu.PobierzPozycje());
        stanGry.CzyZwyciestwoNaukowe(gracze);
        stanGry.CzyZwyciestwoPunktowe(gracze, planszaKonfliktu);
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
    private static PlanszaKonfliktu InicjalizacjaPlanszy(Gracz[] gracze, IRandom random)
    {
        var pionKonfliktu = new PionKonfliktu(0);
        var zetonyPostepu = ZbiorZetonowPostepu.ZetonyPostepu
            .OrderBy(x => random?.Next())
            .Take(5)
            .ToList();
        var strefy = ZbiorStref.Strefy.Select(k => k.Clone()).ToList();

        return new PlanszaKonfliktu(pionKonfliktu, zetonyPostepu, strefy, gracze);
    }
    private static PlanszaEpoki UtworzPlanszeEpoki(Epoka epoka, IRandom random)
    {
        var taliaKart = epoka switch
        {
            Epoka.EpokaI => ZbiorKart.TaliaEpokiI.Select(k => k.Clone()).ToList(),
            Epoka.EpokaII => ZbiorKart.TaliaEpokiII.Select(k => k.Clone()).ToList(),
            Epoka.EpokaIII => ZbiorKart.TaliaEpokiIII.Select(k => k.Clone()).ToList(),
            _ => throw new ArgumentException("Nieznana epoka")
        };
        return ZbiorPlanszEpok.Utworz(epoka, taliaKart.ToList(), random);
    }
    private static List<KartaCudu> InicjalizacjaKartCudow(IRandom random)
    {
        var zbior = ZbiorKart.TaliaKartyCudow.Select(k => k.Clone());
        foreach (var karta in zbior)
        {
            if (karta is KartaCudu kartaCudu)
            {
                kartaCudu.OznaczJakoNiezagrana();
            }
        }
        return zbior
            .OrderBy(x => random.Next())
            .Take(8) // 4 w pierwszej fazie wyboru, 4 w drugiej fazie wyboru
            .ToList();
    }
    //TEMP: Gracze sami wybieraja cuda, ale na potrzeby testowania przydzielamy im losowo 4 karty
    private static void PrzydzielenieCudow(Gracz[] gracze, List<KartaCudu> cuda)
    {
        for (int i = 0; i < 4; i++)
        {
            gracze[0].KartyCudow.Add(cuda[i]);
        }
        for (int i = 4; i < 8; i++)
        {
            gracze[1].KartyCudow.Add(cuda[i]);
        }
    }
}