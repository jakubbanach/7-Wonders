using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

public class StanGry
{
    public bool CzyZakonczona { get; private set; }
    public TypZwyciestwa TypZwyciestwa { get; private set; }
    public Gracz? Zwyciezca { get; private set; }
    private int PunktyGracza1 { get; set; }
    private int PunktyGracza2 { get; set; }

    public StanGry()
    {
        CzyZakonczona = false;
        TypZwyciestwa = TypZwyciestwa.Brak;
        Zwyciezca = null;
        PunktyGracza1 = 0;
        PunktyGracza2 = 0;
    }

    private StanGry(StanGry stan)
    {
        CzyZakonczona = stan.CzyZakonczona;
        TypZwyciestwa = stan.TypZwyciestwa;
        Zwyciezca = stan.Zwyciezca;
        PunktyGracza1 = stan.PunktyGracza1;
        PunktyGracza2 = stan.PunktyGracza2;
    }

    public StanGry Clone()
    {
        return new StanGry(this);
    }

    public void CzyZwyciestwoMilitarne(Gracz[] gracze, int pozycjaPionu)
    {
        if (pozycjaPionu >= 9)
        {
            PoliczPunkty(gracze, null);
            ZakonczGre(gracze[0], TypZwyciestwa.Militarne);
        }

        if (pozycjaPionu <= -9)
        {
            PoliczPunkty(gracze, null);
            ZakonczGre(gracze[1], TypZwyciestwa.Militarne);
        }
    }

    public void CzyZwyciestwoNaukowe(Gracz[] gracze, PlanszaKonfliktu planszaKonfliktu)
    {
        foreach (var gracz in gracze)
            if (gracz.SymboleNaukowe.Distinct().Count() >= 6)
            {
                PoliczPunkty(gracze, planszaKonfliktu);
                ZakonczGre(gracz, TypZwyciestwa.Naukowe);
            }
    }

    public void CzyZwyciestwoPunktowe(Gracz[] gracze, PlanszaKonfliktu planszaKonfliktu)
    {
        PoliczPunkty(gracze, planszaKonfliktu);

        if (PunktyGracza1 > PunktyGracza2)
            ZakonczGre(gracze[0], TypZwyciestwa.Punktowe);
        else if (PunktyGracza2 > PunktyGracza1)
            ZakonczGre(gracze[1], TypZwyciestwa.Punktowe);
        else // remis, brak zwyciestwa punktowego -> TODO
            CzyZwyciestwoRemis(gracze);
    }
    public void PoliczPunkty(Gracz[] gracze, PlanszaKonfliktu planszaKonfliktu)
    {

        if (planszaKonfliktu != null)
        {
            int pozycjaPionu = planszaKonfliktu.PionKonfliktu.PobierzPozycje();
            PunktyPrzewagaMilitarna(gracze, planszaKonfliktu.PobierzStrefeDlaPozycji(pozycjaPionu), pozycjaPionu);
        }
        PunktyMonety(gracze);
        PunktyZwyciestwa(gracze);
        PunktyEfekty(gracze);
    }

