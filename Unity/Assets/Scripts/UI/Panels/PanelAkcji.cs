using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PanelAkcji : MonoBehaviour
{
    [SerializeField] private PanelDecyzjiUI panel;

    public Task<Ruch> Show(List<Ruch> ruchy)
    {
        return panel.ShowRuchy(ruchy);
    }
}