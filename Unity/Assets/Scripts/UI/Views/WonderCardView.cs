using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WonderCardView : MonoBehaviour
//public class CWonderardView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image wonderCardImage;
    [SerializeField] private TMP_Text nameText;

    [SerializeField] private Image zbudowana;

    //private Color normalColor = Color.black;
    //private Color hoverColor = Color.yellow;

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
    }
    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    if (pole == null || pole.Karta == null)
    //    {
    //        Debug.Log("Klikniêto puste pole. Nie mo¿na zagraæ karty.");
    //        return;
    //    }

    //    Debug.Log("Klikniêto kartê: " + pole.Karta.Nazwa);
    //    if (pole.CzyZakryta)
    //    {
    //        Debug.Log("Ta karta jest zakryta. Nie mo¿na jej zagraæ.");
    //        return;
    //    }
    //    if (pole.CzyDostepna == false)
    //    {
    //        Debug.Log("Ta karta jest zablokowana przez inne pola. Nie mo¿na jej zagraæ.");
    //        return;
    //    }
    //    controller.WykonajRuch(pole, TypRuchu.ZbudujKarte);
    //}
    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    if (pole == null || pole.Karta == null || pole.CzyZakryta || !pole.CzyDostepna)
    //    {
    //        return;
    //    }
    //    zbudowana.color = hoverColor;
    //    //border.color *= 1.5f;
    //}

    //public void OnPointerExit(PointerEventData eventData)
    //{
    //    if (pole == null || pole.Karta == null || pole.CzyZakryta || !pole.CzyDostepna)
    //    {
    //        return;
    //    }
    //    zbudowana.color = normalColor;
    //}

}