public interface IAgent
{
    string Name { get; set;}

    Ruch DecideMove(Gra gra);
}