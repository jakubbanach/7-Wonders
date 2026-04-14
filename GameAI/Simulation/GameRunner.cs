using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
public class GameRunner
{
    public int Seed { get; }

    private readonly IRandom gameRandom;
    private readonly IRandom agent1Random;
    private readonly IRandom agent2Random;

    private readonly Func<IRandom, IAgent> agent1Factory;
    private readonly Func<IRandom, IAgent> agent2Factory;

    public GameRunner(
        int seed,
        Func<IRandom, IAgent> agent1Factory,
        Func<IRandom, IAgent> agent2Factory)
    {
        Seed = seed;
        var master = new Random(seed);

        gameRandom = new RandomAdapter(master.Next());
        agent1Random = new RandomAdapter(master.Next());
        agent2Random = new RandomAdapter(master.Next());

        this.agent1Factory = agent1Factory; 
        this.agent2Factory = agent2Factory;
    }
    public MatchResult PlayGame()
    {
        var a1 = agent1Factory(agent1Random);
        var a2 = agent2Factory(agent2Random);

        // TODO: dodac seed do Gra.StworzNowaGre() i przekazywac go do MatchResult.FromGame() zeby mozna bylo odtworzyc gre
        var gra = Gra.StworzNowaGre(random: gameRandom);
        a1.Name = gra.Gracze[0].Nazwa;
        a2.Name = gra.Gracze[1].Nazwa;

        var log = new List<MoveLog>();

        while (!gra.StanGry.CzyZakonczona)
        {
            //Console.WriteLine($"\nAktywny gracz: {gra.AktywnyGracz.Nazwa}, Epoka: {gra.Epoka}, Pozycja konfliktu: {gra.PozycjaKonfliktu}");
            var currentAgent = gra.AktywnyGracz == gra.Gracze[0] ? a1 : a2;
            

            var ruch = currentAgent.WybierzRuch(gra.Clone());

            // ewenetualnie currentAgent
            log.Add(new MoveLog(gra.AktywnyGracz.Nazwa, ruch));

            //Console.WriteLine($"Agent {gra.AktywnyGracz.Nazwa} wykonuje ruch: {ruch.TypRuchu} z karta {ruch.KartaDoZagrania.Nazwa ?? "Brak karty"}");

            var resolver = new AgentDecisionResolver(currentAgent);
            gra.WykonajRuch(ruch, resolver, gameRandom);
        }

        return MatchResult.FromGame(gra, a1, a2, log, Seed);
    }
    public MatchResult ReplayGame()
    {
        var replayRunner = new GameRunner(
            Seed,
            agent1Factory,
            agent2Factory
        );

        return replayRunner.PlayGame();
    }

}
