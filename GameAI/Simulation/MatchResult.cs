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
            moves
        );
    }
}
