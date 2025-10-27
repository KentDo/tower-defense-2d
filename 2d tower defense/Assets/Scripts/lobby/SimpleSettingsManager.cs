using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleSettingsManager : MonoBehaviour
{
    [Header("UI")]
    public Toggle toggleFullscreen;
    public Toggle toggleVSync;
    public TMP_Dropdown dropdownDifficulty; // 0=Easy,1=Normal,2=Hard

    // PlayerPrefs keys
    private const string KEY_FULLSCREEN = "opt_fullscreen";
    private const string KEY_VSYNC      = "opt_vsync";
    private const string KEY_DIFFICULTY = "opt_difficulty";

    private void OnEnable()
    {
        // Load saved values (mặc định: fullscreen ON, vsync ON, difficulty = Normal)
        bool fs   = PlayerPrefs.GetInt(KEY_FULLSCREEN, 1) == 1;
        bool vs   = PlayerPrefs.GetInt(KEY_VSYNC, 1) == 1;
        int  diff = PlayerPrefs.GetInt(KEY_DIFFICULTY, 1);

        if (toggleFullscreen) toggleFullscreen.isOn = fs;
        if (toggleVSync)      toggleVSync.isOn      = vs;

        if (dropdownDifficulty)
        {
            dropdownDifficulty.value = Mathf.Clamp(
                diff, 0, Mathf.Max(0, dropdownDifficulty.options.Count - 1)
            );
            dropdownDifficulty.RefreshShownValue();
        }

        // Apply to runtime
        ApplyFullscreen(fs);
        ApplyVSync(vs);
        ApplyDifficulty(diff);

        // Hook listeners
        if (toggleFullscreen) toggleFullscreen.onValueChanged.AddListener(SetFullscreen);
        if (toggleVSync)      toggleVSync.onValueChanged.AddListener(SetVSync);
        if (dropdownDifficulty) dropdownDifficulty.onValueChanged.AddListener(SetDifficulty);
    }

    private void OnDisable()
    {
        // Unhook để tránh double-listener khi bật/tắt panel nhiều lần
        if (toggleFullscreen) toggleFullscreen.onValueChanged.RemoveListener(SetFullscreen);
        if (toggleVSync)      toggleVSync.onValueChanged.RemoveListener(SetVSync);
        if (dropdownDifficulty) dropdownDifficulty.onValueChanged.RemoveListener(SetDifficulty);
    }

    // === Setters (ghi PlayerPrefs + Apply) ===
    public void SetFullscreen(bool on)
    {
        PlayerPrefs.SetInt(KEY_FULLSCREEN, on ? 1 : 0);
        PlayerPrefs.Save();
        ApplyFullscreen(on);
    }

    public void SetVSync(bool on)
    {
        PlayerPrefs.SetInt(KEY_VSYNC, on ? 1 : 0);
        PlayerPrefs.Save();
        ApplyVSync(on);
    }

    public void SetDifficulty(int idx)
    {
        PlayerPrefs.SetInt(KEY_DIFFICULTY, idx);
        PlayerPrefs.Save();
        ApplyDifficulty(idx);
    }

    // === Apply to runtime ===
    private void ApplyFullscreen(bool on)
    {
        Screen.fullScreen = on;
    }

    private void ApplyVSync(bool on)
    {
        QualitySettings.vSyncCount = on ? 1 : 0;
    }

    private void ApplyDifficulty(int idx)
    {
        // Đọc lại ở gameplay: PlayerPrefs.GetInt("opt_difficulty", 1);
        // 0=Easy, 1=Normal, 2=Hard — tuỳ bạn map sang máu quái/coin start...
    }

    // Gán vào nút "Reset"
    public void ResetDefaults()
    {
        // Defaults: Fullscreen ON, VSync ON, Difficulty = Normal
        if (toggleFullscreen) toggleFullscreen.isOn = true;
        if (toggleVSync)      toggleVSync.isOn      = true;

        if (dropdownDifficulty)
        {
            dropdownDifficulty.value = 1; // Normal
            dropdownDifficulty.RefreshShownValue();
        }

        PlayerPrefs.SetInt(KEY_FULLSCREEN, 1);
        PlayerPrefs.SetInt(KEY_VSYNC, 1);
        PlayerPrefs.SetInt(KEY_DIFFICULTY, 1);
        PlayerPrefs.Save();

        ApplyFullscreen(true);
        ApplyVSync(true);
        ApplyDifficulty(1);
    }
}
