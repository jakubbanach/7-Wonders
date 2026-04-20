using System.Collections.Generic;

public class DecyzjaKontekst<T>
{
    public TypEfektu Efekt { get; }
    public IReadOnlyList<T> Opcje { get; }
    public Gracz? Decydent { get; } // Gracz podejmujący decyzję kto zaczyna
    public IDecisionResolver resolver { get; }

    public DecyzjaKontekst(TypEfektu efekt, List<T> opcje, Gracz? decydent = null, IDecisionResolver decisionResolver = null)
    {
        Efekt = efekt;
        Opcje = opcje;
        Decydent = decydent;
        resolver = decisionResolver;
    }
}
