using UnityEngine;
using UnityEngine.UI;

public class SpeedButton : MonoBehaviour
{
    public Image icon;
    public Sprite speed1Icon;
    public Sprite speed2Icon;
    public Sprite speed3Icon;
    private int curSpeed = 1;

    public void OnClickToggleSpeed()
    {
        curSpeed = curSpeed == 3 ? 1 : curSpeed + 1;
        Time.timeScale = curSpeed;
        UpdateIcon();
    }

    void Start()
    {
        curSpeed = 1;
        Time.timeScale = 1f;
        UpdateIcon();
    }

    void UpdateIcon()
    {
        if (icon == null) return;
        if (curSpeed == 1) icon.sprite = speed1Icon;
        else if (curSpeed == 2) icon.sprite = speed2Icon;
        else if (curSpeed == 3) icon.sprite = speed3Icon;
    }
}
