using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MapSelectUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button btnMap1;
    public Button btnMap2;
    public Button btnMap3;
    public Button btnReturn;

    void Start()
    {
        if (btnMap1) btnMap1.onClick.AddListener(() => LoadMap("Map1"));
        if (btnMap2) btnMap2.onClick.AddListener(() => LoadMap("Map2"));
        if (btnMap3) btnMap3.onClick.AddListener(() => LoadMap("Map3"));
        if (btnReturn) btnReturn.onClick.AddListener(ReturnToLobby);
    }

    void LoadMap(string mapName)
    {
        if (!Application.CanStreamedLevelBeLoaded(mapName))
        {
            Debug.LogError($"Scene {mapName} chưa được thêm vào Build Profiles!");
            return;
        }

        Debug.Log($"Loading {mapName}...");
        SceneManager.LoadScene(mapName);
    }

    void ReturnToLobby()
    {
        if (Application.CanStreamedLevelBeLoaded("Lobby"))
            SceneManager.LoadScene("Lobby");
        else
            Debug.LogError("Lobby scene chưa có trong Build Profiles!");
    }
}
