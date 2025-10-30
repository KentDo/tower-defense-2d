using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button btnPlay;        // NEW: Play = luôn bắt đầu mới ở defaultFirstLevel
    public Button btnReplay;      // Replay = vào màn mặc định (giống “Play” cũ, giữ lại theo yêu cầu)
    public Button btnContinue;    // Continue = vào màn đã lưu gần nhất (nếu có)
    public Button btnSettings;
    public Button btnQuit;

    [Header("Panels")]
    public GameObject settingsPanel;

    [Header("Refs")]
    public SimpleSettingsManager simpleSettingsManager;

    [Header("Config")]
    public string defaultFirstLevel = "Level1";

    void Awake()
    {
        if (btnPlay) btnPlay.onClick.AddListener(OnClickPlay);
        if (btnReplay) btnReplay.onClick.AddListener(OnClickReplay);
        if (btnContinue) btnContinue.onClick.AddListener(OnClickContinue);
        if (btnSettings) btnSettings.onClick.AddListener(OnClickSettings);
        if (btnQuit) btnQuit.onClick.AddListener(OnClickQuit);
    }

    void Start()
    {
        if (simpleSettingsManager) simpleSettingsManager.LoadSettings();
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    // --- Buttons ---
    void OnClickPlay()
    {
        // Bắt đầu mới ở level mặc định (không phụ thuộc save)
        string level = string.IsNullOrEmpty(defaultFirstLevel) ? "Level1" : defaultFirstLevel;
        // (tuỳ bạn) có thể xoá last_level để tránh Continue quay lại level cũ:
        // SaveSystem.ClearLastLevel();
        SceneManager.LoadScene(level);
    }

    void OnClickReplay()
    {
        // Hành vi “Replay”: cũng vào level mặc định (giống Play), giữ lại theo yêu cầu
        string level = string.IsNullOrEmpty(defaultFirstLevel) ? "Level1" : defaultFirstLevel;
        SceneManager.LoadScene(level);
    }

    void OnClickContinue()
    {
        string last = SaveSystem.GetLastLevel();
        if (!string.IsNullOrEmpty(last))
            SceneManager.LoadScene(last);
        else
            OnClickPlay(); // fallback: nếu chưa có save thì bắt đầu mới
    }

    void OnClickSettings()
    {
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    public void OnCloseSettings()
    {
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
