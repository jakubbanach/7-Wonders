using System;

public class RandomAgent : IAgent
{
    private readonly IRandom random;

    public string Name { get; set; } = "Random";
    public RandomAgent(IRandom random)
    {
        this.random = random;
    }
    public Ruch WybierzRuch(Gra gra)
    {
        var ruchy = gra.DostepneRuchy();
        return ruchy[random.Next(ruchy.Count)];
    }
    public T WybierzAkcjePosrednia<T>(Gra gra, DecyzjaKontekst<T> decyzja)
    {
        return decyzja.Opcje[random.Next(decyzja.Opcje.Count)];
    }
}