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

    public void Wykonaj()
    {
        switch (TypRuchu)
        {
            case TypRuchu.WznoszenieBudowli:
                Gracz.ZbudujKarte(KartaDoZagrania);
                break;
            case TypRuchu.OdrzucenieKarty:
                Gracz.DodajMonety(2);
                //int liczbaZoltychKart = 0;
                //liczenie zoltych kart gracza dajacych monety za odrzucenie karty
                //var liczbaZoltychKart = Gracz.PobierzDostepneKarty().Count(karta => karta.KolorKarty == KolorKarty.¯ó³ty);
                //Gracz.DodajMonety(liczbaZoltychKart);
                break;
            case TypRuchu.BudowaCudu:
                //tutaj zaimplementowac wybor karty cudu
                KartaCudu Cud = Gracz.KartyCudow[0];
                Gracz.ZbudujKarteCudu(Cud);
                //usuwanie karty budynku po zbudowaniu cudu (chowanie pod karte cudu)
                //Talia.UsunKarte(KartaDoZagrania); //talia kart budynkow
                break;
            default:
                Console.WriteLine("Nieznany typ ruchu.");
                break;
        }
    }
}
