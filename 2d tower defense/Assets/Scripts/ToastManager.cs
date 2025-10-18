using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToastManager : MonoBehaviour
{
    public static ToastManager I { get; private set; }

    [Header("Setup")]
    public RectTransform container;     // nơi sinh các toast (UI)
    public ToastItem toastPrefab;       // prefab 1 dòng toast

    [Header("Defaults")]
    public float defaultDuration = 1.6f;

    Queue<(string, float)> queue = new Queue<(string, float)>();
    bool showing;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    /// <summary>Hiện toast (kèm queue).</summary>
    public static void Show(string message, float duration = -1f)
    {
        if (!I) { Debug.LogWarning("[Toast] No ToastManager in scene."); return; }
        I.queue.Enqueue((message, duration > 0 ? duration : I.defaultDuration));
        if (!I.showing) I.StartCoroutine(I.RunQueue());
    }

    IEnumerator RunQueue()
    {
        showing = true;
        while (queue.Count > 0)
        {
            var (msg, dur) = queue.Dequeue();
            var ti = Instantiate(toastPrefab, container);
            ti.Show(msg, dur);
            // chờ item tự Destroy -> nghe callback
            yield return new WaitUntil(() => ti == null);
        }
        showing = false;
    }
}
