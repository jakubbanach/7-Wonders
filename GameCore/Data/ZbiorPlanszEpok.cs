using System;
using System.Collections.Generic;
using System.Linq;

public static class ZbiorPlanszEpok
{
    public static PlanszaEpoki Utworz(Epoka epoka, List<Karta> talia, IRandom random)
    {
        return epoka switch
        {
            Epoka.EpokaI => InicjalizujPlanszeEpokiI(talia, random),
            Epoka.EpokaII => InicjalizujPlanszeEpokiII(talia, random),
            Epoka.EpokaIII => InicjalizujPlanszeEpokiIII(talia, random),
            _ => throw new ArgumentException("Nieznana epoka")
        };
    }
    private static PlanszaEpoki InicjalizujPlanszeEpokiI(List<Karta> talia, IRandom random)
    {
        talia.Shuffle(random);

        var karty = talia
            .Take(20)
            .ToList();

        foreach (var karta in karty)
        {
            karta.OznaczJakoNiezagrana();
        }

        // WARSTWA 1 - 6 odkrytych
        var a1 = new PoleKarty(karty[0], false);
        var a2 = new PoleKarty(karty[1], false);
        var a3 = new PoleKarty(karty[2], false);
        var a4 = new PoleKarty(karty[3], false);
        var a5 = new PoleKarty(karty[4], false);
        var a6 = new PoleKarty(karty[5], false);

        // WARSTWA 2 - 5 zakrytych
        var b1 = new PoleKarty(karty[6], true);
        var b2 = new PoleKarty(karty[7], true);
        var b3 = new PoleKarty(karty[8], true);
        var b4 = new PoleKarty(karty[9], true);
        var b5 = new PoleKarty(karty[10], true);

        //zaleznosci WARSTWA 2 -> WARSTWA 1
        b1.BlokujacePola.AddRange(new List<PoleKarty> { a1, a2 });
        b2.BlokujacePola.AddRange(new List<PoleKarty> { a2, a3 });
        b3.BlokujacePola.AddRange(new List<PoleKarty> { a3, a4 });
        b4.BlokujacePola.AddRange(new List<PoleKarty> { a4, a5 });
        b5.BlokujacePola.AddRange(new List<PoleKarty> { a5, a6 });

        // WARSTWA 3 - 4 odkryte
        var c1 = new PoleKarty(karty[11], false);
        var c2 = new PoleKarty(karty[12], false);
        var c3 = new PoleKarty(karty[13], false);
        var c4 = new PoleKarty(karty[14], false);

        //zaleznosci WARSTWA 3 -> WARSTWA 2
        c1.BlokujacePola.AddRange(new List<PoleKarty> { b1, b2 });
        c2.BlokujacePola.AddRange(new List<PoleKarty> { b2, b3 });
        c3.BlokujacePola.AddRange(new List<PoleKarty> { b3, b4 });
        c4.BlokujacePola.AddRange(new List<PoleKarty> { b4, b5 });

        // WARSTWA 4 - 3 zakryte
        var d1 = new PoleKarty(karty[15], true);
        var d2 = new PoleKarty(karty[16], true);
        var d3 = new PoleKarty(karty[17], true);

        //zaleznosci WARSTWA 4 -> WARSTWA 3
        d1.BlokujacePola.AddRange(new List<PoleKarty> { c1, c2 });
        d2.BlokujacePola.AddRange(new List<PoleKarty> { c2, c3 });
        d3.BlokujacePola.AddRange(new List<PoleKarty> { c3, c4 });

        // WARSTWA 5 - 2 odkryte
        var e1 = new PoleKarty(karty[18], false);
        var e2 = new PoleKarty(karty[19], false);

        //zaleznosci WARSTWA 5 -> WARSTWA 4
        e1.BlokujacePola.AddRange(new List<PoleKarty> { d1, d2 });
        e2.BlokujacePola.AddRange(new List<PoleKarty> { d2, d3 });

        var pola = new List<PoleKarty>()
        {
            a1,a2,a3,a4,a5,a6,
            b1,b2,b3,b4,b5,
            c1,c2,c3,c4,
            d1,d2,d3,
            e1,e2
        };

        return new PlanszaEpoki(Epoka.EpokaI)
        {
            Pola = pola
        };
    }

