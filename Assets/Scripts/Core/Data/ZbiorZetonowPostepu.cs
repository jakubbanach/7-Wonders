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
            new(
                "Rolnictwo",
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Monety, wartoœæ: 6),
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartoœæ: 4),
                }
            ),
            new(
                "Filozofia",
                new List<Efekt>
                {
                    new Efekt(TypEfektu.PunktyZwyciestwa, wartoœæ: 7)
                }
            ),
            new(
                "Prawo",
                new List<Efekt>
                {
                    new Efekt(TypEfektu.SymbolNaukowy, symbolNaukowy:SymbolNaukowy.Waga)
                }
            ),
            new(
                "Strategia",
                new List<Efekt>
                {
                    new Efekt(TypEfektu.DodatkoweMilitariaZaCzerwoneKarty)
                }
            ),
            new(
                "Matematyka",
                new List<Efekt>
                {
                    new Efekt(TypEfektu.KoniecGry3PunktyZaZetonPostepu)
                }
            ),
            new(
                "Architektura",
                new List<Efekt>
                {
                    new Efekt(TypEfektu.MniejMaterialowNaCuda, wartoœæ: 2)
                }
            ),
            new(
                "Budownictwo",
                new List<Efekt>
                {
                    new Efekt(TypEfektu.MniejMaterialowNaNiebieskieKarty, wartoœæ: 2)
                }
            ),
            new(
                "Urbanistyka",
                new List<Efekt>
                {
                    new Efekt(TypEfektu.Monety, wartoœæ: 6),
                    new Efekt(TypEfektu.MonetyZaBudoweZBialymSymbolem, wartoœæ: 4),
                }
            ),
            new(
                "Ekonomia",
                new List<Efekt>
                {
                    new Efekt(TypEfektu.MonetyPrzeciwnikaZaMaterialy)
                }
            ),
            new(
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