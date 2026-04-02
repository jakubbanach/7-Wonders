using System.Collections.Generic;
using System.Linq;

public static class ZbiorZetonowPostepu
{
    public static IReadOnlyList<ZetonPostepu> ZetonyPostepu { get; }

    static ZbiorZetonowPostepu()
    {
        ZetonyPostepu = InicjalizujZetonyPostepu();
    }

    private static List<ZetonPostepu> InicjalizujZetonyPostepu()
    {
        var zetony = new List<ZetonPostepu>
        {
            new ZetonPostepu(
                "Rolnictwo",
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Monety, wartosc: 6),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartosc: 4),
                }
            ),
            new ZetonPostepu(
                "Filozofia",
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartosc: 7)
                }
            ),
            new ZetonPostepu(
                "Prawo",
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy, symbolNaukowy:SymbolNaukowy.Waga)
                }
            ),
            new ZetonPostepu(
                "Strategia",
                new List<Efekt>
                {
                    new Efekt(TypEfektu.DodatkoweMilitariaZaCzerwoneKarty)
                }
            ),
            new ZetonPostepu(
                "Matematyka",
                new List<Efekt>
                {
                    new Efekt(TypEfektu.KoniecGry3PunktyZaZetonPostepu)
                }
            ),
            new ZetonPostepu(
                "Architektura",
                new List<Efekt>
                {
                    new Efekt(TypEfektu.MniejMaterialowNaCuda, wartosc: 2)
                }
            ),
            new ZetonPostepu(
                "Budownictwo",
                new List<Efekt>
                {
                    new Efekt(TypEfektu.MniejMaterialowNaNiebieskieKarty, wartosc: 2)
                }
            ),
            new ZetonPostepu(
                "Urbanistyka",
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Monety, wartosc: 6),
                    new Efekt(TypEfektu.MonetyZaBudoweZBialymSymbolem, wartosc: 4),
                }
            ),
            new ZetonPostepu(
                "Ekonomia",
                new List<Efekt>
                {
                    new Efekt(TypEfektu.MonetyPrzeciwnikaZaMaterialy)
                }
            ),
            new ZetonPostepu(
                "Teologia",
                new List<Efekt>
                {
                    new Efekt(TypEfektu.ZaBudoweCuduRozegrajTurePonownie)
                }
            ) 
        };
        return zetony;
    }
   }