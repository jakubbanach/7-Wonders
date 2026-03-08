using TMPro;
using UnityEngine;

public class PanelPlanszyKonfliktu : MonoBehaviour
{
    public TMP_Text conflictText;

    public void Odswiez(Gra gra)
    {
        conflictText.text =
            "Konflikt: " + gra.PozycjaKonfliktu;
    }
}