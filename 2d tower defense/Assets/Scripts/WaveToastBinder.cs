using UnityEngine;

public class WaveToastBinder : MonoBehaviour
{
    public EnemySpawner spawner;
    void Start()
    {
        if (!spawner) spawner = FindObjectOfType<EnemySpawner>();
        if (!spawner) return;
        spawner.onWaveChanged += (cur, total) =>
        {
            ToastManager.Show($"Wave {cur} bắt đầu!");
        };
        spawner.onAllWavesFinished += () => ToastManager.Show("Hoàn thành tất cả Waves!");
    }
}
