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

    public void ZastosujEfekt(Gracz gracz, PlanszaKonfliktu planszaKonfliktu = null)
    {
        // Implementacja zastosowania efektu na graczu
        // Ta metoda mo¿e byæ rozszerzona w zale¿noœci od typu efektu
        switch (TypEfektu)
        {
            case TypEfektu.Surowiec:
                foreach (var surowiec in Surowce)
                {
                    gracz.DodajSurowiec(surowiec.Key, surowiec.Value);
                }
                break;
            //case TypEfektu.WyborSurowca: -> do implementacji wyboru surowca przez gracza
            //case TypEfektu.PunktyZwyciestwa: -> do Punktacji Koncowej
            case TypEfektu.Monety:
                gracz.DodajMonety(Wartosc);
                break;
            case TypEfektu.PunktyMilitarne:
                if (planszaKonfliktu != null)
                {
                    planszaKonfliktu.PrzesunPion(Wartosc, gracz);
                }
                break;
            //case TypEfektu.ZmianaCenySurowca: -> do implementacji zmiany ceny surowca
            case TypEfektu.BialySymbol:
                gracz.BialeSymbole.Add(Tekst);
                break;
            case TypEfektu.SymbolNaukowy:
                gracz.symboleNaukowe.Add(SymbolNaukowy);
                break;
            case TypEfektu.MonetyZaKarty:
                if (Tekst == "Cuda")
                {
                    var liczbaKart = gracz.KartyCudow.Count;
                    // dodaj warunek zbudowania karty cudu
                    // do dodania karty cudu przeciwnika
                    gracz.DodajMonety(Wartosc * liczbaKart);
                }
                else
                {
                    var liczbaKart = gracz.ZbudowaneKarty.Count(k => k.KolorKarty.ToString() == Tekst);
                    gracz.DodajMonety(Wartosc * liczbaKart);
                }
                break;
            //case TypEfektu.PunktyZaKarty: -> do Punktacji Koncowej
            //case TypEfektu.RozegrajTurePonownie: -> do implementacji w mechanice tury
            case TypEfektu.PrzeciwnikOdkladaMonety:
                // do implementacji w mechanice tury
                break;
            //case TypEfektu.DarmowaBudowlaZOdrzuconychKart: -> do implementacji w mechanice tury
            case TypEfektu.OdlozKartePrzeciwnika:
                // do implementacji w mechanice tury
                break;
            case TypEfektu.Wylosuj3ZetonyPostepu:
                // do implementacji w mechanice tury
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