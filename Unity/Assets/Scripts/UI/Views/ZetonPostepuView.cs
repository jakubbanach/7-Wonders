using TMPro;
using UnityEngine;

public class ZetonPostepuView : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;

    public void Setup(ZetonPostepu zeton)
    {
        nameText.text = zeton.Nazwa;
    }
}