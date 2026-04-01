using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

public class GraKonsolowa
{
    //private readonly PlanszaFactory _planszaFactory = new();
    private readonly IWyborKarty _wyborKarty = new WyborKartKonsola();
    private Gracz[] gracze;
    private PlanszaKonfliktu planszaKonfliktu;
    private PlanszaEpoki planszaEpoki;
    private Gracz aktywnyGracz;
    private Gracz przeciwnik => gracze.First(g => g != aktywnyGracz);
    private StanGry stanGry = new StanGry();
    private IRandom random = new RandomAdapter(12345);


    public GraKonsolowa()
    {
        gracze = InicjalizacjaGraczy();
        planszaKonfliktu = InicjalizacjaPlanszy();
        planszaEpoki = UtworzPlanszeEpoki(Epoka.EpokaI, random);
        aktywnyGracz = gracze[0];
    }

    public void PrzydzielanieKartCudow()
    {
        List<KartaCudu> kartyCudow = InicjalizacjaKartCudow();
        // Przydzielanie kart cudów do graczy -> hardkodowane dla testów
        gracze[0].DodajKarteCudu(kartyCudow[0]);
        gracze[0].DodajKarteCudu(kartyCudow[2]);
        gracze[0].DodajKarteCudu(kartyCudow[4]);
        gracze[0].DodajKarteCudu(kartyCudow[10]);

        gracze[1].DodajKarteCudu(kartyCudow[5]);
        gracze[1].DodajKarteCudu(kartyCudow[6]);
        gracze[1].DodajKarteCudu(kartyCudow[7]);
        gracze[1].DodajKarteCudu(kartyCudow[9]);
    }

    public void Start()
    {
        Gracz gracz1 = gracze[0];
        Gracz gracz2 = gracze[1];

        // Przydzielanie kart cudów do graczy -> hardkodowane dla testów
        PrzydzielanieKartCudow();

        Console.WriteLine($"Pion konfliktu na pozycji: {planszaKonfliktu.PionKonfliktu.PobierzPozycje()}");
        Console.WriteLine($"Zetony postepu: {string.Join(", ", planszaKonfliktu.ZetonyPostepu.Select(z => z.Nazwa))}");
        Console.WriteLine($"Strefy: {string.Join(", ", planszaKonfliktu.Strefy.Select(s => s.Nazwa))}");

        Console.WriteLine($"Karty Gracza 1: {gracz1.WypiszKartyCudu()}");
        Console.WriteLine($"Karty Gracza 2: {gracz2.WypiszKartyCudu()}");
        Console.WriteLine($"Monety Gracza 1: {gracz1.WypiszLiczbeSurowca(Surowiec.Monety)}");
        Console.WriteLine($"Monety Gracza 2: {gracz2.WypiszLiczbeSurowca(Surowiec.Monety)}");

        Gracz aktywnyGracz = gracz1;

        //Console.WriteLine("Plansza Epoki I:");

        //ZbiorPlanszEpok.WypiszPlansze(planszaEpoki);
        //Console.WriteLine("Naciœnij dowolny klawisz, aby rozpocz¹æ grê...");
        //Console.ReadKey();
        //RozegrajEpoke(planszaEpoki);
        //Console.WriteLine("Koniec epoki!");

        planszaEpoki = UtworzPlanszeEpoki(Epoka.EpokaII, random);
        Console.WriteLine("Plansza Epoki II:");
        ZbiorPlanszEpok.WypiszPlansze(planszaEpoki);
        Console.WriteLine("Naciœnij dowolny klawisz, aby kontynuowaæ grê...");
        Console.ReadKey();
        RozegrajEpoke(planszaEpoki);
        Console.WriteLine("Koniec epoki!");

        planszaEpoki = UtworzPlanszeEpoki(Epoka.EpokaIII, random);
        Console.WriteLine("Plansza Epoki III:");
        ZbiorPlanszEpok.WypiszPlansze(planszaEpoki);
        Console.WriteLine("Naciœnij dowolny klawisz, aby kontynuowaæ grê...");
        Console.ReadKey();
        RozegrajEpoke(planszaEpoki);
    }
    PlanszaKonfliktu InicjalizacjaPlanszy()
    {
        var pionKonfliktu = new PionKonfliktu(0);
        var zetonyPostepu = ZbiorZetonowPostepu.ZetonyPostepu.OrderBy(x => Guid.NewGuid()).Take(5).ToList();
        var strefy = ZbiorStref.Strefy.ToList();

        // Inicjalizacja planszy do gry
        return new PlanszaKonfliktu(pionKonfliktu, zetonyPostepu, strefy, gracze);
    }

