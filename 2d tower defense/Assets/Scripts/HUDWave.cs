using TMPro;
using UnityEngine;

public class HUDWave : MonoBehaviour
{
    public TextMeshProUGUI txt;
    public EnemySpawner spawner;
    void Start()
    {
        if (!txt) txt = GetComponent<TextMeshProUGUI>();
        if (!spawner) spawner = FindObjectOfType<EnemySpawner>();
        if (!spawner) { Debug.LogWarning("[HUDWave] Spawner not found"); return; }
        spawner.onWaveChanged += (cur, total) => { if (txt) txt.text = $"Wave: {cur}/{total}"; };
        txt.text = spawner.TotalWaves > 0 ? $"Wave: 0/{spawner.TotalWaves}" : "Wave: -";
    }
}
