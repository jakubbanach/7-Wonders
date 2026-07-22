using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class UIManager : MonoBehaviour
{
    [SerializeField] private PanelPlanszyKart panelKart;
    [SerializeField] private PanelPlanszyKonfliktu panelKonfliktu;
    [SerializeField] private PanelGracza panelGracz1;
    [SerializeField] private PanelGracza panelGracz2;
    [SerializeField] private PanelDecyzjiUI panelDecyzjiUI;

    private Gra gra;

    public void Setup(GameController controller, Gra gra)
    {
        this.gra = gra;

        panelDecyzjiUI.Init(controller);
        Odswiez();
    }

    public void Odswiez()
    {
        panelKart.Odswiez(gra);
        panelKonfliktu.Odswiez(gra);

        panelGracz1.Odswiez(gra.Gracze[0]);
        panelGracz2.Odswiez(gra.Gracze[1]);
        if (gra.AktywnyGracz == gra.Gracze[0])
        {
            panelGracz1.GetComponent<CanvasGroup>().alpha = 1f;
            panelGracz2.GetComponent<CanvasGroup>().alpha = 0.5f;
        }
        else
        {
            panelGracz1.GetComponent<CanvasGroup>().alpha = 0.5f;
            panelGracz2.GetComponent<CanvasGroup>().alpha = 1f;
        }
    }

    public void PokazRuchy(List<Ruch> ruchy)
    {
        panelDecyzjiUI.ShowRuchy(ruchy);
    }

    public void UkryjAkcje()
    {
        panelDecyzjiUI.Hide();
    }
}
