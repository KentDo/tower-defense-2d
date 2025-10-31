using UnityEngine;
using UnityEngine.UI;

public class SimpleSettingsManager : MonoBehaviour
{
    [Header("UI")]
    public Toggle toggleFullscreen;
    public Toggle toggleVSync;

    const string KEY_FS = "opt_fullscreen";
    const string KEY_VS = "opt_vsync";

    void Awake()
    {
        if (toggleFullscreen) toggleFullscreen.onValueChanged.AddListener(_ => ApplyFullscreen());
        if (toggleVSync) toggleVSync.onValueChanged.AddListener(_ => ApplyVSync());
    }

    public void LoadSettings()
    {
        bool fs = PlayerPrefs.GetInt(KEY_FS, Screen.fullScreen ? 1 : 0) == 1;
        bool vs = PlayerPrefs.GetInt(KEY_VS, QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;

        if (toggleFullscreen) toggleFullscreen.isOn = fs;
        if (toggleVSync) toggleVSync.isOn = vs;

        ApplyFullscreen();
        ApplyVSync();
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
}
