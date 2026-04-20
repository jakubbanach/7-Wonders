public class SimulationDecisionResolver : IDecisionResolver
{
    private readonly IAgent agent;

    public SimulationDecisionResolver(IAgent agent)
    {
        this.agent = agent;
    }

    public T Resolve<T>(Gra gra, DecyzjaKontekst<T> kontekst)
    {
        return agent.WybierzAkcjePosrednia(gra, kontekst);
    }
}