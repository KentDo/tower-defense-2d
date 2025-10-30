using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button btnPlay;       // NEW
    public Button btnContinue;
    public Button btnQuit;
    public Button btnSettings;

    [Header("Settings Panel")]
    public GameObject settingsPanel;

    [Header("Config")]
    [Tooltip("Tên level mặc định nếu chưa có save (dùng cho Continue khi chưa có save)")]
    public string defaultFirstLevel = "Map1";
    [Tooltip("Tên scene chọn map")]
    public string mapSelectScene = "MapSelect";   // NEW

    private void Awake()
    {
        if (btnPlay)     btnPlay.onClick.AddListener(OnPlay);           // NEW
        if (btnContinue) btnContinue.onClick.AddListener(OnContinue);
        if (btnQuit)     btnQuit.onClick.AddListener(OnQuit);
        if (btnSettings) btnSettings.onClick.AddListener(() => ToggleSettings(true));

        if (settingsPanel) settingsPanel.SetActive(false);
    }

    private void Start()
    {
        if (btnContinue) btnContinue.interactable = SaveSystem.HasSave;
    }

    // === PLAY -> MapSelect ===
    private void OnPlay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mapSelectScene);
    }

    // === CONTINUE (vào save gần nhất, nếu chưa có thì vào defaultFirstLevel) ===
    private void OnContinue()
    {
        string level = SaveSystem.HasSave ? SaveSystem.GetLastLevel(defaultFirstLevel)
                                          : defaultFirstLevel;
        SceneManager.LoadScene(level);
    }

    private void OnQuit()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    public void ToggleSettings(bool show)
    {
        if (settingsPanel) settingsPanel.SetActive(show);
    }
    public void OnCloseSettings() => ToggleSettings(false);
}
