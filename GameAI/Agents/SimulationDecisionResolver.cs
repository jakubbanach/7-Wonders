using System.Threading.Tasks;
public class SimulationDecisionResolver : IDecisionResolver
{
    private readonly IAgent agent;

    public SimulationDecisionResolver(IAgent agent)
    {
        this.agent = agent;
    }

    public Task<T> Resolve<T>(Gra gra, DecyzjaKontekst<T> kontekst)
    {
        return Task.FromResult(agent.WybierzAkcjePosrednia(gra, kontekst));
    }
}