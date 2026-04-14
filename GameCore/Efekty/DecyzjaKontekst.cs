using System.Collections.Generic;

public class DecyzjaKontekst<T>
{
    public TypEfektu Efekt { get; }
    public IReadOnlyList<T> Opcje { get; }

    public DecyzjaKontekst(TypEfektu efekt, List<T> opcje)
    {
        Efekt = efekt;
        Opcje = opcje;
    }
}
