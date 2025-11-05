using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class VictoryUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button btnRetry;
    public Button btnMainMenu;

    [Header("Scene Config")]
    public string gameplaySceneName = "Map1";
    public string mainMenuSceneName = "Lobby";

    private void Start()
    {
        if (btnRetry) btnRetry.onClick.AddListener(OnRetry);
        if (btnMainMenu) btnMainMenu.onClick.AddListener(OnMainMenu);
    }

    private void OnRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    private void OnMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
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

    void OnEnable()
    {
        Time.timeScale = 1f;
        EnsureEventSystem();   // <-- đảm bảo có EventSystem
    }
}
