using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button btnContinue;
    public Button btnReplay;
    public Button btnQuit;
    public Button btnSettings;

    [Header("Settings Panel")]
    public GameObject settingsPanel;

    [Header("Config")]
    [Tooltip("Tên level mặc định nếu chưa có save")]
    public string defaultFirstLevel = "Level1";

    private void Awake()
    {
        if (btnContinue) btnContinue.onClick.AddListener(OnContinue);
        if (btnReplay)   btnReplay.onClick.AddListener(OnReplay);
        if (btnQuit)     btnQuit.onClick.AddListener(OnQuit);
        if (btnSettings) btnSettings.onClick.AddListener(() => ToggleSettings(true));

        if (settingsPanel) settingsPanel.SetActive(false);
    }

    private void Start()
    {
        if (btnContinue)
            btnContinue.interactable = SaveSystem.HasSave;
    }

    private void OnContinue()
    {
        string level = SaveSystem.HasSave ? SaveSystem.GetLastLevel(defaultFirstLevel)
                                          : defaultFirstLevel;
        SceneManager.LoadScene(level);
    }

    private void OnReplay()
    {
        SaveSystem.ClearSave();
        SceneManager.LoadScene(defaultFirstLevel);
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

    public void OnCloseSettings()
    {
        ToggleSettings(false);
    }
}
