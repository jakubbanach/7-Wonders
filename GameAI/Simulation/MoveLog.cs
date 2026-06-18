using System;
using System.Collections.Generic;
using System.Linq;
public class MoveLog
{
    // Pola wymagane do treningu
    public string Agent { get; set; } = null!;
    public float[]? State { get; set; }                    // wejscie do modelu
    public float[]? ActionMask { get; set; }              // maska legalnych akcji
    public int ActionIndex { get; set; } = -1;           // cel treningu
    public float[]? PolicyTarget { get; set; }           // target policy z visit counts

    public string? TypRuchu { get; set; }
    public string? KartyDoWyboru { get; set; }
    public Karta? Karta { get; set; }
    public KartaCudu? KartaCudu { get; set; }
    public Dictionary<Surowiec, int>? SurowceGracza { get; set; }
    public Dictionary<Surowiec, int>? SurowcePrzeciwnika { get; set; }
    public List<DecisionLog>? Decisions { get; set; }


    public MoveLog() { }

    public MoveLog(string agent, Ruch ruch, IEnumerable<Karta> dostepneKarty)
    {
        Agent = agent;
        TypRuchu = ruch.TypRuchu.ToString();
        KartyDoWyboru = string.Join(", ", dostepneKarty.Select(k => k.Nazwa));
        Karta = ruch.KartaDoZagrania;
        KartaCudu = ruch.KartaCudu;
        SurowceGracza = ruch.Gracz.Surowce;
        SurowcePrzeciwnika = ruch.Przeciwnik.Surowce;
        Decisions = new List<DecisionLog>();
    }
}