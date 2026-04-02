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
    public Efekt(TypEfektu typEfektu, Dictionary<Surowiec, int> surowce = null, int wartoœæ = 0, string tekst = "", SymbolNaukowy symbolNaukowy=0,Surowiec surowiec=0)
    {
        TypEfektu = typEfektu;
        Surowce = surowce ?? new Dictionary<Surowiec, int>();
        Wartosc = wartoœæ;
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
        return $"Efekt: {TypEfektu}, Surowce: {string.Join(", ", Surowce)}, Wartoœæ: {Wartosc}, Tekst: {Tekst}";
    }

    public void ZastosujEfekt(Gracz gracz, Gracz? przeciwnik = null, PlanszaKonfliktu? planszaKonfliktu = null, Karta? karta = null)
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
                        wartoscDoDodania += 1; // Dodaj 1 punkt militarny za ka¿d¹ czerwon¹ kartê
                    }
                    planszaKonfliktu.PrzesunPion(wartoscDoDodania, gracz);
                }
                break;
            case TypEfektu.BialySymbol:
                gracz.DodajBialySymbol(Tekst);
                break;
            case TypEfektu.SymbolNaukowy:
                gracz.DodajSymbolNaukowy(SymbolNaukowy);
                break;
            case TypEfektu.MonetyZaKarty:
                Func<Gracz, int> licznikKart = Tekst switch
                {
                    "Monety" => g => g.WypiszLiczbeSurowca(Surowiec.Monety) / 3,
                    "Cuda" => g => g.KartyCudow.Where(k => k.CzyZagrana).Count(),
                    "Br¹zowy i Szary" => g => g.ZbudowaneKarty.Count(k => k.KolorKarty == KolorKarty.Br¹zowy || k.KolorKarty == KolorKarty.Szary),
                    _ => g => g.ZbudowaneKarty.Count(k => k.KolorKarty.ToString() == Tekst)
                };

                int n1 = licznikKart(gracz);
                int n2 = przeciwnik != null ? licznikKart(przeciwnik) : 0;

                gracz.DodajMonety(Wartosc * Math.Max(n1, n2));
                break;
            //case TypEfektu.DarmowaBudowlaZOdrzuconychKart: -> do implementacji w mechanice tury
            case TypEfektu.Wylosuj3ZetonyPostepu:
                // do implementacji w mechanice tury
                break;
            case TypEfektu.OdlozKartePrzeciwnika:
                // do implementacji w mechanice tury
                break;
            //case TypEfektu.RozegrajTurePonownie: -> do implementacji w mechanice tury
            case TypEfektu.PrzeciwnikOdkladaMonety:
                if (przeciwnik != null)
                {
                    przeciwnik.DodajMonety(-Wartosc);
                }
                break;
            case TypEfektu.WybierzZetonPostepu:
                // jakoœ daæ graczowi wybór ¿etonu postêpu
                // wybrany ¿eton dodajemy do gracza
                // do implementacji w mechanice tury
                break;
            default:
                // Inne typy efektów do implementacji
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
                return $"Wybór - {string.Join(" lub ", Surowce.Select(s => $"{s.Value}x{s.Key}"))}";
            case TypEfektu.PunktyZwyciestwa:
                return $"{Wartosc} Punkty Zwyciêstwa";
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
                return $"{Wartosc} monet za ka¿d¹ kartê {Tekst}";
            case TypEfektu.PunktyZaKarty:
                if (Tekst == "Monety")
                {
                    return $"{Wartosc} punktów za ka¿de 3 monety u gracza co ma ich wiêcej";
                }
                return $"{Wartosc} punktów za ka¿d¹ kartê {Tekst} u gracza co ma ich wiêcej";
            case TypEfektu.RozegrajTurePonownie:
                return "Rozegraj turê ponownie";
            case TypEfektu.PrzeciwnikOdkladaMonety:
                return $"Przeciwnik odk³ada {Wartosc} monet";
            case TypEfektu.DarmowaBudowlaZOdrzuconychKart:
                return "Mo¿esz zbudowaæ darmowo budowlê z odrzuconych kart";
            case TypEfektu.OdlozKartePrzeciwnika:
                return $"Odk³adasz 1 {Tekst} kartê przeciwnika";
            case TypEfektu.Wylosuj3ZetonyPostepu:
                return "Wylosuj 3 ¿etony postêpu";
            default:
                return TypEfektu.ToString();
        }
    }
}