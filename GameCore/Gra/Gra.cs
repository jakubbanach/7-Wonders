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
    private IDecisionResolver? currentResolver;
    private IRandom? currentRandom;
    private List<Karta> stosKartOdrzuconych = new List<Karta>();

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
    public IReadOnlyList<Karta> StosKartOdrzuconych => stosKartOdrzuconych;

    private Gra(Gra gra)
    {
        idAktywnegoGracza = gra.idAktywnegoGracza;
        gracze = gra.gracze
            .Select(g => g.Clone())
            .ToArray();
        planszaKonfliktu = gra.planszaKonfliktu.Clone();
        planszaEpoki = gra.planszaEpoki.Clone();
        stanGry = gra.stanGry.Clone();
        stosKartOdrzuconych = gra.stosKartOdrzuconych
            .Select(k => k.Clone())
            .ToList();
    }

    public Gra Clone()
    {
        return new Gra(this);
    }
    public void CopyFrom(Gra source)
    {
        idAktywnegoGracza = source.idAktywnegoGracza;

        for (int i = 0; i < source.gracze.Length; i++)
            gracze[i].CopyFrom(source.gracze[i]);

        planszaKonfliktu.CopyFrom(source.planszaKonfliktu);
        planszaEpoki.CopyFrom(source.planszaEpoki);
        stanGry.CopyFrom(source.stanGry);

        stosKartOdrzuconych.Clear();

        for (int i = 0; i < source.stosKartOdrzuconych.Count; i++)
        {
            if (i < stosKartOdrzuconych.Count)
                stosKartOdrzuconych[i] = source.stosKartOdrzuconych[i].Clone();
            else
                stosKartOdrzuconych.Add(source.stosKartOdrzuconych[i].Clone());
        }

        stosKartOdrzuconych.RemoveRange(
            source.stosKartOdrzuconych.Count,
            stosKartOdrzuconych.Count - source.stosKartOdrzuconych.Count);
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

    public void WykonajRuch(Ruch ruchNew, IDecisionResolver resolver, IRandom random)
    {
        currentResolver = resolver;
        currentRandom = random;
        var karta = ZnajdzDostepnaKarte(ruchNew.KartaDoZagrania.Nazwa);

        var kartaCudu = ruchNew.KartaCudu == null ? null : ZnajdzKarteCudu(ruchNew.KartaCudu.Nazwa);

        Ruch ruch = new Ruch(AktywnyGracz, Przeciwnik, karta, ruchNew.TypRuchu, kartaCudu);
        ruch.Wykonaj(this, planszaKonfliktu);

        currentResolver = null;
        currentRandom = null;
        
        var poleKarty = planszaEpoki.ZnajdzPole(karta);
        if (poleKarty == null)
        {
            Console.WriteLine("Nie mozna znalezc karty na planszy epoki!");
            return;
        }
        planszaEpoki.UsunPole(poleKarty);
        if (CzyKoniecGry())
        {
            return;
        }
        if (CzyKoniecEpoki())
        {
            if (planszaEpoki.Epoka == Epoka.EpokaIII)
            {
                ZakonczGre();
                return;
            }
            planszaEpoki = UtworzPlanszeEpoki(planszaEpoki.Epoka + 1, random);

            WybierzKtoZaczynaKolejnaEpoke(resolver);
            return;
        }

        // sprawdzamy czy efekt karty cudu pozwala na rozegranie kolejnej tury
        if (ruch.TypRuchu == TypRuchu.ZbudujCud && ruch.KartaCudu != null)
        {
            // sprawdzamy czy lista efektow karty cudu zawiera efekt pozwalajacy na rozegranie kolejnej tury
            var czyPonownaTura = ruch.KartaCudu.Efekty.Select(e => e.TypEfektu).Contains(TypEfektu.RozegrajTurePonownie);
            var czyZaBudoweCuduRozegrajTurePonownie = AktywnyGracz.Efekty.Select(e => e.TypEfektu).Contains(TypEfektu.ZaBudoweCuduRozegrajTurePonownie);
            if (czyPonownaTura || czyZaBudoweCuduRozegrajTurePonownie)
            {
                return;
            }
        }
        ZmienTure(); 
    }

    public void Efekt_Losuj3Zetony(IDecisionResolver? resolver = null)
    {
        //Console.WriteLine("Efekt: Wylosuj 3 zetony postepu");
        if (resolver != null)
        {
            currentResolver = resolver;
        }
        if (currentResolver == null)
        {
            Console.WriteLine("Brak resolvera do losowania zetonu postepu!");
            return;
        }
        if (currentRandom == null)
        {
            Console.WriteLine("Brak generatora losowego do losowania zetonu postepu!");
            return;
        }
        var zetonyNaPlanszy = planszaKonfliktu.ZetonyPostepu;
        var pozostale = ZbiorZetonowPostepu.ZetonyPostepu
            .Where(z => !zetonyNaPlanszy.Any(zp => zp.Nazwa == z.Nazwa))
            .ToList();

        var wylosowane = pozostale
            .OrderBy(_ => currentRandom.Next(int.MaxValue))
            .Take(3)
            .ToList();

        var decyzja = new DecyzjaKontekst<ZetonPostepu>(
            TypEfektu.Wylosuj3ZetonyPostepu,
            wylosowane,
            decisionResolver: currentResolver
        );

        var wybor = currentResolver.Resolve(this.Clone(), decyzja);

        AktywnyGracz.DodajZetonPostepu(wybor);
        //Console.WriteLine($"Gracz {AktywnyGracz.Nazwa} losuje zeton postepu {wybor.Nazwa}");
    }
    public void Efekt_WybierzZetonPostepu(IDecisionResolver? resolver = null)
    {
        //Console.WriteLine($"Efekt: Wybierz 1 zeton postepu z {planszaKonfliktu.ZetonyPostepu.Count} dostepnych");
        if (resolver != null)
        {
            currentResolver = resolver;
        }
        if (currentResolver == null)
        {
            Console.WriteLine("Brak resolvera do wyboru zetonu postepu!");
            return;
        }
        if (!planszaKonfliktu.ZetonyPostepu.Any())
        {
            Console.WriteLine("Brak zetonow postepu na planszy!");
            return;
        }
        var zetonyNaPlanszy = planszaKonfliktu.ZetonyPostepu;

        var decyzja = new DecyzjaKontekst<ZetonPostepu>(
            TypEfektu.WybierzZetonPostepu,
            zetonyNaPlanszy,
            decisionResolver: currentResolver
        );

        var wybor = currentResolver.Resolve(this.Clone(), decyzja);
        if (wybor == null)
        {
            Console.WriteLine("Resolver nie zwrocil wyboru zetonu postepu!");
            return;
        }

        AktywnyGracz.DodajZetonPostepu(wybor);
        planszaKonfliktu.UsunZetonPostepu(wybor);
        //Console.WriteLine($"Gracz {AktywnyGracz.Nazwa} wybiera zeton postepu {wybor.Nazwa}");
    }
    public void Efekt_OdlozKartePrzeciwnika(string kolorOdkladanejKarty)
    {
        //Console.WriteLine($"Efekt: Odloz karte przeciwnika o kolorze {kolorOdkladanejKarty}");
        if (currentResolver == null)
        {
            Console.WriteLine("Brak resolvera do wyboru karty przeciwnika!");
            return;
        }
        // mozliwe tylko do odlozenia karta szara lub brazowa, wiec sprawdzamy tylko te karty
        var kolorKarty = kolorOdkladanejKarty == "Szary" ? KolorKarty.Szary : KolorKarty.Brazowy;

        var kartyPrzeciwnika = Przeciwnik.ZbudowaneKarty
            .Where(k => k.KolorKarty == kolorKarty)
            .ToList();

        if (kartyPrzeciwnika.Count == 0)
        {
            //Console.WriteLine("Przeciwnik nie ma kart do odlozenia o wymaganym kolorze!");
            return;
        }

        var decyzja = new DecyzjaKontekst<Karta>(
            TypEfektu.OdlozKartePrzeciwnika,
            kartyPrzeciwnika,
            decisionResolver: currentResolver
        );

        var wybor = currentResolver?.Resolve(this.Clone(), decyzja);
        if (wybor == null)
        {
            Console.WriteLine("Resolver nie zwrocil wyboru karty przeciwnika!");
            return;
        }

        Przeciwnik.UsunKarte(wybor);
        //Console.WriteLine($"Przeciwnik odklada karte {wybor.Nazwa}");
    }
    public void Efekt_DarmowaBudowla()
    {
        //Console.WriteLine("Efekt: Darmowa budowla z odrzuconych kart");
        if (currentResolver == null)
        {
            Console.WriteLine("Brak resolvera do wyboru darmowej budowli!");
            return;
        }
        var odrzuconeKarty = StosKartOdrzuconych.ToList();
        if (!odrzuconeKarty.Any())
        {
            // Console.WriteLine("Brak odrzuconych kart do wyboru darmowej budowli!");
            return;
        }

        var decyzja = new DecyzjaKontekst<Karta>(
            TypEfektu.DarmowaBudowlaZOdrzuconychKart,
            odrzuconeKarty,
            decisionResolver: currentResolver
        );

        var kartaDoDodania = currentResolver.Resolve(this.Clone(), decyzja);

        //Console.WriteLine($"Gracz {AktywnyGracz.Nazwa} wybiera karte {kartaDoDodania.Nazwa} do darmowej budowy");
        kartaDoDodania.OznaczJakoZagrana();
        kartaDoDodania.OznaczJakoNieodrzucona();
        UsunZeStosuOdrzuconych(kartaDoDodania);
        AktywnyGracz.ZbudowaneKarty.Add(kartaDoDodania);

        foreach (var efekt in kartaDoDodania.Efekty)
        {
            efekt.ZastosujEfekt(AktywnyGracz, Przeciwnik, planszaKonfliktu, kartaDoDodania, this);
            AktywnyGracz.DodajEfekt(efekt);
        }
    }
    private void WybierzKtoZaczynaKolejnaEpoke(IDecisionResolver resolver)
    {
        var decydujacy = UstalGraczaDecydujacego();

        var opcje = gracze.ToList();

        var decyzja = new DecyzjaKontekst<Gracz>(
            TypEfektu.WybierzGraczaRozpoczynajacegoEpoke,
            opcje,
            decydujacy
        );

        var wybrany = resolver.Resolve(this.Clone(), decyzja);

        idAktywnegoGracza = Array.IndexOf(gracze, wybrany);
    }
    private Gracz UstalGraczaDecydujacego()
    {
        var pozycjaKonfliktu = planszaKonfliktu.PionKonfliktu.PobierzPozycje();
        if (pozycjaKonfliktu > 0)
        {
            return gracze[1]; // terytorium gracza 2
        }
        if (pozycjaKonfliktu < 0)
        {
            return gracze[0];
        }

        return gracze[idAktywnegoGracza];
    }
    public void OdrzucKarte(Karta karta)
    {
        stosKartOdrzuconych.Add(karta);
    }

    public void UsunZeStosuOdrzuconych(Karta karta)
    {
        stosKartOdrzuconych.Remove(karta);
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

    public void ZmienTure()
    {
        idAktywnegoGracza = (idAktywnegoGracza + 1) % gracze.Length;
    }

    public void PotasujZakryteKarty(IRandom random)
    {
        planszaEpoki.PotasujZakryteKarty(random);
    }
    public IReadOnlyList<Karta> DostepneKarty()
    {
        var karty = planszaEpoki.DostepneKarty
            .Select(p => p.Karta)
            .ToList();
        if (!karty.Any())
        {
            // Console.WriteLine("Brak dostepnych kart na planszy epoki!");
            return new List<Karta>();
        }
        return karty;
    }
    private readonly List<Ruch> _listaRuchowBuffer = new List<Ruch>(32);
    public IReadOnlyList<Ruch> DostepneRuchy()
    {
        _listaRuchowBuffer.Clear();
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
                _listaRuchowBuffer.Add(new Ruch(AktywnyGracz, Przeciwnik, karta, TypRuchu.ZbudujKarte));
            }
            // odrzucenie karty
            _listaRuchowBuffer.Add(new Ruch(AktywnyGracz, Przeciwnik, karta, TypRuchu.OdrzucKarte));
            // budowa cudu
            foreach (var kartaCudu in kartyCudow)
            {
                if (!CzyLiczbaZbudowanychCudowMniejsza7())
                    continue;
                var budowaCudu = AktywnyGracz.CzyMoznaZbudowacCud(karta, Przeciwnik, kartaCudu);
                if (budowaCudu.MoznaZagrac)
                {
                    _listaRuchowBuffer.Add(new Ruch(AktywnyGracz, Przeciwnik, karta, TypRuchu.ZbudujCud, kartaCudu));
                }
            }
        }
        return _listaRuchowBuffer;
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
        stanGry.CzyZwyciestwoNaukowe(gracze, planszaKonfliktu);
        if (stanGry.CzyZakonczona)
            return true;
        return false;
    }

    public void ZakonczGre()
    {
        stanGry.CzyZwyciestwoMilitarne(gracze, planszaKonfliktu.PionKonfliktu.PobierzPozycje());
        stanGry.CzyZwyciestwoNaukowe(gracze, planszaKonfliktu);
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