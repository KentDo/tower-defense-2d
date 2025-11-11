// Assets/Scripts/UI/GameOverUI.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameOverUI : MonoBehaviour
{
    [Tooltip("Optional fallback scene name nếu session trống.")]
    public string gameplaySceneName = "Map1";
    public string mainMenuSceneName = "Lobby";

    void OnEnable()
    {
        // đảm bảo có thể click UI khi ở Game Over
        Time.timeScale = 1f;
        EnsureEventSystem();
    }

    // === Nút Retry ===
    public void Retry()
    {
        string targetScene;
        if (!GameSession.TryGetLastGameplayScene(out targetScene) || string.IsNullOrEmpty(targetScene))
            targetScene = gameplaySceneName;

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("[GameOverUI] No gameplay scene configured for retry.");
            return;
        }

        // 1) Reset input & dọn EventSystem cũ để tránh xung đột sau LoadScene
        Time.timeScale = 1f;
        var es = FindObjectOfType<EventSystem>();
        if (es) Destroy(es.gameObject);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 2) Sau khi scene gameplay load xong => Reinitialize các manager
        SceneManager.sceneLoaded += OnSceneLoadedAfterRetry;
        SceneManager.LoadScene(targetScene);
    }

    // === Nút Main Menu ===
    public void MainMenu()
    {
        Time.timeScale = 1f;
        var es = FindObjectOfType<EventSystem>();
        if (es) Destroy(es.gameObject);

        if (!Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            Debug.LogError($"[GameOverUI] Scene '{mainMenuSceneName}' chưa có trong Build Profiles.");
            return;
        }
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // callback chỉ chạy 1 lần ngay sau Retry → khởi tạo lại các hệ thống
    void OnSceneLoadedAfterRetry(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoadedAfterRetry;

        // đảm bảo scene gameplay có EventSystem đúng module
        EnsureEventSystem();

        // Reinitialize managers
        var bm = Object.FindObjectOfType<BuildManager>();
        if (bm) bm.Reinitialize();

        var tp = Object.FindObjectOfType<TowerPlacer>();
        if (tp) tp.Reinitialize();

        Debug.Log("[GameOverUI] Retry complete → managers reinitialized.");
    }

    static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;

#if ENABLE_INPUT_SYSTEM
        new GameObject("EventSystem",
            typeof(EventSystem),
            typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
#else
        new GameObject("EventSystem",
            typeof(EventSystem),
            typeof(StandaloneInputModule));
#endif
    }
}
