using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
public class GameRunner
{
    private readonly Random random;

    public GameRunner(int seed)
    {
        random = new Random(seed);
    }
    public MatchResult PlayGame(IAgent a1, IAgent a2)
    {
        int seed = random.Next();
        // TODO: dodac seed do Gra.StworzNowaGre() i przekazywac go do MatchResult.FromGame() zeby mozna bylo odtworzyc gre
        var gra = Gra.StworzNowaGre();

        var log = new List<MoveLog>();

        while (!gra.StanGry.CzyZakonczona)
        {
            Console.WriteLine($"\nAktywny gracz: {gra.AktywnyGracz.Nazwa}, Epoka: {gra.Epoka}, Pozycja konfliktu: {gra.PozycjaKonfliktu}");
            var currentAgent = gra.AktywnyGracz == gra.Gracze[0] ? a1 : a2;

            var ruch = currentAgent.DecideMove(gra.Clone());

            log.Add(new MoveLog(currentAgent.Name, ruch));

            Console.WriteLine($"Agent {currentAgent.Name} wykonuje ruch: {ruch.TypRuchu} z kartą {ruch.KartaDoZagrania.Nazwa ?? "Brak karty"}");

            // karta z clone() jest inna instancja niz karta z gra.DostepneKarty(), wiec trzeba porownywac po nazwie, a nie po 

            gra.WykonajRuch(ruch);
        }

        return MatchResult.FromGame(gra, a1, a2, log, seed);
    }
    public MatchResult ReplayGame(int seed, IAgent a1, IAgent a2)
        { return PlayGame(a1, a2); }

}
