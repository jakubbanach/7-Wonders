using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

public class PanelDecyzjiUI : MonoBehaviour
{
    [SerializeField] private Transform kontener;
    [SerializeField] private GameObject przyciskPrefab;

    private List<GameObject> spawned = new();

    private TaskCompletionSource<Ruch> ruchTcs;
    private TaskCompletionSource<object> decisionTcs;
    private GameController controller;
    private List<GameObject> spawnedButtons = new();

    public void Init(GameController controller)
    {
        this.controller = controller;
    }

    public Task<Ruch> ShowRuchy(List<Ruch> ruchy)
    {
        Clear();

        ruchTcs = new TaskCompletionSource<Ruch>();

        foreach (var ruch in ruchy)
        {
            var button = CreateButton(GetLabel(ruch));

            button.onClick.AddListener(() =>
            {
                if (ruch.KartaDoZagrania == null)
                {
                    Debug.LogError("Karta do zagrania jest nullem!");
                    return;
                }
                if (ruch.TypRuchu == TypRuchu.ZbudujCud)
                {
                    if (ruch.KartaCudu == null)
                    {
                        Debug.LogError("Karta cudu jest nullem przy ruchu ZbudujCud!");
                        return;
                    }
                    controller.WykonajRuch(ruch.KartaDoZagrania, ruch.TypRuchu, ruch.KartaCudu);
                }
                else
                {
                    controller.WykonajRuch(ruch.KartaDoZagrania, ruch.TypRuchu);
                }
                Hide();
            });
        }

        gameObject.SetActive(true);
        return ruchTcs.Task;
    }

    public Task<T> ShowDecyzje<T>(IReadOnlyList<T> opcje)
    {
        Clear();

        Debug.Log($"Pokazujê decyzje: {string.Join(", ", opcje)}");

        decisionTcs = new TaskCompletionSource<object>();

        foreach (var opcja in opcje)
        {
            Debug.Log($"Tworzê przycisk dla opcji: {opcja}");
            Debug.Log($"Typ opcji: {opcja.GetType()}");
            Debug.Log($"Wartoœæ opcji: {opcja.ToString()}");
            var btn = CreateButton(opcja.ToString());

            btn.onClick.AddListener(() =>
            {
                Debug.Log($"Wybrano opcjê: {opcja}");
                Hide();
            });
        }

        gameObject.SetActive(true);

        return decisionTcs.Task.ContinueWith(t => (T)t.Result);
    }

    private Button CreateButton(string text)
    {
        var obj = Instantiate(przyciskPrefab, kontener);
        spawned.Add(obj);

        var btn = obj.GetComponent<Button>();
        var tmp = obj.GetComponentInChildren<TMP_Text>();

        tmp.text = text;

        return btn;
    }

    private void Clear()
    {
        foreach (var o in spawned)
            Destroy(o);

        spawned.Clear();
    }

    public void Hide()
    {
        Clear();
        gameObject.SetActive(false);
    }

    private string GetLabel(Ruch ruch)
    {
        return ruch.TypRuchu switch
        {
            TypRuchu.ZbudujKarte => "Zbuduj kartê",
            TypRuchu.OdrzucKarte => "Odrzuæ kartê",
            TypRuchu.ZbudujCud => $"Zbuduj cud: {ruch.KartaCudu.Nazwa}",
            _ => "?"
        };
    }
}