using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
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
    float savedSpeed = 1f;  // <-- NEW: speed đang dùng trước khi mở menu

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

        // lưu speed hiện tại (nếu đang chạy)
        if (Time.timeScale > 0f) savedSpeed = Time.timeScale;
        else if (savedSpeed <= 0f) savedSpeed = 1f; // dự phòng

        if (pauseWhenOpen) Time.timeScale = 0f;
        PositionUnderButton();
    }

    void CloseMenu()
    {
        isOpen = false;
        if (dropdownPanel) dropdownPanel.gameObject.SetActive(false);

        // KHÔNG đổi speed về 1! Khôi phục đúng speed trước khi mở menu
        if (pauseWhenOpen)
            Time.timeScale = Mathf.Max(0.01f, savedSpeed);
    }

    void PositionUnderButton()
    {
        if (!btnSettings || !dropdownPanel) return;
        var btnRT = btnSettings.GetComponent<RectTransform>();
        dropdownPanel.anchorMin = btnRT.anchorMin;
        dropdownPanel.anchorMax = btnRT.anchorMax;
        dropdownPanel.pivot     = new Vector2(1f, 1f);
        dropdownPanel.position  = btnRT.position + new Vector3(0f, -btnRT.rect.height * 0.5f, 0f);
    }

    void OnContinue() { CloseMenu(); }

    void OnReplay()
    {
        Time.timeScale = 1f; // replay về x1 là hợp lý
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnQuit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(lobbySceneName);
    }

    void OnOptions()  { CloseMenu(); /* mở Option panel nếu có */ }
}
