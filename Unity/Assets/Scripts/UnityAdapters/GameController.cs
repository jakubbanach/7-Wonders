using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private GraKonsolowa gra;

    [SerializeField] private TextMeshProUGUI statusText;

    void Start()
    {
        gra = new GraKonsolowa();
        statusText.text = "Gra rozpoczêta";
    }

    public void OnKlikRuch()
    {
        statusText.text = "Klikniêto ruch!";
    }
}