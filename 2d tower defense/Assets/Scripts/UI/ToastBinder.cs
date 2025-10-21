using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ToastBinder : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text toastText;
    public CanvasGroup canvasGroup;
    public float duration = 2f;

    [Header("Auto-bind")]
    public LevelManager levelManager;
    public EnemySpawner spawner;

    private bool gameEnded = false;

    void Start()
    {
        if (!levelManager) levelManager = FindObjectOfType<LevelManager>();
        if (!spawner) spawner = FindObjectOfType<EnemySpawner>();

        // Gắn event
        if (levelManager)
            levelManager.onLivesChanged += OnLivesChanged;

        if (spawner)
            spawner.onAllWavesFinished += OnAllWavesFinished;
    }

    public void Show(string message, float time = 2f)
    {
        if (gameEnded) return; // không spam khi đã kết thúc
        StopAllCoroutines();
        StartCoroutine(ShowToast(message, time));
    }

    private IEnumerator ShowToast(string message, float time)
    {
        toastText.text = message;
        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(time);

        // fade out
        for (float t = 1; t >= 0; t -= Time.deltaTime)
        {
            canvasGroup.alpha = t;
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }

    // ===== WIN / LOSE EVENTS =====
    void OnAllWavesFinished()
    {
        if (gameEnded) return;
        gameEnded = true;
        StartCoroutine(ShowEnd("🏆 Victory!", Color.yellow));
    }

    void OnLivesChanged(int newLives)
    {
        if (gameEnded || newLives > 0) return;
        gameEnded = true;
        StartCoroutine(ShowEnd("💀 Defeat...", Color.red));
    }

    IEnumerator ShowEnd(string message, Color color)
    {
        toastText.color = color;
        toastText.text = message;
        canvasGroup.alpha = 1f;

        // Dừng thời gian game
        Time.timeScale = 0f;

        // Đợi 2s rồi cho chọn hành động
        yield return new WaitForSecondsRealtime(2f);

        toastText.text += "\n\nPress [R] to Restart\nPress [M] for Main Menu";

        // Đợi input người chơi
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                yield break;
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
                yield break;
            }
            yield return null;
        }
    }
}
