using UnityEngine;
using System.Collections;
using TMPro;

public class StartWaveButton : MonoBehaviour
{
    [Header("References")]
    public EnemySpawner spawner;
    public GameObject rootToHide;        // Panel chứa nút Start
    public TMP_Text countdownText;       // Text hiển thị đếm ngược (có thể đặt trên nút)
    [Header("Settings")]
    public float autoStartDelay = 5f;    // Thời gian chờ trước khi auto start

    void Awake()
    {
        if (!spawner) spawner = FindObjectOfType<EnemySpawner>();
    }

    void Start()
    {
        // Bắt đầu coroutine đếm ngược
        StartCoroutine(AutoStartAfterDelay());
    }

    IEnumerator AutoStartAfterDelay()
    {
        float remaining = autoStartDelay;

        // Đếm ngược từng giây
        while (remaining > 0f)
        {
            if (countdownText)
                countdownText.text = $"Bắt đầu sau: {Mathf.CeilToInt(remaining)}";

            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }

        // Khi hết thời gian, tự start wave
        if (spawner && !spawner.IsRunning)
        {
            spawner.StartWavesByButton();
        }

        if (rootToHide)
            rootToHide.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    public void OnClickStart()
    {
        if (!spawner) return;

        // Nếu người chơi bấm sớm hơn, hủy đếm ngược
        StopAllCoroutines();

        spawner.StartWavesByButton();

        if (rootToHide)
            rootToHide.SetActive(false);
        else
            gameObject.SetActive(false);
    }
}
