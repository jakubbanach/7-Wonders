using System.Threading.Tasks;
using UnityEngine;

public class UnityDecisionResolver : MonoBehaviour, IDecisionResolver
{
    [SerializeField]private PanelDecyzjiUI panel;

    public Task<T> Resolve<T>(Gra gra, DecyzjaKontekst<T> decyzja)
    {
        Debug.Log($"Resolving decision {decyzja} for game {gra}");
        return panel.ShowDecyzje<T>(decyzja.Opcje);
    }
}