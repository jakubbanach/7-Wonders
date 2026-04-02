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
        //Console.WriteLine($"Dostępne ruchy: {ruchy.Count}");
        //foreach (var ruch in ruchy)
        //{
        //    Console.WriteLine($"Ruch: {ruch.TypRuchu}, Karta: {ruch.KartaDoZagrania?.Nazwa}, KartaCudu: {ruch.KartaCudu?.Nazwa}");
        //}
        //Console.WriteLine($"Stan planszy");
        //Console.WriteLine($"Pozycja konfliktu: {gra.Gracze[0].WypiszStan()}");
        //Console.WriteLine($"Pozycja konfliktu: {gra.Gracze[1].WypiszStan()}");
        //Console.WriteLine($"Plansza do stringa\n {gra.PlanszaEpoki.PlanszaDoStringa()}");
        return ruchy[random.Next(ruchy.Count)];
    }
}