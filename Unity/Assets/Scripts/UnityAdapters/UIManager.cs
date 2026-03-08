using UnityEngine;

public class UIManager : MonoBehaviour
{
    public PanelPlanszyKart panelKart;
    public PanelPlanszyKonfliktu panelKonfliktu;
    public PanelGracza panelGracz1;
    public PanelGracza panelGracz2;

    private Gra gra;

    public void Setup(Gra gra)
    {
        this.gra = gra;

        Odswiez();
    }

    public void Odswiez()
    {
        panelKart.Odswiez(gra);
        panelKonfliktu.Odswiez(gra);

        panelGracz1.Odswiez(gra.Gracze[0]);
        panelGracz2.Odswiez(gra.Gracze[1]);
    }
}
