public interface IAgent
{
    string Name { get; }

    Ruch DecideMove(Gra gra);
}