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
    private PlanszaKonfliktu plansza;
    private PlanszaEpoki planszaEpoki;
    private Gracz aktywnyGracz;

    public GraKonsolowa()
    {
        plansza = InicjalizacjaPlanszy();
        gracze = InicjalizacjaGraczy();
        planszaEpoki = UtworzPlanszeEpoki(Epoka.EpokaI);
        aktywnyGracz = gracze[0];
    }

    public void Start()
    {
        Dictionary<Epoka, List<Karta>> kartyEpok = InicjalizacjaKartEpok();
        List<KartaCudu> kartyCudow = InicjalizacjaKartCudow();

        Gracz gracz1 = gracze[0];
        Gracz gracz2 = gracze[1];

        // Przydzielanie kart cudów do graczy -> hardkodowane dla testów
        gracz1.DodajKarteCudu(kartyCudow[0]);
        gracz1.DodajKarteCudu(kartyCudow[2]);
        gracz1.DodajKarteCudu(kartyCudow[4]);
        gracz1.DodajKarteCudu(kartyCudow[10]);

        gracz2.DodajKarteCudu(kartyCudow[5]);
        gracz2.DodajKarteCudu(kartyCudow[6]);
        gracz2.DodajKarteCudu(kartyCudow[7]);
        gracz2.DodajKarteCudu(kartyCudow[9]);

        Console.WriteLine($"Pion konfliktu na pozycji: {plansza.PionKonfliktu.PobierzPozycje()}");
        Console.WriteLine($"Zetony postepu: {string.Join(", ", plansza.ZetonyPostepu.Select(z => z.Nazwa))}");
        Console.WriteLine($"Strefy: {string.Join(", ", plansza.Strefy.Select(s => s.Nazwa))}");

        Console.WriteLine($"Karty Gracza 1: {gracz1.WypiszKartyCudu()}");
        Console.WriteLine($"Karty Gracza 2: {gracz2.WypiszKartyCudu()}");
        Console.WriteLine($"Monety Gracza 1: {gracz1.WypiszLiczbeSurowca(Surowiec.Monety)}");
        Console.WriteLine($"Monety Gracza 2: {gracz2.WypiszLiczbeSurowca(Surowiec.Monety)}");

        Gracz aktywnyGracz = gracz1;

        Console.WriteLine("Plansza Epoki I:");

        ZbiorPlanszEpok.WypiszPlansze(planszaEpoki);
        Console.WriteLine("Naciœnij dowolny klawisz, aby rozpocz¹æ grê...");
        Console.ReadKey();
        //DOCELOWO - rozgrywka do koñca epoki
        //while (!planszaEpoki.CzyKoniecEpoki)
        //{
        //    RozegrajTure();
        //}

        //while (planszaEpoki.DostepneKarty.Any())
        //{
        //    Console.Clear();
        //    var wypisanaPlansza = WypiszPlansze(planszaEpoki, gracze, plansza);
        //    Console.WriteLine(wypisanaPlansza);

        //    Console.WriteLine($"Ruch gracza: {aktywnyGracz.Nazwa}");
        //    int indeks = _wyborKarty.Wybierz(planszaEpoki.DostepneKarty.ToList());
        //    PoleKarty wybrana = planszaEpoki.DostepneKarty.ElementAt(indeks);

        //    Console.WriteLine($"Wybrano kartê: {wybrana.Karta!.Nazwa}");

        //    aktywnyGracz.ZbudujKarte(wybrana.Karta);
        //    planszaEpoki.UsunPole(wybrana);

        //    Console.WriteLine(new string('*', 11));

        //    aktywnyGracz = aktywnyGracz == gracz1 ? gracz2 : gracz1;

        //}
        RozegrajEpoke(planszaEpoki);
        Console.WriteLine("Koniec epoki!");

        planszaEpoki = UtworzPlanszeEpoki(Epoka.EpokaII);
        Console.WriteLine("Plansza Epoki II:");
        ZbiorPlanszEpok.WypiszPlansze(planszaEpoki);
        Console.WriteLine("Naciœnij dowolny klawisz, aby kontynuowaæ grê...");
        Console.ReadKey();
        RozegrajEpoke(planszaEpoki);
        Console.WriteLine("Koniec epoki!");

        planszaEpoki = UtworzPlanszeEpoki(Epoka.EpokaIII);
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
        return new PlanszaKonfliktu(pionKonfliktu, zetonyPostepu, strefy);
    }

    Gracz[] InicjalizacjaGraczy(String nazwa1 = "Gracz 1", String nazwa2 = "Gracz 2")
    {
        return new Gracz[]
        {
            new Gracz(nazwa1),
            new Gracz(nazwa2)
        };
    }

    Dictionary<Epoka, List<Karta>> InicjalizacjaKartEpok()
    {
        var kartyEpokiI = ZbiorKart.TaliaEpokiI;
        var kartyEpokiII = ZbiorKart.TaliaEpokiII;
        var kartyEpokiIII = ZbiorKart.TaliaEpokiIII;

        return new Dictionary<Epoka, List<Karta>>
        {
            { Epoka.EpokaI, kartyEpokiI.ToList() },
            { Epoka.EpokaII, kartyEpokiII.ToList() },
            { Epoka.EpokaIII, kartyEpokiIII.ToList() }
        };
    }

    List<KartaCudu> InicjalizacjaKartCudow()
    {
        return ZbiorKart.TaliaKartyCudow.ToList();
    }

    PlanszaEpoki UtworzPlanszeEpoki(Epoka epoka)
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

    void RozegrajEpoke(PlanszaEpoki planszaEpoki)
    {
        while (planszaEpoki.DostepneKarty.Any())
        {
            Console.Clear();
            var wypisanaPlansza = WypiszPlansze(planszaEpoki, gracze, plansza);
            Console.WriteLine(wypisanaPlansza);

            Console.WriteLine($"Ruch gracza: {aktywnyGracz.Nazwa}");
            int indeks = _wyborKarty.Wybierz(planszaEpoki.DostepneKarty.ToList());
            PoleKarty wybrana = planszaEpoki.DostepneKarty.ElementAt(indeks);

            Console.WriteLine($"Wybrano kartê: {wybrana.Karta!.Nazwa}");

            aktywnyGracz.ZbudujKarte(wybrana.Karta);
            planszaEpoki.UsunPole(wybrana);

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
