using System.Collections.Generic;
using System.Linq;
using System;

public static class ZbiorKart
{
    public static IReadOnlyList<Karta> TaliaEpokiI { get; }
    public static IReadOnlyList<Karta> TaliaEpokiII { get; }
    public static IReadOnlyList<Karta> TaliaEpokiIII { get; }
    public static IReadOnlyList<KartaCudu> TaliaKartyCudow { get; }

    static ZbiorKart()
    {
        TaliaEpokiI = InicjalizujTalieEpokiI();
        TaliaEpokiII = InicjalizujTalieEpokiII();
        TaliaEpokiIII = InicjalizujTalieEpokiIII();
        TaliaKartyCudow = InicjalizujTalieKartyCudow();
    }

    private static List<Karta> InicjalizujTalieEpokiI()
    {
        var talia = new List<Karta>
        {
            new Karta(
                "Wycinka",
                Epoka.EpokaI,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 } })
                },
                KolorKarty.Brazowy
            ),
            new Karta(
                "Zloza Gliny",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 } })
                },
                KolorKarty.Brazowy
            ),
            new Karta(
                "Skladowisko Kamienia",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Kamien, 1 } })
                },
                KolorKarty.Brazowy
            ),
            new Karta(
                "Glinianka",
                Epoka.EpokaI,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 } })
                },
                KolorKarty.Brazowy
            ),
            new Karta(
                "Sklad Drewna",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 } })
                },
                KolorKarty.Brazowy
            ),
            new Karta(
                "Kamieniolom",
                Epoka.EpokaI,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Kamien, 1 } })
                },
                KolorKarty.Brazowy
            ),
            new Karta(
                "Huta Szkla",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Szklo, 1 } })
                },
                KolorKarty.Szary
            ),
            new Karta(
                "Wytwornia Papirusu",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Papirus, 1 } })
                },
                KolorKarty.Szary
            ),
            new Karta(
                "Tawerna",
                Epoka.EpokaI,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Monety,wartosc: 4),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Wazon")
                },
                KolorKarty.Zolty
            ),
            new Karta(
                "Magazyn Kamienia",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 3 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.ZmianaCenySurowca,surowiec: Surowiec.Kamien,wartosc: 1)
                },
                KolorKarty.Zolty
            ),
            new Karta(
                "Magazyn Drewna",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 3 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.ZmianaCenySurowca,surowiec: Surowiec.Drewno,wartosc: 1)
                },
                KolorKarty.Zolty
            ),
            new Karta(
                "Magazyn Gliny",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 3 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.ZmianaCenySurowca,surowiec: Surowiec.Glina,wartosc: 1)
                },
                KolorKarty.Zolty
            ),
            new Karta(
                "Skryptorium",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.Pismo),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Ksiega")
                },
                KolorKarty.Zielony
            ),
            new Karta(
                "Apteka",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Szklo, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.Kolo),
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 1)
                },
                KolorKarty.Zielony
            ),
            new Karta(
                "Zielarnia",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.Mozdzierz),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Zebatka")
                },
                KolorKarty.Zielony
            ),
            new Karta(
                "Warsztat",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.Liczydlo),
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 1)
                },
                KolorKarty.Zielony
            ),
            new Karta(
                "Garnizon",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartosc: 1),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Miecz")
                },
                KolorKarty.Czerwony
            ),
            new Karta(
                "Palisada",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartosc: 1),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Mur")
                },
                KolorKarty.Czerwony
            ),
            new Karta(
                "Wieza Straznicza",
                Epoka.EpokaI,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartosc: 1)
                },
                KolorKarty.Czerwony
            ),
            new Karta(
                "Stajnie",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartosc: 1),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Podkowa")
                },
                KolorKarty.Czerwony
            ),
            new Karta(
                "Teatr",
                Epoka.EpokaI,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 3),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Maska")
                },
                KolorKarty.Niebieski
            ),
            new Karta(
                "Laznie",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Kamien, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 3),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Kropla")
                },
                KolorKarty.Niebieski
            ),
            new Karta(
                "Oltarz",
                Epoka.EpokaI,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 3),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Ksiezyc")
                },
                KolorKarty.Niebieski
            )
        };
        return talia;
    }
    private static List<Karta> InicjalizujTalieEpokiII()
    {
        var talia = new List<Karta>
        {
            new Karta(
            "Kamieniolom Stokowy",
            Epoka.EpokaII,
            new Dictionary<Surowiec, int> { { Surowiec.Monety, 2 } },
            new List<Efekt>
            {
                new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Kamien, 2 } })
            },
            KolorKarty.Brazowy
            ),
            new Karta(
            "Cegielnia",
            Epoka.EpokaII,
            new Dictionary<Surowiec, int> { { Surowiec.Monety, 2 } },
            new List<Efekt>
            {
                new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 } })
            },
            KolorKarty.Brazowy
            ),
            new Karta(
            "Tartak",
            Epoka.EpokaII,
            new Dictionary<Surowiec, int> { { Surowiec.Monety, 2 } },
            new List<Efekt>
            {
                new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Drewno, 2 } })
            },
            KolorKarty.Brazowy
            ),
            new Karta(
                "Pracownia Wyrobu Szkla",
                Epoka.EpokaII,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Szklo, 1 } })
                },
                KolorKarty.Szary
            ),
            new Karta(
                "Suszarnia Papirusu",
                Epoka.EpokaII,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Papirus, 1 } })
                },
                KolorKarty.Szary
            ),
            new Karta(
                "Karawanseraj",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 2 }, { Surowiec.Szklo, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.WyborSurowca, new Dictionary<Surowiec, int> { 
                        { Surowiec.Drewno, 1 }, { Surowiec.Glina, 1 }, { Surowiec.Kamien, 1 }
                    })
                },
                KolorKarty.Zolty
            ),
            new Karta(
                "Urzad celny",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 4 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.ZmianaCenySurowca,surowiec: Surowiec.Papirus,wartosc: 1),
                    new Efekt(TypEfektu.ZmianaCenySurowca,surowiec: Surowiec.Szklo,wartosc: 1)
                },
                KolorKarty.Zolty
            ),
            new Karta(
                "Browar",
                Epoka.EpokaII,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Monety,wartosc: 6),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Beczka")
                },
                KolorKarty.Zolty
            ),
            new Karta(
                "Forum",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 3 }, { Surowiec.Glina, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.WyborSurowca, new Dictionary<Surowiec, int> {
                        { Surowiec.Szklo, 1 }, { Surowiec.Papirus, 1 }
                    })
                },
                KolorKarty.Zolty
            ),
            new Karta(
                "Labolatorium",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Szklo, 2 }, { Surowiec.Drewno, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.Liczydlo),
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 1),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Lampa")
                },
                KolorKarty.Zielony
            ),
            new Karta(
                "Biblioteka",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamien, 1 }, { Surowiec.Drewno, 1 }, { Surowiec.Szklo, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.Pismo),
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 2)
                },
                KolorKarty.Zielony,
                darmowaBudowa: "Ksiega"
            ),
            new Karta(
                "Szkola",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 }, { Surowiec.Papirus, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.Kolo),
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 1),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Harfa")
                },
                KolorKarty.Zielony
            ),
            new Karta(
                "Ambulatorium",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 }, { Surowiec.Kamien, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.Mozdzierz),
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 2)
                },
                KolorKarty.Zielony,
                darmowaBudowa: "Zebatka"
            ),
            new Karta(
                "Mury Obronne",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamien, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartosc: 2)
                },
                KolorKarty.Czerwony
            ),
            new Karta(
                "Plac Apelowy",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 }, { Surowiec.Szklo, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartosc: 2),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Helm")
                },
                KolorKarty.Czerwony
            ),
            new Karta(
                "Koszary",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 3 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartosc: 1)
                },
                KolorKarty.Czerwony,
                darmowaBudowa: "Miecz"
            ),
            new Karta(
                "Stadniny",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 }, { Surowiec.Drewno, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartosc: 1)
                },
                KolorKarty.Czerwony,
                darmowaBudowa: "Podkowa"
            ),
            new Karta(
                "Tor Strzelecki",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamien, 1 }, { Surowiec.Drewno, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartosc: 2),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Strzelnica")
                },
                KolorKarty.Czerwony
            ),
            new Karta(
                "Gmach Sadu",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 2 }, { Surowiec.Szklo, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 5)
                },
                KolorKarty.Niebieski
            ),
            new Karta(
                "Akwedukt",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamien, 3 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 5)
                },
                KolorKarty.Niebieski,
                darmowaBudowa: "Kropla"
            ),
            new Karta(
                "Mownica",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamien, 1 }, { Surowiec.Drewno, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 4),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Sad")
                },
                KolorKarty.Niebieski
            ),
            new Karta(
                "Swiatynia",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 4),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Slonce")
                },
                KolorKarty.Niebieski,
                darmowaBudowa: "Ksiezyc"
            ),
            new Karta(
                "Posag",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 4),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Kolumna")
                },
                KolorKarty.Niebieski,
                darmowaBudowa: "Maska"
            )
        };
        return talia;
    }
    private static List<Karta> InicjalizujTalieEpokiIII()
    {
        var gildie = new List<Karta>
        {
            new Karta(
                "Cech Lichwiarzy",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamien, 2 }, { Surowiec.Drewno, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZaKarty, tekst: "Monety", wartosc: 1)
                },
                KolorKarty.Fioletowy
            ),
            new Karta(
                "Cech Budowniczych",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamien, 2 }, { Surowiec.Glina, 1 }, { Surowiec.Drewno, 1 }, { Surowiec.Szklo, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZaKarty, tekst: "Cuda", wartosc: 2)
                },
                KolorKarty.Fioletowy
            ),
            new Karta(
                "Gildia Kupiecka",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 }, { Surowiec.Drewno, 1 }, { Surowiec.Szklo, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.MonetyZaKartyWiecejWMiescie, tekst: "Zolty", wartosc: 1),
                    new Efekt(TypEfektu.PunktyZaKarty, tekst: "Zolty", wartosc: 1)
                },
                KolorKarty.Fioletowy
            ),
            new Karta(
                "Stowarzyszenie Urzednikow",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 2 }, { Surowiec.Glina, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.MonetyZaKartyWiecejWMiescie, tekst: "Niebieski", wartosc: 1),
                    new Efekt(TypEfektu.PunktyZaKarty, tekst: "Niebieski", wartosc: 1)
                },
                KolorKarty.Fioletowy
            ),
            new Karta(
                "Cech Armatorow",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 }, { Surowiec.Kamien, 1 }, { Surowiec.Szklo, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.MonetyZaKartyWiecejWMiescie, tekst: "Brazowy i Szary", wartosc: 1),
                    new Efekt(TypEfektu.PunktyZaKarty, tekst: "Brazowy i Szary", wartosc: 1),
                },
                KolorKarty.Fioletowy
            ),
            new Karta(
                "Gildia Strategow",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamien, 2 }, { Surowiec.Glina, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.MonetyZaKartyWiecejWMiescie, tekst: "Czerwony", wartosc: 1),
                    new Efekt(TypEfektu.PunktyZaKarty, tekst: "Czerwony", wartosc: 1)
                },
                KolorKarty.Fioletowy
            ),
            new Karta(
                "Towarzystwo Naukowe",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 }, { Surowiec.Drewno, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.MonetyZaKartyWiecejWMiescie, tekst: "Zielony", wartosc: 1),
                    new Efekt(TypEfektu.PunktyZaKarty, tekst: "Zielony", wartosc: 1)
                },
                KolorKarty.Fioletowy
            ),
        };
        var talia = new List<Karta>
        {
            new Karta(
                "Latarnia Morska",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 }, { Surowiec.Szklo, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartosc: 3),
                    new Efekt(TypEfektu.MonetyZaKarty, tekst: "Zolty", wartosc: 1)
                },
                KolorKarty.Zolty,
                darmowaBudowa: "Wazon"
            ),
            new Karta(
                "Port",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 }, { Surowiec.Szklo, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartosc: 3),
                    new Efekt(TypEfektu.MonetyZaKarty, tekst: "Brazowy", wartosc: 2)
                },
                KolorKarty.Zolty
            ),
            new Karta(
                "Arena",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 }, { Surowiec.Kamien, 1 }, { Surowiec.Drewno, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartosc: 3),
                    new Efekt(TypEfektu.MonetyZaKarty, tekst: "Cuda", wartosc: 2)
                },
                KolorKarty.Zolty,
                darmowaBudowa: "Beczka"
            ),
            new Karta(
                "Izba Handlowa",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Papirus, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartosc: 3),
                    new Efekt(TypEfektu.MonetyZaKarty, tekst: "Szary", wartosc: 3)
                },
                KolorKarty.Zolty
            ),
            new Karta(
                "Zbrojownia",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamien, 2 }, { Surowiec.Szklo, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartosc: 3),
                    new Efekt(TypEfektu.MonetyZaKarty, tekst: "Czerwony", wartosc: 1)
                },
                KolorKarty.Zolty
            ),
            new Karta(
                "Akademia",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamien, 1 }, { Surowiec.Drewno, 1 }, { Surowiec.Szklo, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.ZegarSloneczny),
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 3)
                },
                KolorKarty.Zielony
            ),
            new Karta(
                "Pracownia Naukowa",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 2 }, { Surowiec.Szklo, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.ZegarSloneczny),
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 3)
                },
                KolorKarty.Zielony
            ),
            new Karta(
                "Uniwersytet",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 }, { Surowiec.Szklo, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.Globus),
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 2)
                },
                KolorKarty.Zielony,
                darmowaBudowa: "Harfa"
            ),
            new Karta(
                "Obserwatorium",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamien, 1 }, { Surowiec.Papirus, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.Globus),
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 2)
                },
                KolorKarty.Zielony,
                darmowaBudowa: "Lampa"
            ),
            new Karta(
                "Cyrk",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 }, { Surowiec.Kamien, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartosc: 2)
                },
                KolorKarty.Czerwony,
                darmowaBudowa: "Helm"
            ),
            new Karta(
                "Fortyfikacje",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamien, 2 }, { Surowiec.Glina, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartosc: 2)
                },
                KolorKarty.Czerwony,
                darmowaBudowa: "Mur"
            ),
            new Karta(
                "Arsenal",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 3 }, { Surowiec.Drewno, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartosc: 3)
                },
                KolorKarty.Czerwony
            ),
            new Karta(
                "Siedziba Trybuna",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 8 }},
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartosc: 3)
                },
                KolorKarty.Czerwony
            ),
            new Karta(
                "Machiny Obleznicze",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 3 }, { Surowiec.Szklo, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartosc: 2)
                },
                KolorKarty.Czerwony,
                darmowaBudowa: "Strzelnica"
            ),
            new Karta(
                "Obelisk",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamien, 2 }, { Surowiec.Szklo, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 5)
                },
                KolorKarty.Niebieski
            ),
            new Karta(
                "Budynek Senatu",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 }, { Surowiec.Kamien, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 5)
                },
                KolorKarty.Niebieski,
                darmowaBudowa: "Sad"
            ),
            new Karta(
                "Ratusz",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamien, 3 }, { Surowiec.Drewno, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 7)
                },
                KolorKarty.Niebieski
            ),
            new Karta(
                "Palac",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 }, { Surowiec.Kamien, 1 }, { Surowiec.Drewno, 1 }, { Surowiec.Szklo, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 7)
                },
                KolorKarty.Niebieski
            ),
            new Karta(
                "Panteon",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 }, { Surowiec.Drewno, 1 }, { Surowiec.Papirus, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 6)
                },
                KolorKarty.Niebieski,
                darmowaBudowa: "Slonce"
            ),
            new Karta(
                "Ogrody",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 }, { Surowiec.Drewno, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 6)
                },
                KolorKarty.Niebieski,
                darmowaBudowa: "Kolumna"
            )
        };
        return talia.Concat(gildie).ToList();
    }

    private static List<KartaCudu> InicjalizujTalieKartyCudow()
    {
        var talia = new List<KartaCudu>
        {
            new KartaCudu(
                "Piramida Cheopsa",
                new Dictionary<Surowiec, int> { { Surowiec.Kamien, 3 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartosc: 9)
                }
            ),
            new KartaCudu(
                "Wiszace Ogrody Semiramidy",
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 2 }, { Surowiec.Szklo, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Monety, wartosc: 6),
                    new Efekt(TypEfektu.RozegrajTurePonownie),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartosc: 3)
                }
            ),
            new KartaCudu(
                "Swiatynia Artemidy W Efezie",
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 }, { Surowiec.Kamien, 1 }, { Surowiec.Szklo, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Monety,wartosc: 12),
                    new Efekt(TypEfektu.RozegrajTurePonownie)
                }
            ),
            new KartaCudu(
                "Sfinks",
                new Dictionary<Surowiec, int> { { Surowiec.Kamien, 1 }, { Surowiec.Glina, 1 }, { Surowiec.Szklo, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartosc: 6),
                    new Efekt(TypEfektu.RozegrajTurePonownie)
                }
            ),
            new KartaCudu(
                "Latarnia Morska Na Faros",
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 }, { Surowiec.Kamien, 1 }, { Surowiec.Papirus, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.WyborSurowca, surowce: new Dictionary<Surowiec, int>
                    {
                        { Surowiec.Drewno, 1 },
                        { Surowiec.Glina, 1 },
                        { Surowiec.Kamien, 1 }
                    }),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartosc: 4)
                }
            ),
            new KartaCudu(
                "Kolos Rodyjski",
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 3 }, { Surowiec.Szklo, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne, wartosc: 2),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartosc: 3)
                }
            ),
            new KartaCudu(
                "Via Appia",
                new Dictionary<Surowiec, int> { { Surowiec.Kamien, 2 }, { Surowiec.Glina, 2 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Monety, wartosc: 3),
                    new Efekt(TypEfektu.PrzeciwnikOdkladaMonety, wartosc: 3),
                    new Efekt(TypEfektu.RozegrajTurePonownie),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartosc: 3)
                }
            ),
            new KartaCudu(
                "Pireus",
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 2 }, { Surowiec.Kamien, 1 }, { Surowiec.Glina, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.WyborSurowca, surowce: new Dictionary<Surowiec, int>
                    {
                        { Surowiec.Papirus, 1 },
                        { Surowiec.Szklo, 1 }
                    }),
                    new Efekt(TypEfektu.RozegrajTurePonownie),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartosc: 2)
                }
            ),
            new KartaCudu(
                "Mauzoleum W Halikarnasie",
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 }, { Surowiec.Szklo, 2 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.DarmowaBudowlaZOdrzuconychKart),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartosc: 2)
                }
            ),
            new KartaCudu(
                "Circus Maximus",
                new Dictionary<Surowiec, int> { { Surowiec.Kamien, 2 }, { Surowiec.Drewno, 1 }, { Surowiec.Szklo, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.OdlozKartePrzeciwnika, tekst: "Szary"),
                    new Efekt(TypEfektu.PunktyMilitarne, wartosc: 1),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartosc: 3)
                }
            ),
            new KartaCudu(
                "Posag Zeusa W Olimpii",
                new Dictionary<Surowiec, int> { { Surowiec.Kamien, 1 }, { Surowiec.Drewno, 1 }, { Surowiec.Glina, 1 }, { Surowiec.Papirus, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.OdlozKartePrzeciwnika, tekst: "Brazowy"),
                    new Efekt(TypEfektu.PunktyMilitarne, wartosc: 1),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartosc: 3)
                }
            ),
            new KartaCudu(
                "Biblioteka Aleksandryjska",
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 3 }, { Surowiec.Szklo, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Wylosuj3ZetonyPostepu),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartosc: 4)
                }
            ),
        };
        return talia;
    }
}