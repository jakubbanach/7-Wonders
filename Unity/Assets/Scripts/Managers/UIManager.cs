using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private PanelPlanszyKart panelKart;
    [SerializeField] private PanelPlanszyKonfliktu panelKonfliktu;
    [SerializeField] private PanelGracza panelGracz1;
    [SerializeField] private PanelGracza panelGracz2;

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
