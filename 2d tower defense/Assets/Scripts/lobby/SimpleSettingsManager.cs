using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleSettingsManager : MonoBehaviour
{
    [Header("UI")]
    public Toggle toggleFullscreen;
    public Toggle toggleVSync;
    public TMP_Dropdown dropdownDifficulty;

    // Bạn có thể dùng giá trị này ở game (ví dụ scale HP/tiền)
    public static int DifficultyIndex { get; private set; } = 1; // 0=Easy,1=Normal,2=Hard

    const string KEY_FS = "opt_fullscreen";
    const string KEY_VS = "opt_vsync";
    const string KEY_DF = "opt_difficulty";

    void Awake()
    {
        // Gán callback UI
        if (toggleFullscreen) toggleFullscreen.onValueChanged.AddListener(_ => ApplyFullscreen());
        if (toggleVSync) toggleVSync.onValueChanged.AddListener(_ => ApplyVSync());
        if (dropdownDifficulty) dropdownDifficulty.onValueChanged.AddListener(_ => ApplyDifficulty());
    }

    public void LoadSettings()
    {
        bool fs = PlayerPrefs.GetInt(KEY_FS, Screen.fullScreen ? 1 : 0) == 1;
        bool vs = PlayerPrefs.GetInt(KEY_VS, QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;
        int df = PlayerPrefs.GetInt(KEY_DF, 1);

        if (toggleFullscreen) toggleFullscreen.isOn = fs;
        if (toggleVSync) toggleVSync.isOn = vs;
        if (dropdownDifficulty)
        {
            df = Mathf.Clamp(df, 0, 2);
            dropdownDifficulty.value = df;
        }

        ApplyAll();
    }

    public void ApplyAll()
    {
        ApplyFullscreen();
        ApplyVSync();
        ApplyDifficulty();
    }

    void ApplyFullscreen()
    {
        bool fs = toggleFullscreen ? toggleFullscreen.isOn : Screen.fullScreen;
        Screen.fullScreen = fs;
        PlayerPrefs.SetInt(KEY_FS, fs ? 1 : 0);
        PlayerPrefs.Save();
    }

    void ApplyVSync()
    {
        bool vs = toggleVSync ? toggleVSync.isOn : (QualitySettings.vSyncCount > 0);
        QualitySettings.vSyncCount = vs ? 1 : 0;
        PlayerPrefs.SetInt(KEY_VS, vs ? 1 : 0);
        PlayerPrefs.Save();
    }

    void ApplyDifficulty()
    {
        int df = dropdownDifficulty ? dropdownDifficulty.value : 1;
        DifficultyIndex = Mathf.Clamp(df, 0, 2);
        PlayerPrefs.SetInt(KEY_DF, DifficultyIndex);
        PlayerPrefs.Save();

        // Debug ví dụ: bạn có thể map sang multiplier HP/tiền ở nơi khác
        switch (DifficultyIndex)
        {
            case 0: Debug.Log("[Settings] Difficulty set: 0 (Easy)"); break;
            case 1: Debug.Log("[Settings] Difficulty set: 1 (Normal)"); break;
            case 2: Debug.Log("[Settings] Difficulty set: 2 (Hard)"); break;
        }
    }
}
