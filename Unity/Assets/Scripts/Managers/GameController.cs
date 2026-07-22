using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private Gra gra;
    public UIManager uiManager;
    private IRandom random;
    private PoleKarty wybranaPole;
    private List<Ruch> dostepneRuchy;

    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private int seed = 12345;
    [SerializeField] private UnityDecisionResolver resolver;

    void Start()
    {
        random = new RandomAdapter(seed);
        gra = Gra.StworzNowaGre(random: random);
        statusText.text = "Gra rozpoczęta";
        uiManager.Setup(this, gra);

        Debug.Log("Gra rozpoczęta w Unity.");
        Debug.Log($"Epoka: {gra.Epoka}");
        Debug.Log(gra.PlanszaEpoki.PlanszaDoStringa());
    }

    public void OnKlikRuch()
    {
        statusText.text = "Kliknięto ruch!";
    }
    public async void WykonajRuch(Karta karta, TypRuchu typRuchu, KartaCudu? kartaCudu = null)
    {
        Ruch ruch = new Ruch(gra.AktywnyGracz, gra.Przeciwnik, karta, typRuchu, kartaCudu);
        await gra.WykonajRuch(ruch, resolver, random);

        wybranaPole = null;

        uiManager.Odswiez();
    }
    public void WybranoKarte(PoleKarty pole)
    {
        wybranaPole = pole;

        dostepneRuchy = gra.DostepneRuchy()
            .Where(r => r.KartaDoZagrania == pole.Karta)
            .ToList();

        uiManager.PokazRuchy(dostepneRuchy);
    }
}