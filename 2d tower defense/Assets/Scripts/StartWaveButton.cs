using UnityEngine;

public class StartWaveButton : MonoBehaviour
{
    public EnemySpawner spawner;
    public GameObject rootToHide;   // panel chứa nút Start (ẩn khi bấm)

    void Awake()
    {
        if (!spawner) spawner = FindObjectOfType<EnemySpawner>();
    }

    public void OnClickStart()
    {
        if (!spawner) return;
        spawner.StartWavesByButton();
        if (rootToHide) rootToHide.SetActive(false);
        else gameObject.SetActive(false);
    }
}
