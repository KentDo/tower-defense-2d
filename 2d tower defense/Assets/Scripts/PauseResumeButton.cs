using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseResumeButton : MonoBehaviour
{
    [Header("Button Icon")]
    public Image icon;
    public Sprite pauseSprite;   // Icon "pause"
    public Sprite resumeSprite;  // Icon "resume"

    [Header("Pause Menu")]
    public GameObject pauseMenuPanel;  // Kéo panel PauseMenu vào đây

    [Header("Scene Settings")]
    public string mainMenuScene = "Lobby";   // tên scene menu chính
    public string currentScene = "Map1";     // tên scene hiện tại (để restart)

    private bool paused = false;

    public void OnClickToggle()
    {
        paused = !paused;
        Time.timeScale = paused ? 0f : 1f;

        // Đổi icon
        if (icon) icon.sprite = paused ? resumeSprite : pauseSprite;

        // Bật/tắt menu
        if (pauseMenuPanel) pauseMenuPanel.SetActive(paused);
    }

    public void OnClickResume()
    {
        paused = false;
        Time.timeScale = 1f;
        if (icon) icon.sprite = pauseSprite;
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
    }

    public void OnClickRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(currentScene);
    }

    public void OnClickMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }
}
