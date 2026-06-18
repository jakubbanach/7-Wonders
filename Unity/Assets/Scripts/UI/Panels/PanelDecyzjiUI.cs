using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelDecyzjiUI : MonoBehaviour
{
    [SerializeField] private Transform kontener;
    [SerializeField] private GameObject przyciskPrefab;

    private List<GameObject> spawned = new();

    // ====== RUCH ======
    private TaskCompletionSource<Ruch> ruchTcs;

    public Task<Ruch> ShowRuchy(List<Ruch> ruchy)
    {
        Clear();

        ruchTcs = new TaskCompletionSource<Ruch>();

        foreach (var ruch in ruchy)
        {
            var btn = CreateButton(ruch.ToString());

            btn.onClick.AddListener(() =>
            {
                ruchTcs.SetResult(ruch);
                Hide();
            });
        }

        gameObject.SetActive(true);
        return ruchTcs.Task;
    }

    // ====== DECYZJE POŒREDNIE ======
    private TaskCompletionSource<object> decisionTcs;

    public Task<T> ShowDecyzje<T>(List<T> opcje)
    {
        Clear();

        decisionTcs = new TaskCompletionSource<object>();

        foreach (var opcja in opcje)
        {
            var btn = CreateButton(opcja.ToString());

            btn.onClick.AddListener(() =>
            {
                decisionTcs.SetResult(opcja);
                Hide();
            });
        }

        gameObject.SetActive(true);

        return decisionTcs.Task.ContinueWith(t => (T)t.Result);
    }

    // ====== helper ======
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
}