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
        var encoding = GameStateEncoder.EncodeDecision(kontekst);

        if (moveLog != null)
        {
            moveLog.Decisions.Add(new DecisionLog
            {
                TypDecyzji = kontekst.Efekt.ToString(),
                Opcje = kontekst.Opcje.Select(o => o!.ToString()!).ToList(),
                Wybor = wybor!.ToString()!,
                State = GameStateEncoder.Encode(gra),
                LegalMask = encoding.LegalMask,
                ChoiceMask = CreateChoiceMask(encoding.Options, wybor)
            });
        }

        return wybor;
    }

    private static float[] CreateChoiceMask(string[] options, object wybor)
    {
        var mask = new float[options.Length];
        var selectedIndex = Array.FindIndex(options, option => string.Equals(option, wybor?.ToString(), StringComparison.Ordinal));

        if (selectedIndex >= 0)
            mask[selectedIndex] = 1f;

        return mask;
    }
}