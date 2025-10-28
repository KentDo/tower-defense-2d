using UnityEngine;
using UnityEngine.UI;   // thêm namespace này

public class PauseResumeButton : MonoBehaviour
{
    public Image icon;          // Kéo Image component vào đây (BtnPause)
    public Sprite pauseSprite;  // PNG pause
    public Sprite resumeSprite; // PNG resume/play

    bool paused;

    public void OnClickToggle()
    {
        paused = !paused;
        Time.timeScale = paused ? 0f : 1f;
        if (icon) icon.sprite = paused ? resumeSprite : pauseSprite;
    }
}
