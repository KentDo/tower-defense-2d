using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MapSelectItem : MonoBehaviour
{
    [Header("UI Refs")]
    public Image thumbnail;
    public TMP_Text txtName;
    public Button btnPlay;
    public GameObject lockOverlay;       // Image đen 60% + icon khóa
    public Transform starsRow;           // 3 Image con (Star0..2)

    [Header("Data")]
    public string sceneName;
    public string displayName;

    public void Setup(Sprite thumb, string display, string scene)
    {
        if (thumbnail) thumbnail.sprite = thumb;
        if (txtName) txtName.text = display;
        sceneName = scene;
        displayName = display;

        bool unlocked = MapProgress.IsUnlocked(sceneName);
        if (btnPlay) btnPlay.interactable = unlocked;
        if (lockOverlay) lockOverlay.SetActive(!unlocked);

        RenderStars(MapProgress.GetStars(sceneName));

        if (btnPlay)
        {
            btnPlay.onClick.RemoveAllListeners();
            btnPlay.onClick.AddListener(() => SceneManager.LoadScene(sceneName));
        }
    }

    public void RenderStars(int count)
    {
        if (!starsRow) return;
        for (int i = 0; i < starsRow.childCount; i++)
            starsRow.GetChild(i).gameObject.SetActive(i < Mathf.Clamp(count, 0, 3));
    }
}
