using System;
using System.Collections.Generic;
using System.Linq;
public class Efekt
{
    public TypEfektu TypEfektu { get; protected set; }
    public Dictionary<Surowiec, int> Surowce { get; protected set; }
    public int Wartosc { get; protected set; }
    public string Tekst { get; protected set; }
    public SymbolNaukowy SymbolNaukowy { get; protected set; }
    public Surowiec Surowiec { get; protected set; }

    // TODO: ogarnac SymbolNaukowy i Surowiec, zeby nie przyjmowaly zawsze wartosci domyslnych (odpowiednio Globus i Glina)
    public Efekt(TypEfektu typEfektu, Dictionary<Surowiec, int> surowce = null, int wartosc = 0, string tekst = "", SymbolNaukowy symbolNaukowy=0,Surowiec surowiec=0)
    {
        TypEfektu = typEfektu;
        Surowce = surowce ?? new Dictionary<Surowiec, int>();
        Wartosc = wartosc;
        Tekst = tekst;
        SymbolNaukowy = symbolNaukowy;
        Surowiec = surowiec;
    }

    private Efekt(Efekt efekt)
    {
        TypEfektu = efekt.TypEfektu;
        Surowce = new Dictionary<Surowiec, int>(efekt.Surowce);
        Wartosc = efekt.Wartosc;
        Tekst = efekt.Tekst;
        SymbolNaukowy = efekt.SymbolNaukowy;
        Surowiec = efekt.Surowiec;
    }

    public Efekt Clone()
    {
        return new Efekt(this);
    }

    public override string ToString()
    {
        return $"Efekt: {TypEfektu}, Surowce: {string.Join(", ", Surowce)}, Wartosc: {Wartosc}, Tekst: {Tekst}";
    }

    public void ZastosujEfekt(Gracz gracz, Gracz? przeciwnik = null, PlanszaKonfliktu? planszaKonfliktu = null, 
        Karta? karta = null, Gra gra = null, IDecisionResolver? decisionResolver = null)
    {
        switch (TypEfektu)
        {
            case TypEfektu.Surowiec:
                foreach (var surowiec in Surowce)
                {
                    gracz.DodajSurowiec(surowiec.Key, surowiec.Value);
                }
                break;
            case TypEfektu.PunktyZwyciestwa: // -> do Punktacji Koncowej
                gracz.DodajPunktyZwyciestwa(Wartosc);
                break;
            case TypEfektu.Monety:
                gracz.DodajMonety(Wartosc);
                break;
            case TypEfektu.PunktyMilitarne:
                if (planszaKonfliktu != null)
                {
                    int wartoscDoDodania = Wartosc;
                    if (karta != null && karta.KolorKarty == KolorKarty.Czerwony && 
                        gracz.Efekty.Any(e => e.TypEfektu == TypEfektu.DodatkoweMilitariaZaCzerwoneKarty))
                    {
                        wartoscDoDodania += 1; // Dodaj 1 punkt militarny za kazda czerwona karte
                    }
                    planszaKonfliktu.PrzesunPion(wartoscDoDodania, gracz);
                }
                break;
            case TypEfektu.BialySymbol:
                gracz.DodajBialySymbol(Tekst);
                break;
            case TypEfektu.SymbolNaukowy:
                bool duplikat = gracz.DodajSymbolNaukowy(SymbolNaukowy);

                if (duplikat && gra != null)
                {
                    gra.Efekt_WybierzZetonPostepu(decisionResolver);
                }
                break;
            case TypEfektu.MonetyZaKarty:
                Func<Gracz, int> licznikKartGracza = Tekst switch
                {
                    "Cuda" => g => g.KartyCudow.Where(k => k.CzyZagrana).Count(),
                    _ => g => g.ZbudowaneKarty.Count(k => k.KolorKarty.ToString() == Tekst)
                };

                int liczbaKart = licznikKartGracza(gracz);
                gracz.DodajMonety(Wartosc * liczbaKart);
                break;
            case TypEfektu.MonetyZaKartyWiecejWMiescie:
                Func<Gracz, int> licznikKart = Tekst switch
                {
                    "Monety" => g => g.WypiszLiczbeSurowca(Surowiec.Monety) / 3,
                    "Cuda" => g => g.KartyCudow.Where(k => k.CzyZagrana).Count(),
                    "Brazowy i Szary" => g => g.ZbudowaneKarty.Count(k => k.KolorKarty == KolorKarty.Brazowy || k.KolorKarty == KolorKarty.Szary),
                    _ => g => g.ZbudowaneKarty.Count(k => k.KolorKarty.ToString() == Tekst)
                };

                int n1 = licznikKart(gracz);
                int n2 = przeciwnik != null ? licznikKart(przeciwnik) : 0;

                gracz.DodajMonety(Wartosc * Math.Max(n1, n2));
                break;
            case TypEfektu.PrzeciwnikOdkladaMonety:
                if (przeciwnik != null)
                {
                    przeciwnik.DodajMonety(-Wartosc);
                }
                break;
            case TypEfektu.Wylosuj3ZetonyPostepu:
                gra.Efekt_Losuj3Zetony(decisionResolver);
                break;

            case TypEfektu.OdlozKartePrzeciwnika:
                gra.Efekt_OdlozKartePrzeciwnika(Tekst);
                break;

            case TypEfektu.DarmowaBudowlaZOdrzuconychKart:
                gra.Efekt_DarmowaBudowla();
                break;
            default:
                break;
        }
    }

