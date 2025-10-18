using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ToastItem : MonoBehaviour
{
    public TextMeshProUGUI text;   // gán trong prefab
    public Image bg;               // optional: nền mờ
    public float fadeIn = 0.15f;
    public float hold = 1.2f;
    public float fadeOut = 0.25f;
    CanvasGroup cg;

    void Awake() { cg = GetComponent<CanvasGroup>(); }

    public void Show(string msg, float duration)
    {
        if (text) text.text = msg;
        hold = duration;
        StartCoroutine(CoRun());
    }

    IEnumerator CoRun()
    {
        cg.alpha = 0f;
        // fade in
        float t = 0;
        while (t < fadeIn) { t += Time.unscaledDeltaTime; cg.alpha = Mathf.SmoothStep(0, 1, t / fadeIn); yield return null; }
        cg.alpha = 1f;

        // hold
        t = 0;
        while (t < hold) { t += Time.unscaledDeltaTime; yield return null; }

        // fade out
        t = 0;
        while (t < fadeOut) { t += Time.unscaledDeltaTime; cg.alpha = 1f - (t / fadeOut); yield return null; }
        Destroy(gameObject);
    }
}
