using System.Threading.Tasks;

public class UnityDecisionResolver : IDecisionResolver
{
    private PanelDecyzjiUI panel;

    public UnityDecisionResolver(PanelDecyzjiUI panel)
    {
        this.panel = panel;
    }

    public Task<T> Resolve<T>(Gra gra, DecyzjaKontekst<T> decyzja)
    {
        return panel.ShowDecyzje(decyzja.Opcje);
    }
}