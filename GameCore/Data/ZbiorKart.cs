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
                KolorKarty.Brązowy
            ),
            new Karta(
                "Złoża Gliny",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 } })
                },
                KolorKarty.Brązowy
            ),
            new Karta(
                "Składowisko Kamienia",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Kamień, 1 } })
                },
                KolorKarty.Brązowy
            ),
            new Karta(
                "Glinianka",
                Epoka.EpokaI,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 } })
                },
                KolorKarty.Brązowy
            ),
            new Karta(
                "Skład Drewna",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 } })
                },
                KolorKarty.Brązowy
            ),
            new Karta(
                "Kamieniołom",
                Epoka.EpokaI,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Kamień, 1 } })
                },
                KolorKarty.Brązowy
            ),
            new Karta(
                "Huta Szkła",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Szkło, 1 } })
                },
                KolorKarty.Szary
            ),
            new Karta(
                "Wytwórnia Papirusu",
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
                    new Efekt(TypEfektu.Monety,wartość: 4),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Wazon")
                },
                KolorKarty.Żółty
            ),
            new Karta(
                "Magazyn Kamienia",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 3 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.ZmianaCenySurowca,surowiec: Surowiec.Kamień,wartość: 1)
                },
                KolorKarty.Żółty
            ),
            new Karta(
                "Magazyn Drewna",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 3 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.ZmianaCenySurowca,surowiec: Surowiec.Drewno,wartość: 1)
                },
                KolorKarty.Żółty
            ),
            new Karta(
                "Magazyn Gliny",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 3 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.ZmianaCenySurowca,surowiec: Surowiec.Glina,wartość: 1)
                },
                KolorKarty.Żółty
            ),
            new Karta(
                "Skryptorium",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.Pismo),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Księga")
                },
                KolorKarty.Zielony
            ),
            new Karta(
                "Apteka",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Szkło, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.Koło),
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 1)
                },
                KolorKarty.Zielony
            ),
            new Karta(
                "Zielarnia",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.Moździerz),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Zębatka")
                },
                KolorKarty.Zielony
            ),
            new Karta(
                "Warsztat",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.Liczydło),
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 1)
                },
                KolorKarty.Zielony
            ),
            new Karta(
                "Garnizon",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartość: 1),
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
                    new Efekt(TypEfektu.PunktyMilitarne,wartość: 1),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Mur")
                },
                KolorKarty.Czerwony
            ),
            new Karta(
                "Wieża Strażnicza",
                Epoka.EpokaI,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartość: 1)
                },
                KolorKarty.Czerwony
            ),
            new Karta(
                "Stajnie",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartość: 1),
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
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 3),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Maska")
                },
                KolorKarty.Niebieski
            ),
            new Karta(
                "Łaźnie",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 3),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Kropla")
                },
                KolorKarty.Niebieski
            ),
            new Karta(
                "Ołtarz",
                Epoka.EpokaI,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 3),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Księżyc")
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
            "Kamieniołom Stokowy",
            Epoka.EpokaII,
            new Dictionary<Surowiec, int> { { Surowiec.Monety, 2 } },
            new List<Efekt>
            {
                new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Kamień, 2 } })
            },
            KolorKarty.Brązowy
            ),
            new Karta(
            "Cegielnia",
            Epoka.EpokaII,
            new Dictionary<Surowiec, int> { { Surowiec.Monety, 2 } },
            new List<Efekt>
            {
                new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 } })
            },
            KolorKarty.Brązowy
            ),
            new Karta(
            "Tartak",
            Epoka.EpokaII,
            new Dictionary<Surowiec, int> { { Surowiec.Monety, 2 } },
            new List<Efekt>
            {
                new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Drewno, 2 } })
            },
            KolorKarty.Brązowy
            ),
            new Karta(
                "Pracownia Wyrobu Szkła",
                Epoka.EpokaII,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Szkło, 1 } })
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
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 2 }, { Surowiec.Szkło, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.WyborSurowca, new Dictionary<Surowiec, int> { 
                        { Surowiec.Drewno, 1 }, { Surowiec.Glina, 1 }, { Surowiec.Kamień, 1 }
                    })
                },
                KolorKarty.Żółty
            ),
            new Karta(
                "Urząd celny",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 4 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.ZmianaCenySurowca,surowiec: Surowiec.Papirus,wartość: 1),
                    new Efekt(TypEfektu.ZmianaCenySurowca,surowiec: Surowiec.Szkło,wartość: 1)
                },
                KolorKarty.Żółty
            ),
            new Karta(
                "Browar",
                Epoka.EpokaII,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Monety,wartość: 6),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Beczka")
                },
                KolorKarty.Żółty
            ),
            new Karta(
                "Forum",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 3 }, { Surowiec.Glina, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.WyborSurowca, new Dictionary<Surowiec, int> {
                        { Surowiec.Szkło, 1 }, { Surowiec.Papirus, 1 }
                    })
                },
                KolorKarty.Żółty
            ),
            new Karta(
                "Labolatorium",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Szkło, 2 }, { Surowiec.Drewno, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.Liczydło),
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 1),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Lampa")
                },
                KolorKarty.Zielony
            ),
            new Karta(
                "Biblioteka",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 1 }, { Surowiec.Drewno, 1 }, { Surowiec.Szkło, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.Pismo),
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 2)
                },
                KolorKarty.Zielony,
                darmowaBudowa: "Księga"
            ),
            new Karta(
                "Szkoła",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 }, { Surowiec.Papirus, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.Koło),
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 1),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Harfa")
                },
                KolorKarty.Zielony
            ),
            new Karta(
                "Ambulatorium",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 }, { Surowiec.Kamień, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.Moździerz),
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 2)
                },
                KolorKarty.Zielony,
                darmowaBudowa: "Zębatka"
            ),
            new Karta(
                "Mury Obronne",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartość: 2)
                },
                KolorKarty.Czerwony
            ),
            new Karta(
                "Plac Apelowy",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 }, { Surowiec.Szkło, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartość: 2),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Hełm")
                },
                KolorKarty.Czerwony
            ),
            new Karta(
                "Koszary",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 3 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartość: 1)
                },
                KolorKarty.Czerwony,
                darmowaBudowa: "Miecz"
            ),
            new Karta(
                "Stadniny",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 }, { Surowiec.Drewno, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartość: 1)
                },
                KolorKarty.Czerwony,
                darmowaBudowa: "Podkowa"
            ),
            new Karta(
                "Tor Strzelecki",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 1 }, { Surowiec.Drewno, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartość: 2),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Strzelnica")
                },
                KolorKarty.Czerwony
            ),
            new Karta(
                "Gmach Sądu",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 2 }, { Surowiec.Szkło, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 5)
                },
                KolorKarty.Niebieski
            ),
            new Karta(
                "Akwedukt",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 3 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 5)
                },
                KolorKarty.Niebieski,
                darmowaBudowa: "Kropla"
            ),
            new Karta(
                "Mównica",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 1 }, { Surowiec.Drewno, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 4),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Sąd")
                },
                KolorKarty.Niebieski
            ),
            new Karta(
                "Świątynia",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 4),
                    new Efekt(TypEfektu.BialySymbol,tekst: "Słońce")
                },
                KolorKarty.Niebieski,
                darmowaBudowa: "Księżyc"
            ),
            new Karta(
                "Posąg",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 4),
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
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 2 }, { Surowiec.Drewno, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZaKarty, tekst: "Monety", wartość: 1)
                },
                KolorKarty.Fioletowy
            ),
            new Karta(
                "Cech Budowniczych",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 2 }, { Surowiec.Glina, 1 }, { Surowiec.Drewno, 1 }, { Surowiec.Szkło, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZaKarty, tekst: "Cuda", wartość: 2)
                },
                KolorKarty.Fioletowy
            ),
            new Karta(
                "Gildia Kupiecka",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 }, { Surowiec.Drewno, 1 }, { Surowiec.Szkło, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.MonetyZaKarty, tekst: "Żółty", wartość: 1),
                    new Efekt(TypEfektu.PunktyZaKarty, tekst: "Żółty", wartość: 1)
                },
                KolorKarty.Fioletowy
            ),
            new Karta(
                "Stowarzyszenie Urzędników",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 2 }, { Surowiec.Glina, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.MonetyZaKarty, tekst: "Niebieski", wartość: 1),
                    new Efekt(TypEfektu.PunktyZaKarty, tekst: "Niebieski", wartość: 1)
                },
                KolorKarty.Fioletowy
            ),
            new Karta(
                "Cech Armatorów",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 }, { Surowiec.Kamień, 1 }, { Surowiec.Szkło, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.MonetyZaKarty, tekst: "Brązowy i Szary", wartość: 1),
                    new Efekt(TypEfektu.PunktyZaKarty, tekst: "Brązowy i Szary", wartość: 1),
                },
                KolorKarty.Fioletowy
            ),
            new Karta(
                "Gildia Strategów",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 2 }, { Surowiec.Glina, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.MonetyZaKarty, tekst: "Czerwony", wartość: 1),
                    new Efekt(TypEfektu.PunktyZaKarty, tekst: "Czerwony", wartość: 1)
                },
                KolorKarty.Fioletowy
            ),
            new Karta(
                "Towarzystwo Naukowe",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 }, { Surowiec.Drewno, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.MonetyZaKarty, tekst: "Zielony", wartość: 1),
                    new Efekt(TypEfektu.PunktyZaKarty, tekst: "Zielony", wartość: 1)
                },
                KolorKarty.Fioletowy
            ),
        };
        var talia = new List<Karta>
        {
            new Karta(
                "Latarnia Morska",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 }, { Surowiec.Szkło, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 3),
                    new Efekt(TypEfektu.MonetyZaKarty, tekst: "Żółty", wartość: 1)
                },
                KolorKarty.Żółty,
                darmowaBudowa: "Wazon"
            ),
            new Karta(
                "Port",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 }, { Surowiec.Szkło, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 3),
                    new Efekt(TypEfektu.MonetyZaKarty, tekst: "Brązowy", wartość: 2)
                },
                KolorKarty.Żółty
            ),
            new Karta(
                "Arena",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 }, { Surowiec.Kamień, 1 }, { Surowiec.Drewno, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 3),
                    new Efekt(TypEfektu.MonetyZaKarty, tekst: "Cuda", wartość: 2)
                },
                KolorKarty.Żółty,
                darmowaBudowa: "Beczka"
            ),
            new Karta(
                "Izba Handlowa",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Papirus, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 3),
                    new Efekt(TypEfektu.MonetyZaKarty, tekst: "Szary", wartość: 3)
                },
                KolorKarty.Żółty
            ),
            new Karta(
                "Zbrojownia",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 2 }, { Surowiec.Szkło, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 3),
                    new Efekt(TypEfektu.MonetyZaKarty, tekst: "Czerwony", wartość: 1)
                },
                KolorKarty.Żółty
            ),
            new Karta(
                "Akademia",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 1 }, { Surowiec.Drewno, 1 }, { Surowiec.Szkło, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.ZegarSłoneczny),
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 3)
                },
                KolorKarty.Zielony
            ),
            new Karta(
                "Pracownia Naukowa",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 2 }, { Surowiec.Szkło, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.ZegarSłoneczny),
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 3)
                },
                KolorKarty.Zielony
            ),
            new Karta(
                "Uniwersytet",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 }, { Surowiec.Szkło, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.Globus),
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 2)
                },
                KolorKarty.Zielony,
                darmowaBudowa: "Harfa"
            ),
            new Karta(
                "Ambulatorium",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 1 }, { Surowiec.Papirus, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy,symbolNaukowy: SymbolNaukowy.Globus),
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 2)
                },
                KolorKarty.Zielony,
                darmowaBudowa: "Lampa"
            ),
            new Karta(
                "Cyrk",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 }, { Surowiec.Kamień, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartość: 2)
                },
                KolorKarty.Czerwony,
                darmowaBudowa: "Hełm"
            ),
            new Karta(
                "Fortyfikacje",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 2 }, { Surowiec.Glina, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartość: 2)
                },
                KolorKarty.Czerwony,
                darmowaBudowa: "Mur"
            ),
            new Karta(
                "Arsenał",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 3 }, { Surowiec.Drewno, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartość: 3)
                },
                KolorKarty.Czerwony
            ),
            new Karta(
                "Siedziba Trybuna",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 8 }},
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartość: 3)
                },
                KolorKarty.Czerwony
            ),
            new Karta(
                "Machiny Oblężnicze",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 3 }, { Surowiec.Szkło, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartość: 2)
                },
                KolorKarty.Czerwony,
                darmowaBudowa: "Strzelnica"
            ),
            new Karta(
                "Obelisk",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 2 }, { Surowiec.Szkło, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 5)
                },
                KolorKarty.Niebieski
            ),
            new Karta(
                "Budynek Senatu",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 }, { Surowiec.Kamień, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 5)
                },
                KolorKarty.Niebieski,
                darmowaBudowa: "Sąd"
            ),
            new Karta(
                "Mównica",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 3 }, { Surowiec.Drewno, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 7)
                },
                KolorKarty.Niebieski
            ),
            new Karta(
                "Pałac",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 }, { Surowiec.Kamień, 1 }, { Surowiec.Drewno, 1 }, { Surowiec.Szkło, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 7)
                },
                KolorKarty.Niebieski
            ),
            new Karta(
                "Panteon",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 }, { Surowiec.Drewno, 1 }, { Surowiec.Papirus, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 6)
                },
                KolorKarty.Niebieski,
                darmowaBudowa: "Słońce"
            ),
            new Karta(
                "Ogrody",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 }, { Surowiec.Drewno, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 6)
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
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 3 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 9)
                }
            ),
            new KartaCudu(
                "Wiszące Ogrody Semiramidy",
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 2 }, { Surowiec.Szkło, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Monety, wartość: 6),
                    new Efekt(TypEfektu.RozegrajTurePonownie),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 3)
                }
            ),
            new KartaCudu(
                "Świątynia Artemidy W Efezie",
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 }, { Surowiec.Kamień, 1 }, { Surowiec.Szkło, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Monety,wartość: 12),
                    new Efekt(TypEfektu.RozegrajTurePonownie)
                }
            ),
            new KartaCudu(
                "Sfinks",
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 1 }, { Surowiec.Glina, 1 }, { Surowiec.Szkło, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Monety,wartość: 6),
                    new Efekt(TypEfektu.RozegrajTurePonownie)
                }
            ),
            new KartaCudu(
                "Latarnia Morska Na Faros",
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 }, { Surowiec.Kamień, 1 }, { Surowiec.Papirus, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.WyborSurowca, surowce: new Dictionary<Surowiec, int>
                    {
                        { Surowiec.Drewno, 1 },
                        { Surowiec.Glina, 1 },
                        { Surowiec.Kamień, 1 }
                    }),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 4)
                }
            ),
            new KartaCudu(
                "Kolos Rodyjski",
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 3 }, { Surowiec.Szkło, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne, wartość: 2),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 3)
                }
            ),
            new KartaCudu(
                "Via Appia",
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 2 }, { Surowiec.Glina, 2 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Monety, wartość: 3),
                    new Efekt(TypEfektu.PrzeciwnikOdkladaMonety, wartość: 3),
                    new Efekt(TypEfektu.RozegrajTurePonownie),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 3)
                }
            ),
            new KartaCudu(
                "Pireus",
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 2 }, { Surowiec.Kamień, 1 }, { Surowiec.Glina, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.WyborSurowca, surowce: new Dictionary<Surowiec, int>
                    {
                        { Surowiec.Papirus, 1 },
                        { Surowiec.Szkło, 1 }
                    }),
                    new Efekt(TypEfektu.RozegrajTurePonownie),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 2)
                }
            ),
            new KartaCudu(
                "Mauzoleum W Halikarnasie",
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 }, { Surowiec.Szkło, 2 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.DarmowaBudowlaZOdrzuconychKart),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 2)
                }
            ),
            new KartaCudu(
                "Circus Maximus",
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 2 }, { Surowiec.Drewno, 1 }, { Surowiec.Szkło, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.OdlozKartePrzeciwnika, tekst: "Szary"),
                    new Efekt(TypEfektu.PunktyMilitarne, wartość: 1),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 3)
                }
            ),
            new KartaCudu(
                "Posąg Zeusa W Olimpii",
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 1 }, { Surowiec.Drewno, 1 }, { Surowiec.Glina, 1 }, { Surowiec.Papirus, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.OdlozKartePrzeciwnika, tekst: "Brązowy"),
                    new Efekt(TypEfektu.PunktyMilitarne, wartość: 1),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 3)
                }
            ),
            new KartaCudu(
                "Biblioteka Aleksandryjska",
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 3 }, { Surowiec.Szkło, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Wylosuj3ZetonyPostepu),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 4)
                }
            ),
        };
        return talia;
    }
}