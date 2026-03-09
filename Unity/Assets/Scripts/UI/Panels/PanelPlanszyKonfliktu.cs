using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PanelPlanszyKonfliktu : MonoBehaviour
{
    [SerializeField] private TMP_Text conflictText;

    [SerializeField] private Transform zetonyContainer;
    [SerializeField] private ZetonPostepuView zetonPrefab;

    [SerializeField] private Transform polaKonfliktuContainer;
    //[SerializeField] private PoleKonfliktuView polePrefab;

    private IReadOnlyList<Strefa> strefy = ZbiorStref.Strefy;

    public void Odswiez(Gra gra)
    {
        var planszaKonfliktu = gra.PlanszaKonfliktu;
        var konflikt = planszaKonfliktu.PionKonfliktu;
        var zetonyPostepu = planszaKonfliktu.ZetonyPostepu;
        var zbiorStref = ZbiorStref.Strefy;

        conflictText.text = "Konflikt: " + konflikt.PobierzPozycje();
        // tutaj bedzie przydzielenie obrazkow do zetonu postepu na planszy konfliktu
        OdswiezZetony(zetonyPostepu);
        OdswiezPolaKonfliktu(planszaKonfliktu);

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
    private void OdswiezPolaKonfliktu(PlanszaKonfliktu planszaKonfliktu)
    {
        Debug.Log("OdswiezPolaKonfliktu: " + planszaKonfliktu.PionKonfliktu.PobierzPozycje());
        var strefaDlaKonfliktu = planszaKonfliktu.PobierzStrefeDlaPozycji(planszaKonfliktu.PionKonfliktu.PobierzPozycje());
        Debug.Log("Strefa dla konfliktu: " + strefaDlaKonfliktu.Nazwa);
        Debug.Log("Pola strefy: " + strefaDlaKonfliktu.LiczbaPol);
        Debug.Log("Punkty za strefe: " + strefaDlaKonfliktu.LiczbaPunktow);
        //foreach (Transform child in polaKonfliktuContainer)
        //{
        //    Destroy(child.gameObject);
        //}

        //foreach (var strefa in strefy)
        //{
        //    if strefa, != konflikt.PobierzPozycje())
        //        continue;
        //    var view = Instantiate(zetonPrefab, polaKonfliktuContainer);
        //    view.Setup(strefa);
        //}
        //planszaKonfliktu.PobierzStrefeDlaPozycji(planszaKonfliktu.PionKonfliktu.PobierzPozycje());
    }
}