    public string Wypisz()
    {
        switch (TypEfektu)
        {
            case TypEfektu.Surowiec:
                return string.Join(" + ", Surowce.Select(s => $"{s.Value}x{s.Key}"));
            case TypEfektu.WyborSurowca:
                return $"Wybor - {string.Join(" lub ", Surowce.Select(s => $"{s.Value}x{s.Key}"))}";
            case TypEfektu.PunktyZwyciestwa:
                return $"{Wartosc} Punkty Zwyciestwa";
            case TypEfektu.Monety:
                return $"{Wartosc} Monet";
            case TypEfektu.PunktyMilitarne:
                return $"{Wartosc} Punkty Militarne";
            case TypEfektu.ZmianaCenySurowca:
                return $"Zmiana ceny {Surowiec} na {Wartosc}";
            case TypEfektu.BialySymbol:
                return Tekst;
            case TypEfektu.SymbolNaukowy:
                return $"SN - {SymbolNaukowy}";
            case TypEfektu.MonetyZaKarty:
                return $"{Wartosc} monet za kazda karte {Tekst}";
            case TypEfektu.MonetyZaKartyWiecejWMiescie:
                 if (Tekst == "Monety")
                {
                    return $"{Wartosc} monet za kazde 3 monety u gracza co ma ich wiecej";
                }
                return $"{Wartosc} monet za kazda karte {Tekst} u gracza co ma ich wiecej";
            case TypEfektu.PunktyZaKarty:
                if (Tekst == "Monety")
                {
                    return $"{Wartosc} punktow za kazde 3 monety u gracza co ma ich wiecej";
                }
                return $"{Wartosc} punktow za kazda karte {Tekst} u gracza co ma ich wiecej";
            case TypEfektu.RozegrajTurePonownie:
                return "Rozegraj ture ponownie";
            case TypEfektu.PrzeciwnikOdkladaMonety:
                return $"Przeciwnik odklada {Wartosc} monet";
            case TypEfektu.DarmowaBudowlaZOdrzuconychKart:
                return "Mozesz zbudowac darmowo budowle z odrzuconych kart";
            case TypEfektu.OdlozKartePrzeciwnika:
                return $"Odkladasz 1 {Tekst} karte przeciwnika";
            case TypEfektu.Wylosuj3ZetonyPostepu:
                return "Wylosuj 3 zetony postepu";
            default:
                return TypEfektu.ToString();
        }
    }
}