using System.Linq;
using TMPro;
using UnityEngine;

public class PanelGracza : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text moneyText;

    [SerializeField] private Transform brownContainer;
    [SerializeField] private Transform grayContainer;
    [SerializeField] private Transform blueContainer;
    [SerializeField] private Transform yellowContainer;
    [SerializeField] private Transform redContainer;
    [SerializeField] private Transform greenContainer;
    [SerializeField] private Transform purpleContainer;

    [SerializeField] private CardView boughtCardPrefab;

    [SerializeField] private Transform wondersContainer;
    [SerializeField] private WonderCardView wonderCardPrefab;

    void Clear(Transform container)
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);
    }
    public void Odswiez(Gracz gracz)
    {
        nameText.text = gracz.Nazwa;
        moneyText.text = "Monety: " + gracz.Monety();
        Karta[] karty = gracz.PobierzZbudowaneKarty().ToArray();
        KartaCudu[] cuda = gracz.KartyCudow.ToArray();
        Debug.Log("Gracz " + gracz.Nazwa + " ma " + karty.Length + " zbudowanych kart.");
        Debug.Log("Gracz " + gracz.Nazwa + " ma " + cuda.Length + " kart cudow.");

        Clear(brownContainer);
        Clear(grayContainer);
        Clear(blueContainer);
        Clear(yellowContainer);
        Clear(redContainer);
        Clear(greenContainer);
        Clear(purpleContainer);

        foreach (var karta in karty)
        {
            Transform container = GetContainerForColor(karta.KolorKarty);

            var view = Instantiate(boughtCardPrefab, container);
            view.Setup(karta);
        }
        Clear(wondersContainer);
        foreach (var wonder in gracz.KartyCudow)
        {
            var view = Instantiate(wonderCardPrefab, wondersContainer);
            view.Setup(wonder);
        }
    }

    private Transform GetContainerForColor(KolorKarty kolor)
    {
        return kolor switch
        {
            KolorKarty.Brazowy => brownContainer,
            KolorKarty.Szary => grayContainer,
            KolorKarty.Niebieski => blueContainer,
            KolorKarty.Zolty => yellowContainer,
            KolorKarty.Czerwony => redContainer,
            KolorKarty.Zielony => greenContainer,
            KolorKarty.Fioletowy => purpleContainer,
            _ => throw new System.ArgumentException("Nieznany kolor karty: " + kolor)
        };
    }
}