using System;
using System.Collections.Generic;
using System.Text;
public class MatchResult
{
    public Guid MatchId { get; }
    public int Seed { get; }
    public string Agent1Name { get; }
    public string Agent2Name { get; }
    public IAgent? Agent1 { get; }
    public IAgent? Agent2 { get; }

    public string? Winner { get; }
    public TypZwyciestwa TypZwyciestwa { get; }

    public int Turns { get; }

    public int Agent1Score { get; private set; }
    public int Agent2Score { get; private set; }
    // TODO: Duże pola - można pominąć dla --minimal-logs
    public List<Karta>? Agent1Cards { get; private set; }
    public List<KartaCudu>? Agent1BuildWonders { get; private set; }
    public Dictionary<Surowiec, int>? Agent1Resources { get; private set; }
    public List<Karta>? Agent2Cards { get; private set; }
    public List<KartaCudu>? Agent2BuildWonders { get; private set; }
    public Dictionary<Surowiec, int>? Agent2Resources { get; private set; }
    public IReadOnlyList<MoveLog>? Moves { get; private set; }

    public MatchResult(
        Guid matchId,
        int seed,
        string agent1,
        string agent2,
        IAgent? agent1Instance,
        IAgent? agent2Instance,
        string? winner,
        TypZwyciestwa typZwyciestwa,
        int turns,
        int score1,
        int score2,
        List<Karta>? agent1Cards,
        List<KartaCudu>? agent1BuildWonders,
        Dictionary<Surowiec, int>? agent1Resources,
        List<Karta>? agent2Cards,
        List<KartaCudu>? agent2BuildWonders,
        Dictionary<Surowiec, int>? agent2Resources,
        List<MoveLog> moves)
    {
        MatchId = matchId;
        Seed = seed;
        Agent1Name = agent1;
        Agent2Name = agent2;
        Agent1 = agent1Instance;
        Agent2 = agent2Instance;
        Winner = winner;
        TypZwyciestwa = typZwyciestwa;
        Turns = turns;
        Agent1Score = score1;
        Agent2Score = score2;
        Agent1Cards = agent1Cards;
        Agent1BuildWonders = agent1BuildWonders;
        Agent1Resources = agent1Resources;
        Agent2Cards = agent2Cards;
        Agent2BuildWonders = agent2BuildWonders;
        Agent2Resources = agent2Resources;
        Moves = moves;
    }


    public static MatchResult FromGame(Gra gra, IAgent a1, IAgent a2, List<MoveLog> moves, int seed, bool minimal = false)
    {
        var stan = gra.StanGry;
        string? winner = stan.Zwyciezca == null ? null : stan.Zwyciezca == gra.Gracze[0] ? a1.Name : a2.Name;

        return new MatchResult(
            Guid.NewGuid(),
            seed,
            a1.Name,
            a2.Name,
            a1,
            a2,
            winner,
            stan.TypZwyciestwa,
            moves.Count,
            stan.GetPunktyGracza1(),
            stan.GetPunktyGracza2(),  
            minimal ? null : gra.Gracze[0].PobierzZbudowaneKarty(),
            minimal ? null : gra.Gracze[0].PobierzZbudowaneKartyCudow(),
            minimal ? null : gra.Gracze[0].Surowce,
            minimal ? null : gra.Gracze[1].PobierzZbudowaneKarty(),
            minimal ? null : gra.Gracze[1].PobierzZbudowaneKartyCudow(),
            minimal ? null : gra.Gracze[1].Surowce,
            moves
        );
    }

    /// <summary>
    /// Usuwa duże pola (karty, cuda, surowce, decyzje) z MatchResult i jego MoveLog,
    /// żeby zmniejszyć rozmiar pliku treningowego dla dużych zbiorów (1000+ gier).
    /// </summary>
    public void MinimalizeForTraining()
    {
        Agent1Cards = null;
        Agent1BuildWonders = null;
        Agent1Resources = null;
        Agent2Cards = null;
        Agent2BuildWonders = null;
        Agent2Resources = null;

        if (Moves != null)
        {
            foreach (var move in Moves)
            {
                move.Karta = null;
                move.KartaCudu = null;
                move.SurowceGracza = null;
                move.SurowcePrzeciwnika = null;
                move.TypRuchu = null;
                move.KartyDoWyboru = null;
                move.Decisions = null;
            }
        }
    }
}
