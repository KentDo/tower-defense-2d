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
    private float savedTimeScale = 1f; // <--- Lưu tốc độ hiện tại trước khi pause

    public void OnClickToggle()
    {
        paused = !paused;

        if (paused)
        {
            savedTimeScale = Time.timeScale; // Lưu lại tốc độ hiện tại
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = savedTimeScale; // Phục hồi lại tốc độ trước đó
        }

        if (icon) icon.sprite = paused ? resumeSprite : pauseSprite;
        if (pauseMenuPanel) pauseMenuPanel.SetActive(paused);
    }

    public void OnClickResume()
    {
        paused = false;
        Time.timeScale = savedTimeScale; // <--- Giữ nguyên tốc độ trước khi pause
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
