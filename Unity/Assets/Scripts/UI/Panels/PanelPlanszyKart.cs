using UnityEngine;
public class PanelPlanszyKart : MonoBehaviour
{
    public Transform cardsParent;
    public GameObject cardPrefab;

    public void Odswiez(Gra gra)
    {
        //Clear();

        //var karty = gra.DostepneKarty();

        //foreach (var karta in karty)
        //{
        //    GameObject obj = Instantiate(cardPrefab, cardsParent);

        //    CardView view = obj.GetComponent<CardView>();
        //    view.Setup(karta);
        //}
    }

    void Clear()
    {
        foreach (Transform child in cardsParent)
        {
            Destroy(child.gameObject);
        }
    }
}