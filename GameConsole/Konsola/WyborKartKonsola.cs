using System;
using System.Collections.Generic;

public class WyborKartKonsola : IWyborKarty
{
    public int Wybierz(List<PoleKarty> dostepne)
    {
        if (dostepne.Count == 0)
        {
            throw new InvalidOperationException("Brak dostepnych kart do wyboru.");
        }
        while (true)
        {
            Console.WriteLine("Dostepne karty:");
            int index = 0;
            foreach (var pole in dostepne)
            {
                var karta = pole.Karta;
                if (karta == null)
                    continue;

                Console.WriteLine($"{index}: {karta.WypiszOpis()}");
                index++;
            }

            Console.Write("Wybierz karte: ");
            if (int.TryParse(Console.ReadLine(), out int wybor) &&
                wybor >= 0 && wybor < dostepne.Count)
            {
                return wybor;
            }

            Console.WriteLine("Nieprawidlowy wybor.");
        }
    }
}