    public void CzyZwyciestwoRemis(Gracz[] gracze)
    {
        var finalnePunktyGracza1 = PunktyGracza1;
        var finalnePunktyGracza2 = PunktyGracza2;
        PunktyGracza1 = 0;
        PunktyGracza2 = 0;

        foreach (var gracz in gracze)
        {
            var niebieskieEfekty = gracz.ZbudowaneKarty
                .Where(k => k.KolorKarty == KolorKarty.Niebieski)
                .SelectMany(k => k.Efekty)
                .Where(e => e.TypEfektu == TypEfektu.PunktyZwyciestwa);

            foreach (var efekt in niebieskieEfekty)
            {
                if (gracz == gracze[0])
                    PunktyGracza1 += efekt.Wartosc;
                else
                    PunktyGracza2 += efekt.Wartosc;
            }
        }

        if (PunktyGracza1 > PunktyGracza2)
            ZakonczGre(gracze[0], TypZwyciestwa.Punktowe);
        else if (PunktyGracza2 > PunktyGracza1)
            ZakonczGre(gracze[1], TypZwyciestwa.Punktowe);
        else
            ZakonczGre(null, TypZwyciestwa.Brak);
        PunktyGracza1 = finalnePunktyGracza1;
        PunktyGracza2 = finalnePunktyGracza2;
    }
    public void PunktyPrzewagaMilitarna(Gracz[] gracze, Strefa strefa, int pozycjaPionu)
    {
        if (pozycjaPionu > 0)
        {
            if (strefa.LiczbaPunktow > 0)
                PunktyGracza1 += strefa.LiczbaPunktow;
        }
        else if (pozycjaPionu < 0)
        {
            if (strefa.LiczbaPunktow > 0)
                PunktyGracza2 += strefa.LiczbaPunktow;
        }
    }
    public void PunktyMonety(Gracz[] gracze)
    {
        foreach (var gracz in gracze)
        {
            int monety = gracz.WypiszLiczbeSurowca(Surowiec.Monety);
            if (monety > 0)
            {
                if (gracz == gracze[0])
                    PunktyGracza1 += monety / 3;
                else
                    PunktyGracza2 += monety / 3;
            }
        }
    }
    public void PunktyZwyciestwa(Gracz[] gracze)
    {
        foreach (var gracz in gracze)
        {
            int punktyZwyciestwa = gracz.PunktyZwyciestwa;
            if (punktyZwyciestwa > 0)
            {
                if (gracz == gracze[0])
                    PunktyGracza1 += punktyZwyciestwa;
                else
                    PunktyGracza2 += punktyZwyciestwa;
            }
        }
    }
    public void PunktyEfekty(Gracz[] gracze)
    {
        foreach (var gracz in gracze)
        {
            var efektyKoncowe = gracz.PobierzEfekty()
                .Where(e => e != null && KartaBazowa.EfektyKoniecGry.Contains(e.TypEfektu))
                .ToList();
            foreach (var efekt in efektyKoncowe)
            {
                if (efekt.TypEfektu == TypEfektu.KoniecGry3PunktyZaZetonPostepu)
                {
                    var zetony = gracz.ZetonyPostepu.Count;
                    if (gracz == gracze[0])
                        PunktyGracza1 += zetony * 3;
                    else
                        PunktyGracza2 += zetony * 3;
                }
                else if (efekt.TypEfektu == TypEfektu.PunktyZaKarty)
                {
                    Gracz? przeciwnik = PobierzPrzeciwnika(gracz, gracze);

                    Func<Gracz, int> licznikKart = efekt.Tekst switch
                    {
                        "Monety" => g => g.WypiszLiczbeSurowca(Surowiec.Monety) / 3,
                        "Cuda" => g => g.KartyCudow.Where(k  => k.CzyZagrana).Count(),
                        "Brazowy i Szary" => g => g.ZbudowaneKarty.Count(k => k.KolorKarty == KolorKarty.Brazowy || k.KolorKarty == KolorKarty.Szary),
                        _ => g => g.ZbudowaneKarty.Count(k => k.KolorKarty.ToString() == efekt.Tekst)
                    };

                    int n1 = licznikKart(gracz);
                    int n2 = przeciwnik != null ? licznikKart(przeciwnik) : 0;

                    int punkty = efekt.Wartosc * Math.Max(n1, n2);

                    if (gracz == gracze[0])
                        PunktyGracza1 += punkty;
                    else
                        PunktyGracza2 += punkty;
                }
            }
        }
    }
    private Gracz? PobierzPrzeciwnika(Gracz gracz, Gracz[] gracze)
    {
        if (gracz == gracze[0])
            return gracze[1];
        else if (gracz == gracze[1])
            return gracze[0];
        else
            return null;
    }
    public void ZakonczGre(Gracz zwyciezca, TypZwyciestwa typ)
    {
        CzyZakonczona = true;
        TypZwyciestwa = typ;
        Zwyciezca = zwyciezca;
    }

    public int GetPunktyGracza1()
    {
        return PunktyGracza1;
    }
    public int GetPunktyGracza2()
    {
        return PunktyGracza2;
    }
}
