using UnityEngine;
using UnityEngine.UI;

public class SpeedButton : MonoBehaviour
{
    public Image icon;
    public Sprite speed1Icon;
    public Sprite speed2Icon;
    public Sprite speed3Icon;

    int curIndex = 0;            // 0→x1, 1→x2, 2→x3
    readonly float[] speeds = { 1f, 2f, 3f };

    void Start()
    {
        // đọc speed hiện tại để icon khớp (mặc định x1 nếu ngoài [1..3])
        float s = Mathf.Clamp(Time.timeScale, 1f, 3f);
        curIndex = (Mathf.Approximately(s, 2f) ? 1 :
                   (s > 2.5f ? 2 : 0));
        UpdateIcon();
    }

    public void OnClickToggleSpeed()
    {
        curIndex = (curIndex + 1) % speeds.Length;
        Time.timeScale = speeds[curIndex];
        UpdateIcon();
    }

    void UpdateIcon()
    {
        if (!icon) return;
        icon.sprite = (curIndex == 0) ? speed1Icon
                   : (curIndex == 1) ? speed2Icon
                   : speed3Icon;
    }
}
