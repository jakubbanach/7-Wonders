using System;
using System.Linq;
using System.Threading.Tasks;

public class AgentDecisionResolver : IDecisionResolver
{
    private readonly IAgent agent;
    private readonly MoveLog? moveLog;

    public AgentDecisionResolver(IAgent agent, MoveLog? moveLog = null)
    {
        this.agent = agent;
        this.moveLog = moveLog;
    }

    public Task<T> Resolve<T>(Gra gra, DecyzjaKontekst<T> kontekst)
    {
        var wybor = agent.WybierzAkcjePosrednia(gra, kontekst);
        var fixedEncoding = GameStateEncoder.EncodeSubdecision(gra, kontekst.Efekt);
        var encoding = fixedEncoding.Options.Length > 0
            ? fixedEncoding
            : GameStateEncoder.EncodeDecision(kontekst);

        if (moveLog != null && moveLog.Decisions != null)
        {
            moveLog.Decisions.Add(new DecisionLog
            {
                TypDecyzji = kontekst.Efekt.ToString(),
                Opcje = encoding.Options.ToList(),
                Wybor = wybor!.ToString()!,
                State = GameStateEncoder.Encode(gra),
                LegalMask = encoding.LegalMask,
                ChoiceMask = CreateChoiceMask(encoding.Options, wybor)
            });
        }

        return Task.FromResult(wybor);
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