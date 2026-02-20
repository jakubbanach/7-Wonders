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
            case TypEfektu.WyborSurowca:
                // brak efektu natychmiastowego, implementacja w funkcji ObliczKoszt
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
            case TypEfektu.ZmianaCenySurowca:
                // brak efektu natychmiastowego, implementacja w funkcji ObliczKoszt
                break;
            case TypEfektu.BialySymbol:
                gracz.BialeSymbole.Add(Tekst);
                break;
            case TypEfektu.SymbolNaukowy:
                gracz.SymboleNaukowe.Add(SymbolNaukowy);
                break;
            case TypEfektu.MonetyZaKarty:
                if (Tekst == "Cuda")
                {
                    var liczbaKartGracza = gracz.KartyCudow.Count;
                    var liczbaKartPrzeciwnika = przeciwnik != null ? przeciwnik.KartyCudow.Count : 0;
                    
                    gracz.DodajMonety(Wartosc * (liczbaKartGracza>liczbaKartPrzeciwnika ? liczbaKartGracza : liczbaKartPrzeciwnika));
                }
                else if (Tekst == "Br¹zowy i Szary")
                {
                    var liczbaKartGracza = gracz.ZbudowaneKarty.Count(k => k.KolorKarty == KolorKarty.Br¹zowy || k.KolorKarty == KolorKarty.Szary);
                    var liczbaKartPrzeciwnika = przeciwnik != null ? przeciwnik.ZbudowaneKarty.Count(k => k.KolorKarty == KolorKarty.Br¹zowy || k.KolorKarty == KolorKarty.Szary) : 0;
                    gracz.DodajMonety(Wartosc * (liczbaKartGracza > liczbaKartPrzeciwnika ? liczbaKartGracza : liczbaKartPrzeciwnika));
                }
                else
                {
                    var liczbaKartGracza = gracz.ZbudowaneKarty.Count(k => k.KolorKarty.ToString() == Tekst);
                    var liczbaKartPrzeciwnika = przeciwnik != null ? przeciwnik.ZbudowaneKarty.Count(k => k.KolorKarty.ToString() == Tekst) : 0;
                    gracz.DodajMonety(Wartosc * (liczbaKartGracza > liczbaKartPrzeciwnika ? liczbaKartGracza : liczbaKartPrzeciwnika));
                }
                break;
            //case TypEfektu.PunktyZaKarty: -> do Punktacji Koncowej
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