using System;

public class RandomAgent : IAgent
{
    private readonly IRandom random;

    public string Name { get; set; } = "Random";
    public RandomAgent(IRandom random)
    {
        this.random = random;
    }
    public Ruch DecideMove(Gra gra)
    {
        var ruchy = gra.DostepneRuchy(); // Pobierz wszystkie dostępne 
        Console.WriteLine($"Dostępne ruchy: {ruchy.Count}");
        return ruchy[random.Next(ruchy.Count)];
    }
}