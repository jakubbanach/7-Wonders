using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PoleKonfliktuView : MonoBehaviour
{
    public Image image;
    public TMP_Text label;

    public void Setup(int pozycja, Strefa strefa)
    {
        label.text = pozycja.ToString();

        if (strefa.LiczbaTraconychMonet > 0)
        {
            // poka¿ ikonê monet
        }

        if (strefa.LiczbaPunktow > 0)
        {
            // poka¿ ikonê punktów
        }
        if (pozycja == -9 || pozycja == 9)
        {
            image.color = Color.yellow; // zwycieskie pola
        }
    }
}