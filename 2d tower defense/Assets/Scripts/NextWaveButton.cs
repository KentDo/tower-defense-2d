using UnityEngine;
using UnityEngine.UI;

public class NextWaveButton : MonoBehaviour
{
    public Button btn;
    public EnemySpawner spawner;

    void Start()
    {
        if (!btn) btn = GetComponent<Button>();
        if (!spawner) spawner = FindObjectOfType<EnemySpawner>();
        btn.interactable = false; // Ban đầu disable

        // Nếu muốn tự động bật khi CanStartNextWave, có thể check mỗi frame (hoặc đăng ký event nếu có)
        InvokeRepeating(nameof(CheckNextWaveReady), 0f, 0.2f);
    }

    void CheckNextWaveReady()
    {
        if (spawner) btn.interactable = spawner.CanStartNextWave;
    }

    public void OnNextWaveClicked()
    {
        if (spawner) spawner.StartNextWaveNow();
    }
}
