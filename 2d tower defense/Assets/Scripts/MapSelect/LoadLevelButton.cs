using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevelButton : MonoBehaviour
{
    [SerializeField] string sceneName = "Map1";
    [SerializeField] bool clearSavesOnPlay = true;

    public void LoadThisLevel()
    {
        Time.timeScale = 1f;                 // đề phòng đang pause
        if (clearSavesOnPlay)
        {
            SaveSystem.ClearSave();          // nếu có hệ save tối giản
            // GameSaveManager.Delete();     // bật nếu dùng save đầy đủ
        }
        SceneManager.LoadScene(sceneName);
    }
}
