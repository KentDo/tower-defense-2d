using UnityEngine;
using TMPro;

public class HUDLives : MonoBehaviour
{
    public TextMeshProUGUI txt;

    void Start()
    {
        var lm = FindObjectOfType<LevelManager>();
        if (!lm)
        {
            Debug.LogWarning("[HUDLives] LevelManager not found");
            return;
        }
        if (!txt) txt = GetComponent<TextMeshProUGUI>();

        // cập nhật khi đổi
        lm.onLivesChanged += v => { if (txt) txt.text = v.ToString(); };
        // hiển thị lần đầu
        if (txt) txt.text = lm.lives.ToString();
    }
}
