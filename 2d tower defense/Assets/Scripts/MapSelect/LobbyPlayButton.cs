using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyPlayButton : MonoBehaviour
{
    [SerializeField] string mapSelectSceneName = "MapSelect";
    public void OnPlayClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mapSelectSceneName);
    }
}