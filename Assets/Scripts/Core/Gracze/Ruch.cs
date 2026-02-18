//using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
public class Ruch
{
    public Gracz Gracz { get; private set; }
    public Gracz Przeciwnik { get; private set; }
    public Karta KartaDoZagrania { get; private set; }
    public TypRuchu TypRuchu { get; private set; }

    public Ruch(Gracz gracz, Gracz przeciwnik, Karta kartaDoZagrania, TypRuchu typRuchu)
    {
        Gracz = gracz;
        Przeciwnik = przeciwnik;
        KartaDoZagrania = kartaDoZagrania;
        TypRuchu = typRuchu;
    }

    public void Wykonaj(PlanszaKonfliktu? planszaKonfliktu = null)
    {
        switch (TypRuchu)
        {
            case TypRuchu.ZbudujKarte:
                Gracz.ZbudujKarte(KartaDoZagrania, Przeciwnik, planszaKonfliktu);
                break;
            case TypRuchu.OdrzucKarte:
                Gracz.OdrzucKarte(KartaDoZagrania);
                break;
            case TypRuchu.ZbudujCud:
                //tutaj zaimplementowac wybor karty cudu
                KartaCudu Cud = Gracz.KartyCudow[0]; //Gracz.WybierzNiezbudowanyCud();
                Gracz.ZbudujCud(KartaDoZagrania, Przeciwnik, Cud, planszaKonfliktu);
                break;
            default:
                Console.WriteLine("Nieznany typ ruchu.");
                break;
        }
    }
}
