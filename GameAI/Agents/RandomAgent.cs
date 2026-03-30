using System;

public class RandomAgent : IAgent
{
    private readonly Random random = new Random();

    public string Name => "Random";

    public Ruch DecideMove(Gra gra)
    {
        var ruchy = gra.DostepneRuchy(); // Pobierz wszystkie dostępne 
        return ruchy[random.Next(ruchy.Count)];
    }
}