    Gracz[] InicjalizacjaGraczy(String nazwa1 = "Gracz 1", String nazwa2 = "Gracz 2")
    {
        return new Gracz[]
        {
            new Gracz(nazwa1),
            new Gracz(nazwa2)
        };
    }

    List<KartaCudu> InicjalizacjaKartCudow()
    {
        return ZbiorKart.TaliaKartyCudow.ToList();
    }

    PlanszaEpoki UtworzPlanszeEpoki(Epoka epoka, IRandom random)
    {
        var taliaKart = epoka switch
        {
            Epoka.EpokaI => ZbiorKart.TaliaEpokiI,
            Epoka.EpokaII => ZbiorKart.TaliaEpokiII,
            Epoka.EpokaIII => ZbiorKart.TaliaEpokiIII,
            _ => throw new ArgumentException("Nieznana epoka")
        };
        return ZbiorPlanszEpok.Utworz(epoka, taliaKart.ToList(), random);
    }

    TypRuchu WybierzRuch(Karta karta)
    {
        Console.WriteLine($"Co chcesz zrobiæ z kart¹?");
        while(true)
        {
            Console.WriteLine("1. Zagraj kartê");
            Console.WriteLine("2. Odrzuæ kartê");
            Console.WriteLine("3. Zbuduj cud");
            string? input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    return TypRuchu.ZbudujKarte;
                case "2":
                    return TypRuchu.OdrzucKarte;
                case "3":
                    return TypRuchu.ZbudujCud;
                default:
                    Console.WriteLine("Nieprawid³owy wybór, spróbuj ponownie.");
                    continue;
            }
        }
    }

    void RozegrajTure(Gracz gracz, Gracz przeciwnik, Karta karta)
    {
        TypRuchu typRuchu = WybierzRuch(karta);

        Ruch ruch = new Ruch(aktywnyGracz, przeciwnik, karta, typRuchu);
        ruch.Wykonaj(planszaKonfliktu);
        stanGry.CzyZwyciestwoMilitarne(gracze, planszaKonfliktu.PionKonfliktu.PobierzPozycje());
        stanGry.CzyZwyciestwoNaukowe(gracze);
        if (stanGry.CzyZakonczona)
        {
            Console.WriteLine("Gra zakoñczona!");
            Console.WriteLine($"Zwyciêzca: {stanGry?.Zwyciezca?.Nazwa}");
            Console.WriteLine($"Punkty zwyciêstwa: {stanGry?.Zwyciezca?.PunktyZwyciestwa}");
            Environment.Exit(0);
        }
    }

    void RozegrajEpoke(PlanszaEpoki planszaEpoki)
    {
        while (planszaEpoki.DostepneKarty.Any())
        {
            Console.Clear();
            var wypisanaPlansza = WypiszPlansze(planszaEpoki, gracze, planszaKonfliktu);
            Console.WriteLine(wypisanaPlansza);
            Console.WriteLine($"Ruch gracza: {aktywnyGracz.Nazwa}");

            int indeks = _wyborKarty.Wybierz(planszaEpoki.DostepneKarty.ToList());
            PoleKarty wybranePole = planszaEpoki.DostepneKarty.ElementAt(indeks);

            Console.WriteLine($"Wybrano kartê: {wybranePole.Karta!.Nazwa}");

            RozegrajTure(aktywnyGracz, przeciwnik, wybranePole.Karta);
            //aktywnyGracz.WykonajRuch(wybrana.Karta);
            //aktywnyGracz.ZbudujKarte(wybrana.Karta);
            planszaEpoki.UsunPole(wybranePole);

            Console.WriteLine(new string('*', 11));

            aktywnyGracz = aktywnyGracz == gracze[0] ? gracze[1] : gracze[0];
        }
    }

    string WypiszPlansze(PlanszaEpoki planszaEpoki, Gracz[] gracze, PlanszaKonfliktu planszaKonfliktu)
    {
        var sb = new StringBuilder();

        // ===== KONFLIKT =====
        sb.AppendLine("TOR KONFLIKTU");
        sb.AppendLine(planszaKonfliktu.WypiszTorKonfliktu());
        sb.AppendLine(new string('=', 40));

        // ===== GRACZ 1 =====
        sb.AppendLine(gracze[0].WypiszStan());
        sb.AppendLine(new string('=', 40));

        // ===== PLANSZA EPOKI =====
        sb.AppendLine("PLANSZA EPOKI");
        sb.AppendLine(planszaEpoki.PlanszaDoStringa());
        
        // ===== GRACZ 2 =====
        sb.AppendLine(new string('=', 40));
        sb.AppendLine(gracze[1].WypiszStan());
        sb.AppendLine(new string('=', 40));

        return sb.ToString();
    }
}
