using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
public class MoveLog
{
    public string Agent { get; set; } = null!;
    public string TypRuchu { get; set; } = null!;
    //public string? Karta { get; set; }
    public Karta Karta { get; set; }
    public string? KartaCudu { get; set; }
    public Dictionary<Surowiec, int> SurowceGracza { get; set; }
    public Dictionary<Surowiec, int> SurowcePrzeciwnika { get; set; }
    


    public MoveLog() { } // wymagany dla serializacji

    public MoveLog(string agent, Ruch ruch)
    {
        Agent = agent;
        TypRuchu = ruch.TypRuchu.ToString();
        //Karta = ruch.KartaDoZagrania?.Nazwa;
        Karta = ruch.KartaDoZagrania;
        KartaCudu = ruch.KartaCudu?.Nazwa;
        SurowceGracza = ruch.Gracz.Surowce;
        SurowcePrzeciwnika = ruch.Przeciwnik.Surowce;
    }
}