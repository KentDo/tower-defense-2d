using TMPro;
using UnityEngine;

public class HUDCoins : MonoBehaviour
{
    public TextMeshProUGUI txt;
    void Start()
    {
        var bm = BuildManager.I;
        if (!bm) { Debug.LogWarning("[HUDCoins] BuildManager not found"); return; }
        if (!txt) txt = GetComponent<TextMeshProUGUI>();
        bm.onCoinsChanged += v => { if (txt) txt.text = $"$ {v}"; };
        txt.text = $"$ {bm.coins}";
    }
}
