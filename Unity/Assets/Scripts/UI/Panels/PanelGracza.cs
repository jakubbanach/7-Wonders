using System.Linq;
using TMPro;
using UnityEngine;

public class PanelGracza : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text moneyText;

    public void Odswiez(Gracz gracz)
    {
        nameText.text = gracz.Nazwa;
        moneyText.text = "Monety: " + gracz.Monety();
        Karta[] karty = gracz.PobierzZbudowaneKarty().ToArray();
        Debug.Log("Gracz " + gracz.Nazwa + " ma " + karty.Length + " zbudowanych kart.");
    }
}