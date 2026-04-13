using System;
using System.Collections.Generic;
using System.Text;
public class MatchResult
{
    public Guid MatchId { get; }
    public int Seed { get; }
    public string Agent1Name { get; }
    public string Agent2Name { get; }

    public string? Winner { get; }
    public TypZwyciestwa TypZwyciestwa { get; }

    public int Turns { get; }

    public int Agent1Score { get; }
    public int Agent2Score { get; }
    public List<Karta> Agent1Cards { get; }
    public List<KartaCudu> Agent1BuildWonders { get; }
    public Dictionary<Surowiec, int> Agent1Resources { get; }
    public List<Karta> Agent2Cards { get; }
    public List<KartaCudu> Agent2BuildWonders { get; }
    public Dictionary<Surowiec, int> Agent2Resources { get; }
    public IReadOnlyList<MoveLog> Moves { get; }

    public MatchResult(
        Guid matchId,
        int seed,
        string agent1,
        string agent2,
        string? winner,
        TypZwyciestwa typZwyciestwa,
        int turns,
        int score1,
        int score2,
        List<Karta> agent1Cards,
        List<KartaCudu> agent1BuildWonders,
        Dictionary<Surowiec, int> agent1Resources,
        List<Karta> agent2Cards,
        List<KartaCudu> agent2BuildWonders,
        Dictionary<Surowiec, int> agent2Resources,
    List<MoveLog> moves)
    {
        MatchId = matchId;
        Seed = seed;
        Agent1Name = agent1;
        Agent2Name = agent2;
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


    public static MatchResult FromGame(Gra gra, IAgent a1, IAgent a2, List<MoveLog> moves, int seed)
    {
        var stan = gra.StanGry;
        string? winner = stan.Zwyciezca == null ? null : stan.Zwyciezca == gra.Gracze[0] ? a1.Name : a2.Name;

        return new MatchResult(
            Guid.NewGuid(),
            seed,
            a1.Name,
            a2.Name,
            winner,
            stan.TypZwyciestwa,
            moves.Count,
            stan.GetPunktyGracza1(),
            stan.GetPunktyGracza2(),  
            gra.Gracze[0].PobierzZbudowaneKarty(),
            gra.Gracze[0].PobierzZbudowaneKartyCudow(),
            gra.Gracze[0].Surowce,
            gra.Gracze[1].PobierzZbudowaneKarty(),
            gra.Gracze[1].PobierzZbudowaneKartyCudow(),
            gra.Gracze[1].Surowce,
            moves
        );
    }
}
