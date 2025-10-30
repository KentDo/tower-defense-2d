using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSelectUI : MonoBehaviour
{
    [SerializeField] string lobbySceneName = "Lobby";
    public void BackToLobby()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(lobbySceneName);
    }
}
