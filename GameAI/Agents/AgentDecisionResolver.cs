using System;
using System.Collections.Generic;
using System.Text;

public class AgentDecisionResolver : IDecisionResolver
{
    private readonly IAgent agent;

    public AgentDecisionResolver(IAgent agent)
    {
        this.agent = agent;
    }

    public T Resolve<T>(Gra gra, DecyzjaKontekst<T> decyzja)
    {
        return agent.WybierzAkcjePosrednia(gra, decyzja);
    }
}