using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
public class MoveLog
{
    // Pola wymagane do treningu
    public string Agent { get; set; } = null!;
    public float[]? State { get; set; }                    // POTRZEBNE: wejscie do modelu
    public float[]? ActionMask { get; set; }              // POTRZEBNE: maska legalnych akcji
    public int ActionIndex { get; set; } = -1;           // POTRZEBNE: cel treningu
    public float[]? PolicyTarget { get; set; }           // POTRZEBNE: target policy z visit counts

    // Pola informacyjne (mozna pominac dla --minimal-logs)
    public string? TypRuchu { get; set; }                 // TODO: mozna usunac dla ekonomii
    public string? KartyDoWyboru { get; set; }            // TODO: mozna usunac dla ekonomii
    public Karta? Karta { get; set; }                     // TODO: DUZE - pomin dla --minimal-logs
    public KartaCudu? KartaCudu { get; set; }             // TODO: mozna usunac dla ekonomii
    public Dictionary<Surowiec, int>? SurowceGracza { get; set; }          // TODO: mozna usunac dla ekonomii
    public Dictionary<Surowiec, int>? SurowcePrzeciwnika { get; set; }    // TODO: mozna usunac dla ekonomii
    public List<DecisionLog>? Decisions { get; set; }     // TODO: subglowy - pomin na razie


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
        Decisions = new List<DecisionLog>();
    }
}