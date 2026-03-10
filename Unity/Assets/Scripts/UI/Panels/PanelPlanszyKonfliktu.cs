using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PanelPlanszyKonfliktu : MonoBehaviour
{
    [SerializeField] private Transform zetonyContainer;
    [SerializeField] private ZetonPostepuView zetonPrefab;

    [SerializeField] private Transform TorKonfliktuContainer;
    [SerializeField] private PoleKonfliktuView polePrefab;

    private IReadOnlyList<Strefa> strefy = ZbiorStref.Strefy;
    private List<PoleKonfliktuView> pola = new();

    public void Odswiez(Gra gra)
    {
        var planszaKonfliktu = gra.PlanszaKonfliktu;
        var konflikt = planszaKonfliktu.PionKonfliktu;
        var zetonyPostepu = planszaKonfliktu.ZetonyPostepu;
        var zbiorStref = ZbiorStref.Strefy;

        // tutaj bedzie przydzielenie obrazkow do zetonu postepu na planszy konfliktu
        OdswiezZetony(zetonyPostepu);
        OdswiezPolaKonfliktu(planszaKonfliktu);
        OdswiezPion(planszaKonfliktu);

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
    private void OdswiezPolaKonfliktu(PlanszaKonfliktu plansza)
    {
        foreach (Transform child in TorKonfliktuContainer)
            Destroy(child.gameObject);

        pola.Clear();

        for (int pos = -9; pos <= 9; pos++)
        {
            var strefa = plansza.PobierzStrefeDlaPozycji(pos);

            var pole = Instantiate(polePrefab, TorKonfliktuContainer);
            pole.Setup(pos, strefa);

            pola.Add(pole);
        }
    }

    private void OdswiezPion(PlanszaKonfliktu plansza)
    {
        int pos = plansza.PionKonfliktu.PobierzPozycje();

        pola[pos + 9].image.color = Color.red; // ustawienie koloru pola, na którym znajduje siê pion konfliktu
    }
}