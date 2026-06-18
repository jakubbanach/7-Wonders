using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class WonderCardView : MonoBehaviour
{
    [SerializeField] private Image wonderCardImage;
    [SerializeField] private TMP_Text nameText;

    [SerializeField] private Image zbudowana;

    public void Setup(KartaCudu cud)
    {
        if (cud == null)
        {
            nameText.text = "";
            zbudowana.enabled = false;
            return;
        }
        nameText.text = cud.Nazwa;
        zbudowana.enabled = cud.CzyZagrana;

        if (cud.CzyZagrana)
        {
            //zbudowana.color = Color.green; // zielony kolor dla zbudowanej karty
            Debug.Log("Karta cudu " + cud.Nazwa + " jest zbudowana.");
        }
        else
        {
            // dodaj obramowanie do obiektu
            Debug.Log("Karta cudu " + cud.Nazwa + " nie jest zbudowana.");
        }
    }
}