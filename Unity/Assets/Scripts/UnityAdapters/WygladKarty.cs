using TMPro;
using UnityEngine;

public class WygladKarty : MonoBehaviour
{
    public TMP_Text nameText;

    private Karta karta;

    public void Setup(Karta karta)
    {
        this.karta = karta;

        nameText.text = karta.Nazwa;
    }
}