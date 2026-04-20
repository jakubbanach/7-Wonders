using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
public class MoveLog
{
    public string Agent { get; set; } = null!;
    public string TypRuchu { get; set; } = null!;
    public string KartyDoWyboru { get; set; } = null!;
    public Karta Karta { get; set; }
    public KartaCudu? KartaCudu { get; set; }
    public Dictionary<Surowiec, int> SurowceGracza { get; set; }
    public Dictionary<Surowiec, int> SurowcePrzeciwnika { get; set; }
    public List<DecisionLog> Decisions { get; set; } = new List<DecisionLog>();


    public MoveLog() { } // wymagany dla serializacji

    public MoveLog(string agent, Ruch ruch, IEnumerable<Karta> dostepneKarty)
    {
        Agent = agent;
        TypRuchu = ruch.TypRuchu.ToString();
        KartyDoWyboru = string.Join(", ", dostepneKarty.Select(k => k.Nazwa));
        Karta = ruch.KartaDoZagrania;
        KartaCudu = ruch.KartaCudu;
        SurowceGracza = ruch.Gracz.Surowce;
        SurowcePrzeciwnika = ruch.Przeciwnik.Surowce;
    }
}