using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelBootSave : MonoBehaviour
{
    void Start()
    {
        SaveSystem.SetLastLevel(SceneManager.GetActiveScene().name);
    }
}
