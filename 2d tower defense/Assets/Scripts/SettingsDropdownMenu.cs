using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;  // New Input System
#endif

public class SettingsDropdownMenu : MonoBehaviour
{
    [Header("Refs")]
    public Button btnSettings;
    public RectTransform dropdownPanel;
    public Button btnContinue;
    public Button btnReplay;
    public Button btnQuit;
    public Button btnOptions;

    [Header("Config")]
    public string lobbySceneName = "Lobby";
    public bool pauseWhenOpen = true;

    bool isOpen;

    void Awake()
    {
        if (dropdownPanel) dropdownPanel.gameObject.SetActive(false);

        if (btnSettings) btnSettings.onClick.AddListener(ToggleMenu);
        if (btnContinue) btnContinue.onClick.AddListener(OnContinue);
        if (btnReplay)   btnReplay.onClick.AddListener(OnReplay);
        if (btnQuit)     btnQuit.onClick.AddListener(OnQuit);
        if (btnOptions)  btnOptions.onClick.AddListener(OnOptions);
    }

    void Update()
    {
        if (!isOpen || !dropdownPanel || !btnSettings) return;

        // Click ra ngoài để đóng — hỗ trợ cả hai hệ input
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 sp = Mouse.current.position.ReadValue();
#else
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 sp = Input.mousePosition;
#endif
            bool overMenu = RectTransformUtility.RectangleContainsScreenPoint(dropdownPanel, sp);
            var btnRT = btnSettings.GetComponent<RectTransform>();
            bool overBtn  = RectTransformUtility.RectangleContainsScreenPoint(btnRT, sp);

            if (!overMenu && !overBtn) CloseMenu();
        }
    }

    public void ToggleMenu()
    {
        if (isOpen) CloseMenu();
        else OpenMenu();
    }

    void OpenMenu()
    {
        isOpen = true;
        if (dropdownPanel) dropdownPanel.gameObject.SetActive(true);
        if (pauseWhenOpen) Time.timeScale = 0f;
        PositionUnderButton();
    }

    void CloseMenu()
    {
        isOpen = false;
        if (dropdownPanel) dropdownPanel.gameObject.SetActive(false);
        if (pauseWhenOpen) Time.timeScale = 1f;
    }

    void PositionUnderButton()
    {
        if (!btnSettings || !dropdownPanel) return;
        var btnRT = btnSettings.GetComponent<RectTransform>();
        dropdownPanel.anchorMin = btnRT.anchorMin;
        dropdownPanel.anchorMax = btnRT.anchorMax;
        dropdownPanel.pivot     = new Vector2(1f,1f);
        dropdownPanel.position  = btnRT.position + new Vector3(0f, -btnRT.rect.height * 0.5f, 0f);
    }

    void OnContinue() { CloseMenu(); }
    void OnReplay()   { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    void OnQuit()     { Time.timeScale = 1f; SceneManager.LoadScene(lobbySceneName); }
    void OnOptions()  { CloseMenu(); /* mở Option panel nếu có */ }
}
