using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text nameText;

    [SerializeField] private Image border;

    private Color normalColor = Color.black;
    private Color hoverColor = Color.yellow;
    private Color pressedColor = Color.white;

    private PoleKarty pole;
    private GameController controller;
    public void Setup(PoleKarty pole, GameController gameController = null)
    {
        this.pole = pole;
        if (gameController != null)
            controller = gameController;

        if (pole.Karta == null) // brak karty na polu
        {
            nameText.text = "";
            cardImage.color = Hex("#9096A5");
            border.color = Hex("#9096A5");
            return;
        }

        if (pole.CzyZakryta) // karta jest zakryta
        {
            nameText.text = "Zakryta";
            border.color = Hex("#FFFFFF");
        }
        else if (pole.CzyDostepna) // karta jest zagrana
        {
            nameText.text = pole.Karta.Nazwa;
            UstawKolorKarty(pole.Karta.KolorKarty);
        }
        else
        {
            nameText.text = pole.Karta.Nazwa;
            UstawKolorKarty(pole.Karta.KolorKarty);
            // ukryj widocznoœæ bordera (tak jak w inspektorze siê robi)
            border.enabled = false;

        }
    }
    public void Setup(Karta karta)
    {
        if (karta == null)
        {
            nameText.text = "";
            cardImage.color = Hex("#9096A5");
            border.color = Hex("#9096A5");
            return;
        }
        nameText.text = karta.Nazwa;
        UstawKolorKarty(karta.KolorKarty);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (pole == null || pole.Karta == null)
        {
            Debug.Log("Klikniêto puste pole. Nie mo¿na zagraæ karty.");
            return;
        }

        Debug.Log("Klikniêto kartê: " + pole.Karta.Nazwa);
        if (pole.CzyZakryta)
        {
            Debug.Log("Ta karta jest zakryta. Nie mo¿na jej zagraæ.");
            return;
        }
        if (pole.CzyDostepna == false)
        {
            Debug.Log("Ta karta jest zablokowana przez inne pola. Nie mo¿na jej zagraæ.");
            return;
        }
        //border.color = pressedColor;
        controller.WybranoKarte(pole);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (pole == null || pole.Karta == null || pole.CzyZakryta || !pole.CzyDostepna)
        {
            return;
        }
        if (border.color == pressedColor)
        {
            return; // nie zmieniaj koloru, jeœli karta jest w stanie "pressed"
        }
        border.color = hoverColor;
        //border.color *= 1.5f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (pole == null || pole.Karta == null || pole.CzyZakryta || !pole.CzyDostepna)
        {
            return;
        }
        if (border.color == pressedColor)
        {
            return; // nie zmieniaj koloru, jeœli karta jest w stanie "pressed"
        }
        border.color = normalColor;
    }

    private void UstawKolorKarty(KolorKarty kolor)
    {
        switch (kolor)
        {
            case KolorKarty.Brazowy:
                cardImage.color = Hex("#6E4C1C");
                break;
            case KolorKarty.Szary:
                cardImage.color = Hex("#797E7C");
                break;
            case KolorKarty.Zielony:
                cardImage.color = Hex("#006F35");
                break;
            case KolorKarty.Niebieski:
                cardImage.color = Hex("#026C9C");
                break;
            case KolorKarty.Zolty: 
                cardImage.color = Hex("#F2B301");
                break;
            case KolorKarty.Czerwony:
                cardImage.color = Hex("#98170A");
                break;
            case KolorKarty.Fioletowy:
                cardImage.color = Hex("#73468B");
                break;
            default:
                cardImage.color = Hex("#000000");
                break;
        }
    }
    private Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var color);
        return color;
    }
}