    private static PlanszaEpoki InicjalizujPlanszeEpokiII(List<Karta> talia, IRandom random)
    {
        talia.Shuffle(random);

        var karty = talia
            .Take(20)
            .ToList();

        foreach (var karta in karty)
        {
            karta.OznaczJakoNiezagrana();
        }
        // WARSTWA 1 - 2 odkryte
        var a1 = new PoleKarty(karty[0], false);
        var a2 = new PoleKarty(karty[1], false);

        // WARSTWA 2 - 3 zakryte
        var b1 = new PoleKarty(karty[2], true);
        var b2 = new PoleKarty(karty[3], true);
        var b3 = new PoleKarty(karty[4], true);

        //zaleznosci WARSTWA 2 -> WARSTWA 1
        b1.BlokujacePola.Add(a1);
        b2.BlokujacePola.AddRange(new List<PoleKarty> { a1, a2 });
        b3.BlokujacePola.Add(a2);

        // WARSTWA 3 - 4 odkryte
        var c1 = new PoleKarty(karty[5], false);
        var c2 = new PoleKarty(karty[6], false);
        var c3 = new PoleKarty(karty[7], false);
        var c4 = new PoleKarty(karty[8], false);

        //zaleznosci WARSTWA 3 -> WARSTWA 2
        c1.BlokujacePola.Add(b1);
        c2.BlokujacePola.AddRange(new List<PoleKarty> { b1, b2 });
        c3.BlokujacePola.AddRange(new List<PoleKarty> { b2, b3 });
        c4.BlokujacePola.Add(b3);

        // WARSTWA 4 - 5 zakrytych
        var d1 = new PoleKarty(karty[9], true);
        var d2 = new PoleKarty(karty[10], true);
        var d3 = new PoleKarty(karty[11], true);
        var d4 = new PoleKarty(karty[12], true);
        var d5 = new PoleKarty(karty[13], true);

        //zaleznosci WARSTWA 4 -> WARSTWA 3
        d1.BlokujacePola.Add(c1);
        d2.BlokujacePola.AddRange(new List<PoleKarty> { c1, c2 });
        d3.BlokujacePola.AddRange(new List<PoleKarty> { c2, c3 });
        d4.BlokujacePola.AddRange(new List<PoleKarty> { c3, c4 });
        d5.BlokujacePola.Add(c4);

        // WARSTWA 5 - 6 odkrytych
        var e1 = new PoleKarty(karty[14], false);
        var e2 = new PoleKarty(karty[15], false);
        var e3 = new PoleKarty(karty[16], false);
        var e4 = new PoleKarty(karty[17], false);
        var e5 = new PoleKarty(karty[18], false);
        var e6 = new PoleKarty(karty[19], false);

        //zaleznosci WARSTWA 5 -> WARSTWA 4
        e1.BlokujacePola.Add(d1);
        e2.BlokujacePola.AddRange(new List<PoleKarty> { d1, d2 });
        e3.BlokujacePola.AddRange(new List<PoleKarty> { d2, d3 });
        e4.BlokujacePola.AddRange(new List<PoleKarty> { d3, d4 });
        e5.BlokujacePola.AddRange(new List<PoleKarty> { d4, d5 });
        e6.BlokujacePola.Add(d5);


        var pola = new List<PoleKarty>
        {
            a1,a2,
            b1,b2,b3,
            c1,c2,c3,c4,
            d1,d2,d3,d4,d5,
            e1,e2,e3,e4,e5,e6
        };

        return new PlanszaEpoki(Epoka.EpokaII)
        {
            Pola = pola
        };
    }

