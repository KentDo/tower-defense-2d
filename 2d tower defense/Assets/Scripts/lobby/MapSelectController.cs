using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapDef
{
    public string sceneName;     // ví dụ: "Level1", "Level2", ...
    public string displayName;   // ví dụ: "Đồng Cỏ", "Sa Mạc"
    public Sprite thumbnail;     // ảnh preview
    public string nextScene;     // màn sau (để auto unlock khi hoàn thành)
}

public class MapSelectController : MonoBehaviour
{
    [Header("Config")]
    public List<MapDef> maps = new List<MapDef>();

    [Header("UI")]
    public Transform contentGrid;       // nơi chứa các item
    public MapSelectItem itemPrefab;    // prefab thẻ map

    private void Start()
    {
        // Bảo đảm Level1 mở sẵn
        if (maps.Exists(m => m.sceneName == "Level1"))
            MapProgress.Unlock("Level1", true);

        BuildGrid();
    }

    public void BuildGrid()
    {
        if (!contentGrid || !itemPrefab) return;

        // Clear cũ
        for (int i = contentGrid.childCount - 1; i >= 0; i--)
            Destroy(contentGrid.GetChild(i).gameObject);

        // Tạo mới
        foreach (var def in maps)
        {
            var item = Instantiate(itemPrefab, contentGrid);
            item.Setup(def.thumbnail, string.IsNullOrEmpty(def.displayName) ? def.sceneName : def.displayName, def.sceneName);
        }
    }

    // Hàm tiện: gọi từ Win Screen của từng level
    public static void OnLevelCompleted(string currentScene, int starsEarned, string nextSceneIfAny)
    {
        MapProgress.CompleteLevelAndUnlockNext(currentScene, starsEarned, nextSceneIfAny);
    }
}
