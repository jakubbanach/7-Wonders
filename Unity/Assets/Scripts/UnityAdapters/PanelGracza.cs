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
        moneyText.text = gracz.Monety() + " $";
    }
}