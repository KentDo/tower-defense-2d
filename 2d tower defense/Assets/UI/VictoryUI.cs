using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VictoryUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button btnRetry;
    public Button btnMainMenu;

    [Header("Scene Config")]
    public string gameplaySceneName = "Map1";
    public string mainMenuSceneName = "Lobby";

    private void Start()
    {
        if (btnRetry) btnRetry.onClick.AddListener(OnRetry);
        if (btnMainMenu) btnMainMenu.onClick.AddListener(OnMainMenu);
    }

    private void OnRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    private void OnMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
