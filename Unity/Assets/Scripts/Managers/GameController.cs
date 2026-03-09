using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private Gra gra;
    public UIManager uiManager;

    [SerializeField] private TextMeshProUGUI statusText;

    void Start()
    {
        gra = Gra.StworzNowaGre();
        statusText.text = "Gra rozpoczêta";
        uiManager.Setup(gra);
    }

    public void OnKlikRuch()
    {
        statusText.text = "Klikniêto ruch!";
    }
    public void WykonajRuch(Karta karta, TypRuchu ruch)
    {
        gra.WykonajRuch(karta, ruch);

        uiManager.Odswiez();
    }
}