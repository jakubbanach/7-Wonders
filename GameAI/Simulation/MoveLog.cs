using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
public class MoveLog
{
    public string Agent { get; set; } = null!;
    public string TypRuchu { get; set; } = null!;
    public string? Karta { get; set; }
    public string? KartaCudu { get; set; }


    public MoveLog() { } // wymagany dla serializacji

    public MoveLog(string agent, Ruch ruch)
    {
        Agent = agent;
        TypRuchu = ruch.TypRuchu.ToString();
        Karta = ruch.KartaDoZagrania?.Nazwa;
        KartaCudu = ruch.KartaCudu?.Nazwa;
    }
}