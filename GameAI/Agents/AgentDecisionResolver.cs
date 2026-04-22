using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class AgentDecisionResolver : IDecisionResolver
{
    private readonly IAgent agent;
    private readonly MoveLog moveLog;

    public AgentDecisionResolver(IAgent agent, MoveLog moveLog)
    {
        this.agent = agent;
        this.moveLog = moveLog;
    }

    public T Resolve<T>(Gra gra, DecyzjaKontekst<T> kontekst)
    {
        var wybor = agent.WybierzAkcjePosrednia(gra, kontekst);

        if (moveLog != null)
        {
            moveLog.Decisions.Add(new DecisionLog
            {
                TypDecyzji = kontekst.Efekt.ToString(),
                Opcje = kontekst.Opcje.Select(o => o!.ToString()!).ToList(),
                Wybor = wybor!.ToString()!
            });
        }

        return wybor;
    }
}