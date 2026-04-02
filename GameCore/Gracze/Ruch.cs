//using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
public class Ruch
{
    public Gracz Gracz { get; private set; }
    public Gracz Przeciwnik { get; private set; }
    public Karta KartaDoZagrania { get; private set; }
    public KartaCudu? KartaCudu { get; private set; }
    public TypRuchu TypRuchu { get; private set; }

    public Ruch(Gracz gracz, Gracz przeciwnik, Karta kartaDoZagrania, TypRuchu typRuchu, KartaCudu? kartaCudu = null)
    {
        Gracz = gracz;
        Przeciwnik = przeciwnik;
        KartaDoZagrania = kartaDoZagrania;
        TypRuchu = typRuchu;
        KartaCudu = kartaCudu;
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
                // TODO: Implementacja wyboru karty cudu
                //KartaCudu Cud = Gracz.KartyCudow[0]; //Gracz.WybierzNiezbudowanyCud();
                if (KartaCudu == null)
                {
                    Console.WriteLine("Nie mozna zbudowac cudu bez wybranej karty cudu.");
                    return;
                }
                if (Gracz.PobierzKartyCudu().FirstOrDefault(c => c.Nazwa == KartaCudu.Nazwa) == null)
                {
                    Console.WriteLine($"Gracz nie posiada karty cudu: {KartaCudu.Nazwa}");
                    return;
                }
                Gracz.ZbudujCud(KartaDoZagrania, Przeciwnik, KartaCudu, planszaKonfliktu);
                break;
            default:
                Console.WriteLine("Nieznany typ ruchu.");
                break;
        }
    }
}
