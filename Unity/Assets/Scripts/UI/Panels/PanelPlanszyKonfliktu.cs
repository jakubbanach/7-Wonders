using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PanelPlanszyKonfliktu : MonoBehaviour
{
    [SerializeField] private TMP_Text conflictText;

    [SerializeField] private Transform zetonyContainer;
    [SerializeField] private ZetonPostepuView zetonPrefab;

    public void Odswiez(Gra gra)
    {
        var planszaKonfliktu = gra.PlanszaKonfliktu;
        var konflikt = planszaKonfliktu.PionKonfliktu;
        var zetonyPostepu = planszaKonfliktu.ZetonyPostepu;

        conflictText.text = "Konflikt: " + konflikt.PobierzPozycje();
        // tutaj bedzie przydzielenie obrazkow do zetonu postepu na planszy konfliktu
        OdswiezZetony(zetonyPostepu);

    }

    private void OdswiezZetony(List<ZetonPostepu> zetony)
    {
        foreach (Transform child in zetonyContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var zeton in zetony)
        {
            var view = Instantiate(zetonPrefab, zetonyContainer);
            view.Setup(zeton);
        }
    }
}