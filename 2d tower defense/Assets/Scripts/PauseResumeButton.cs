using UnityEngine;
using UnityEngine.UI;   // thêm namespace này

public class PauseResumeButton : MonoBehaviour
{
    public Image icon;          // Kéo Image component vào đây (BtnPause)
    public Sprite pauseSprite;  // PNG pause
    public Sprite resumeSprite; // PNG resume/play

    bool paused;
    float savedSpeed = 1f;      // Lưu lại tốc độ trước khi pause

    public void OnClickToggle()
    {
        paused = !paused;
        if (paused)
        {
            savedSpeed = Time.timeScale;   // Lưu lại tốc độ hiện tại (1/2/3)
            Time.timeScale = 0f;           // Dừng game
        }
        else
        {
            Time.timeScale = savedSpeed;   // Trả lại đúng tốc độ trước đó
        }
        if (icon) icon.sprite = paused ? resumeSprite : pauseSprite;
    }
}
