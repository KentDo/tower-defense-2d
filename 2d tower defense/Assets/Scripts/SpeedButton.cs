using UnityEngine;

public class SpeedButton : MonoBehaviour
{
    [Range(0.1f, 5f)] public float speed = 1f; // 1, 2, 3
    public void OnClickSetSpeed()
    {
        // khi đang Pause (0), resume trước
        if (Mathf.Approximately(Time.timeScale, 0f)) Time.timeScale = 1f;
        Time.timeScale = speed;
    }
}
