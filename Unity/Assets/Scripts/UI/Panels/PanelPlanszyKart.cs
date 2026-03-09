using UnityEngine;
using UnityEngine.UI;
public class PanelPlanszyKart : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private GameController controller;

    [SerializeField] private int spacing = 40;

    public void Odswiez(Gra gra)
    {
        var plansza = gra.PlanszaEpoki;
        var uklad = plansza.Uklad();

        int index = 0;

        foreach (Transform child in container)
            Destroy(child.gameObject);

        Debug.Log("Odswiezanie planszy kart..." + gra.Epoka.ToString());

        foreach (var liczbaKart in uklad)
        {
            GameObject row = new GameObject("Row", typeof(RectTransform));
            row.transform.SetParent(container, false);

            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = false;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            layout.spacing = spacing;

            row.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            //Debug.Log("Liczba kart w tej linii: " + liczbaKart);
            for (int i = 0; i < liczbaKart; i++)
            {
                var pole = plansza.Pola[index++];

                var card = Instantiate(cardPrefab, row.transform);
                card.Setup(pole, controller);
            }
        }
    }
}