using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("Binding")]
    public Health target;
    public Image fillImage;            // gán Image của đối tượng Fill
    public Image backgroundImage;      // gán Image của Background (tuỳ chọn)

    [Header("Behavior")]
    public bool hideWhenFull = true;
    [Min(0f)] public float smoothSpeed = 10f;   // 0 = cập nhật tức thời
    public Gradient colorByHealth;              // tuỳ chọn: đổi màu theo % máu

    float displayed01 = 1f;

    void OnEnable()
    {
        if (!target) target = GetComponentInParent<Health>();
        ImmediateRefresh();
    }

    void Update()
    {
        if (!target || !fillImage) return;

        float t01 = Mathf.Clamp01(target.Current / (float)target.Max);
        displayed01 = (smoothSpeed > 0f)
            ? Mathf.Lerp(displayed01, t01, 1f - Mathf.Exp(-smoothSpeed * Time.unscaledDeltaTime))
            : t01;

        fillImage.fillAmount = displayed01;

        if (colorByHealth != null && colorByHealth.colorKeys.Length > 0)
            fillImage.color = colorByHealth.Evaluate(displayed01);

        bool hide = hideWhenFull && t01 >= 0.999f;
        if (backgroundImage) backgroundImage.enabled = !hide;
        fillImage.enabled = !hide;
    }

    public void ImmediateRefresh()
    {
        if (!target || !fillImage) return;
        displayed01 = Mathf.Clamp01(target.Current / (float)target.Max);
        fillImage.fillAmount = displayed01;

        if (colorByHealth != null && colorByHealth.colorKeys.Length > 0)
            fillImage.color = colorByHealth.Evaluate(displayed01);

        bool hide = hideWhenFull && displayed01 >= 0.999f;
        if (backgroundImage) backgroundImage.enabled = !hide;
        fillImage.enabled = !hide;
    }
}
