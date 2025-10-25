// Assets/Scripts/UI/GameOverUI.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public string gameplaySceneName = "YourGameplaySceneName";
    public void Retry()
    {
        Time.timeScale = 1f;
        GameSession.ClearAll();
        SceneManager.LoadScene(gameplaySceneName);
    }
}