    private static PlanszaEpoki InicjalizujPlanszeEpokiIII(List<Karta> talia, IRandom random)
    {
        // TODO: 3 karty Gildii + 17 kart z 3 epoki (razem 20 kart)
        talia.Shuffle(random);
        var kartyGildii = talia
            .Where(k => k.KolorKarty == KolorKarty.Fioletowy)
            .Take(3)
            .ToList();

        var kartyEpoki = talia
            .Where(k => k.KolorKarty != KolorKarty.Fioletowy)
            .Take(17)
            .ToList();

        var karty = kartyGildii.Concat(kartyEpoki).ToList();
        karty.Shuffle(random);

        foreach (var karta in karty)
        {
            karta.OznaczJakoNiezagrana();
        }

        //var kartyGildii

        // WARSTWA 1 - 2 odkryte
        var a1 = new PoleKarty(karty[0], false);
        var a2 = new PoleKarty(karty[1], false);

        // WARSTWA 2 - 3 zakryte
        var b1 = new PoleKarty(karty[2], true);
        var b2 = new PoleKarty(karty[3], true);
        var b3 = new PoleKarty(karty[4], true);

        //zaleznosci WARSTWA 2 -> WARSTWA 1
        b1.BlokujacePola.Add(a1);
        b2.BlokujacePola.AddRange(new List<PoleKarty> { a1, a2 });
        b3.BlokujacePola.Add(a2);

        // WARSTWA 3 - 4 odkryte
        var c1 = new PoleKarty(karty[5], false);
        var c2 = new PoleKarty(karty[6], false);
        var c3 = new PoleKarty(karty[7], false);
        var c4 = new PoleKarty(karty[8], false);

        //zaleznosci WARSTWA 3 -> WARSTWA 2
        c1.BlokujacePola.Add(b1);
        c2.BlokujacePola.AddRange(new List<PoleKarty> { b1, b2 });
        c3.BlokujacePola.AddRange(new List<PoleKarty> { b2, b3 });
        c4.BlokujacePola.Add(b3);

        // WARSTWA 4 - 2 zakryte
        var d1 = new PoleKarty(karty[9], true);
        var d3 = new PoleKarty(karty[10], true);

        //zaleznosci WARSTWA 4 -> WARSTWA 3
        d1.BlokujacePola.AddRange(new List<PoleKarty> { c1, c2 });
        d3.BlokujacePola.AddRange(new List<PoleKarty> { c3, c4 });

        // WARSTWA 5 - 4 odkryte
        var e1 = new PoleKarty(karty[11], false);
        var e2 = new PoleKarty(karty[12], false);
        var e3 = new PoleKarty(karty[13], false);
        var e4 = new PoleKarty(karty[14], false);

        //zale�no�ci WARSTWA 5 -> WARSTWA 4
        e1.BlokujacePola.Add(d1);
        e2.BlokujacePola.Add(d1);
        e3.BlokujacePola.Add(d3);
        e4.BlokujacePola.Add(d3);

        // WARSTWA 6 - 3 zakryte
        var f1 = new PoleKarty(karty[15], true);
        var f2 = new PoleKarty(karty[16], true);
        var f3 = new PoleKarty(karty[17], true);

        //zaleznosci WARSTWA 6 -> WARSTWA 5
        f1.BlokujacePola.AddRange(new List<PoleKarty> { e1, e2 });
        f2.BlokujacePola.AddRange(new List<PoleKarty> { e2, e3 });
        f3.BlokujacePola.AddRange(new List<PoleKarty> { e3, e4 });

        // WARSTWA 7 - 2 odkryte
        var g1 = new PoleKarty(karty[18], false);
        var g2 = new PoleKarty(karty[19], false);

        //zaleznosci WARSTWA 7 -> WARSTWA 6
        g1.BlokujacePola.AddRange(new List<PoleKarty> { f1, f2 });
        g2.BlokujacePola.AddRange(new List<PoleKarty> { f2, f3 });

        var pola = new List<PoleKarty>()
        {
            a1,a2,
            b1,b2,b3,
            c1,c2,c3,c4,
            d1,d3,
            e1,e2,e3,e4,
            f1,f2,f3,
            g1,g2
        };

        return new PlanszaEpoki(Epoka.EpokaIII)
        {
            Pola = pola
        };
    }

    public static void WypiszPlansze(PlanszaEpoki plansza)
    {
        int maxSzerokosc = 11;

        int[] uklad = plansza.Epoka switch
        {
            Epoka.EpokaI => new[] { 6, 5, 4, 3, 2 },
            Epoka.EpokaII => new[] { 2, 3, 4, 5, 6 },
            Epoka.EpokaIII => new[] { 2, 3, 4, 2, 4, 3, 2 },
            _ => throw new ArgumentOutOfRangeException()
        };

        int index = 0;

        foreach (int liczbaPol in uklad)
        {
            int szerokoscWiersza = liczbaPol * 2;
            int paddingLewy = (maxSzerokosc - szerokoscWiersza) / 2;

            Console.Write(new string(' ', paddingLewy));

            for (int i = 0; i < liczbaPol; i++)
            {
                var pole = plansza.Pola[index++];

                string symbol =
                    pole.CzyDostepna ? "D" :
                    pole.CzyZakryta ? "X" :
                    "O";
                if(pole.Karta == null)
                {
                    symbol = "-";
                }

                Console.Write(symbol + " ");
            }

            Console.WriteLine();
        }
    }

    public static IEnumerable<Karta> Tasuj(List<Karta> talia, Random rng)
    {
        return talia.OrderBy(_ => rng.Next()).ToList();
    }

    public static string PlanszaDoStringa(PlanszaEpoki plansza)
        {
        var wynik = new System.Text.StringBuilder();
        int maxSzerokosc = 11;
        int[] uklad = plansza.Epoka switch
        {
            Epoka.EpokaI => new[] { 6, 5, 4, 3, 2 },
            Epoka.EpokaII => new[] { 2, 3, 4, 5, 6 },
            Epoka.EpokaIII => new[] { 2, 3, 4, 2, 4, 3, 2 },
            _ => throw new ArgumentOutOfRangeException()
        };
        int index = 0;
        foreach (int liczbaPol in uklad)
        {
            int szerokoscWiersza = liczbaPol * 2;
            int paddingLewy = (maxSzerokosc - szerokoscWiersza) / 2;
            wynik.Append(new string(' ', paddingLewy));
            for (int i = 0; i < liczbaPol; i++)
            {
                var pole = plansza.Pola[index++];
                string symbol =
                    pole.CzyDostepna ? "D" :
                    pole.CzyZakryta ? "X" :
                    "O";
                if (pole.Karta == null)
                {
                    symbol = "-";
                }
                wynik.Append(symbol + " ");
            }
            wynik.AppendLine();
        }

        wynik.AppendLine("[D] Dost�pna | [X] Zakryta | [O] Odkryta | [-] Puste pole");
        return wynik.ToString();
    }

}