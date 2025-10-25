using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToLobby : MonoBehaviour
{
    public string lobbyScene = "Lobby";
    public void GoBack() => SceneManager.LoadScene(lobbyScene);
}
