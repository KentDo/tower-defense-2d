// Assets/Scripts/UI/GameOverUI.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Tooltip("Optional fallback scene name if the session cache is empty.")]
    public string gameplaySceneName = string.Empty;

    public void Retry()
    {
        Time.timeScale = 1f;

        string targetScene;
        if (!GameSession.TryGetLastGameplayScene(out targetScene) || string.IsNullOrEmpty(targetScene))
        {
            targetScene = gameplaySceneName;
        }

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("[GameOverUI] No gameplay scene configured for retry.");
            return;
        }

        GameSession.ClearAll();
        SceneManager.LoadScene(targetScene);
    }
}
