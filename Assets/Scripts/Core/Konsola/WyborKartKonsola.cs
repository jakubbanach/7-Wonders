using System;
using System.Collections.Generic;

public class WyborKartKonsola : IWyborKarty
{
    public int Wybierz(List<PoleKarty> dostepne)
    {
        if (dostepne.Count == 0)
        {
            throw new InvalidOperationException("Brak dostêpnych kart do wyboru.");
        }
        while (true)
        {
            Console.WriteLine("Dostêpne karty:");
            for (int i = 0; i < dostepne.Count; i++)
            {
                Console.WriteLine($"{i}: {dostepne[i].Karta!.Nazwa} (Koszt: {dostepne[i].Karta.WypiszKoszt()})");
            }

            Console.Write("Wybierz kartê: ");
            if (int.TryParse(Console.ReadLine(), out int wybor) &&
                wybor >= 0 && wybor < dostepne.Count)
            {
                return wybor;
            }

            Console.WriteLine("Nieprawid³owy wybór.");
        }
    }
}
