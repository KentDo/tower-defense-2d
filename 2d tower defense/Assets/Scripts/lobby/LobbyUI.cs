using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button btnPlay;
    public Button btnReplay;
    public Button btnContinue;
    public Button btnSettings;
    public Button btnQuit;

    [Header("Panels")]
    public GameObject settingsPanel;
    public Button btnClose; // <- NEW: nếu không gán, mình sẽ tự tìm

    [Header("Refs")]
    public SimpleSettingsManager simpleSettingsManager;

    [Header("Config")]
    public string defaultFirstLevel = "Map1";

    void Awake()
    {
        // Wire main buttons
        if (btnPlay) btnPlay.onClick.AddListener(OnClickPlay);
        if (btnReplay) btnReplay.onClick.AddListener(OnClickReplay);
        if (btnContinue) btnContinue.onClick.AddListener(OnClickContinue);
        if (btnSettings) btnSettings.onClick.AddListener(ToggleSettings);
        if (btnQuit) btnQuit.onClick.AddListener(OnClickQuit);

        // Tự tìm nút Close nếu chưa gán trong Inspector
        AutoWireClose();
    }

    void Start()
    {
        if (simpleSettingsManager) simpleSettingsManager.LoadSettings();
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    void Update()
    {
        // Bấm ESC để mở/đóng Settings
        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleSettings();
    }

    // ===== Public =====
    public void OnCloseSettings()
    {
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    // ===== Private =====
    void ToggleSettings()
    {
        if (!settingsPanel) return;
        settingsPanel.SetActive(!settingsPanel.activeSelf);
        // đảm bảo Close được nối listener nếu panel được thay prefab ở runtime
        if (btnClose == null) AutoWireClose();
    }

    void OnClickPlay()
    {
        string level = string.IsNullOrEmpty(defaultFirstLevel) ? "Map1" : defaultFirstLevel;
        TryLoad(level);
    }

    void OnClickReplay()
    {
        string level = string.IsNullOrEmpty(defaultFirstLevel) ? "Map1" : defaultFirstLevel;
        TryLoad(level);
    }

    void OnClickContinue()
    {
        string last = SaveSystem.GetLastLevel();
        if (!string.IsNullOrEmpty(last) && Application.CanStreamedLevelBeLoaded(last))
            SceneManager.LoadScene(last);
        else
            OnClickPlay();
    }

    void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    bool TryLoad(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[LobbyUI] Scene '{sceneName}' chưa có trong Build Profiles.");
            return false;
        }
        SceneManager.LoadScene(sceneName);
        return true;
    }

    void AutoWireClose()
    {
        if (!settingsPanel) return;

        // 1) Nếu đã có tham chiếu, gắn listener chắc chắn
        if (btnClose != null)
        {
            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(OnCloseSettings);
            return;
        }

        // 2) Thử tìm theo tên con "CloseButton"
        var t = settingsPanel.transform.Find("CloseButton");
        if (t) btnClose = t.GetComponent<Button>();

        // 3) Nếu vẫn chưa có, quét mọi Button con và nhận diện bằng tên/label text
        if (btnClose == null)
        {
            foreach (var b in settingsPanel.GetComponentsInChildren<Button>(true))
            {
                if (b.name.ToLower().Contains("close"))
                {
                    btnClose = b;
                    break;
                }
                var tmp = b.GetComponentInChildren<TMP_Text>();
                if (tmp && tmp.text.Trim().ToLower() == "close")
                {
                    btnClose = b;
                    break;
                }
                var utext = b.GetComponentInChildren<UnityEngine.UI.Text>();
                if (utext && utext.text.Trim().ToLower() == "close")
                {
                    btnClose = b;
                    break;
                }
            }
        }

        if (btnClose != null)
        {
            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(OnCloseSettings);
        }
        else
        {
            Debug.LogWarning("[LobbyUI] Không tìm thấy nút Close trong SettingsPanel. Hãy kéo thả vào field 'Btn Close'.");
        }
    }
}
