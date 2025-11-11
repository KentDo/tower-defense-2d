using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelBootSave : MonoBehaviour
{
    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        SaveSystem.SetLastLevel(sceneName);
    }
}
