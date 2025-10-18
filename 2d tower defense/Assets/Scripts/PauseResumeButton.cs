using TMPro;
using UnityEngine;

public class PauseResumeButton : MonoBehaviour
{
    public TextMeshProUGUI label;
    bool paused;

    public void OnClickToggle()
    {
        paused = !paused;
        Time.timeScale = paused ? 0f : 1f;
        if (label) label.text = paused ? "Resume" : "Pause";
    }
}
