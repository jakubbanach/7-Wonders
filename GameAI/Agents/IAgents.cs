public interface IAgent
{
    string Name { get; set;}

    Ruch WybierzRuch(Gra gra);
    T WybierzAkcjePosrednia<T>(Gra gra, DecyzjaKontekst<T> decyzja);
}