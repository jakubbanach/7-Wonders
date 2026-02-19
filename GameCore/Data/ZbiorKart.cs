using System.Collections.Generic;
using System.Linq;

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
            new(
                "Wycinka",
                Epoka.EpokaI,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 } })
                },
                KolorKarty.Brązowy
            ),
            new(
                "Złoża Gliny",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 } })
                },
                KolorKarty.Brązowy
            ),
            new(
                "Składowisko Kamienia",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Kamień, 1 } })
                },
                KolorKarty.Brązowy
            ),
            new(
                "Glinianka",
                Epoka.EpokaI,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 } })
                },
                KolorKarty.Brązowy
            ),
            new(
                "Skład Drewna",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 } })
                },
                KolorKarty.Brązowy
            ),
            new(
                "Kamieniołom",
                Epoka.EpokaI,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Kamień, 1 } })
                },
                KolorKarty.Brązowy
            ),
            new(
                "Huta Szkła",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Szkło, 1 } })
                },
                KolorKarty.Szary
            ),
            new(
                "Wytwórnia Papirusu",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Papirus, 1 } })
                },
                KolorKarty.Szary
            ),
            new(
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
            new(
                "Magazyn Kamienia",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 3 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.ZmianaCenySurowca,surowiec: Surowiec.Kamień,wartość: 1)
                },
                KolorKarty.Żółty
            ),
            new(
                "Magazyn Drewna",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 3 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.ZmianaCenySurowca,surowiec: Surowiec.Drewno,wartość: 1)
                },
                KolorKarty.Żółty
            ),
            new(
                "Magazyn Gliny",
                Epoka.EpokaI,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 3 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.ZmianaCenySurowca,surowiec: Surowiec.Glina,wartość: 1)
                },
                KolorKarty.Żółty
            ),
            new(
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
            new(
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
            new(
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
            new(
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
            new(
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
            new(
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
            new(
                "Wieża Strażnicza",
                Epoka.EpokaI,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartość: 1)
                },
                KolorKarty.Czerwony
            ),
            new(
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
            new(
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
            new(
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
            new(
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
            new(
            "Kamieniołom Stokowy",
            Epoka.EpokaII,
            new Dictionary<Surowiec, int> { { Surowiec.Monety, 2 } },
            new List<Efekt>
            {
                new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Kamień, 2 } })
            },
            KolorKarty.Brązowy
            ),
            new(
            "Cegielnia",
            Epoka.EpokaII,
            new Dictionary<Surowiec, int> { { Surowiec.Monety, 2 } },
            new List<Efekt>
            {
                new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 } })
            },
            KolorKarty.Brązowy
            ),
            new(
            "Tartak",
            Epoka.EpokaII,
            new Dictionary<Surowiec, int> { { Surowiec.Monety, 2 } },
            new List<Efekt>
            {
                new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Drewno, 2 } })
            },
            KolorKarty.Brązowy
            ),
            new(
                "Pracownia Wyrobu Szkła",
                Epoka.EpokaII,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Szkło, 1 } })
                },
                KolorKarty.Szary
            ),
            new(
                "Suszarnia Papirusu",
                Epoka.EpokaII,
                null,
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Surowiec,new Dictionary<Surowiec, int> { { Surowiec.Papirus, 1 } })
                },
                KolorKarty.Szary
            ),
            new(
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
            new(
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
            new(
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
            new(
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
            new(
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
            new(
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
            new(
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
            new(
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
            new(
                "Mury Obronne",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartość: 2)
                },
                KolorKarty.Czerwony
            ),
            new(
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
            new(
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
            new(
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
            new(
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
            new(
                "Gmach Sądu",
                Epoka.EpokaII,
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 2 }, { Surowiec.Szkło, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 5)
                },
                KolorKarty.Niebieski
            ),
            new(
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
            new(
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
            new(
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
            new(
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
            new(
                "Cech Lichwiarzy",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 2 }, { Surowiec.Drewno, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZaKarty, tekst: "Monety", wartość: 1)
                },
                KolorKarty.Fioletowy
            ),
            new(
                "Cech Budowniczych",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 2 }, { Surowiec.Glina, 1 }, { Surowiec.Drewno, 1 }, { Surowiec.Szkło, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZaKarty, tekst: "Cuda", wartość: 2)
                },
                KolorKarty.Fioletowy
            ),
            new(
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
            new(
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
            new(
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
            new(
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
            new(
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
            new(
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
            new(
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
            new(
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
            new(
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
            new(
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
            new(
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
            new(
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
            new(
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
            new(
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
            new(
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
            new(
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
            new(
                "Arsenał",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 3 }, { Surowiec.Drewno, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartość: 3)
                },
                KolorKarty.Czerwony
            ),
            new(
                "Siedziba Trybuna",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Monety, 8 }},
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne,wartość: 3)
                },
                KolorKarty.Czerwony
            ),
            new(
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
            new(
                "Obelisk",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 2 }, { Surowiec.Szkło, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 5)
                },
                KolorKarty.Niebieski
            ),
            new(
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
            new(
                "Mównica",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 3 }, { Surowiec.Drewno, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 7)
                },
                KolorKarty.Niebieski
            ),
            new(
                "Pałac",
                Epoka.EpokaIII,
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 1 }, { Surowiec.Kamień, 1 }, { Surowiec.Drewno, 1 }, { Surowiec.Szkło, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa,wartość: 7)
                },
                KolorKarty.Niebieski
            ),
            new(
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
            new(
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
            new(
                "Piramida Cheopsa",
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 3 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 9)
                }
            ),
            new(
                "Wiszące Ogrody Semiramidy",
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 2 }, { Surowiec.Szkło, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Monety, wartość: 6),
                    new Efekt(TypEfektu.RozegrajTurePonownie),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 3)
                }
            ),
            new(
                "Świątynia Artemidy W Efezie",
                new Dictionary<Surowiec, int> { { Surowiec.Drewno, 1 }, { Surowiec.Kamień, 1 }, { Surowiec.Szkło, 1 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Monety,wartość: 12),
                    new Efekt(TypEfektu.RozegrajTurePonownie)
                }
            ),
            new(
                "Sfinks",
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 1 }, { Surowiec.Glina, 1 }, { Surowiec.Szkło, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Monety,wartość: 6),
                    new Efekt(TypEfektu.RozegrajTurePonownie)
                }
            ),
            new(
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
            new(
                "Kolos Rodyjski",
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 3 }, { Surowiec.Szkło, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyMilitarne, wartość: 2),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 3)
                }
            ),
            new(
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
            new(
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
            new(
                "Mauzoleum W Halikarnasie",
                new Dictionary<Surowiec, int> { { Surowiec.Glina, 2 }, { Surowiec.Szkło, 2 }, { Surowiec.Papirus, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.DarmowaBudowlaZOdrzuconychKart),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 2)
                }
            ),
            new(
                "Circus Maximus",
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 2 }, { Surowiec.Drewno, 1 }, { Surowiec.Szkło, 1 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.OdlozKartePrzeciwnika, tekst: "Szary"),
                    new Efekt(TypEfektu.PunktyMilitarne, wartość: 1),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 3)
                }
            ),
            new(
                "Posąg Zeusa W Olimpii",
                new Dictionary<Surowiec, int> { { Surowiec.Kamień, 1 }, { Surowiec.Drewno, 1 }, { Surowiec.Glina, 1 }, { Surowiec.Papirus, 2 } },
                new List<Efekt>
                {
                    new Efekt(TypEfektu.OdlozKartePrzeciwnika, tekst: "Brązowy"),
                    new Efekt(TypEfektu.PunktyMilitarne, wartość: 1),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartość: 3)
                }
            ),
            new(
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