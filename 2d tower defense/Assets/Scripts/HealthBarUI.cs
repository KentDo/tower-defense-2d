using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("Binding")]
    public Health target;
    public Image fillImage;            // gán Image của đối tượng Fill
    public Image backgroundImage;      // gán Image của Background (tuỳ chọn)

    [Header("Behavior")]
    public bool hideWhenFull = false;           // LUÔN false để thanh máu luôn hiển thị
    [Min(0f)] public float smoothSpeed = 10f;   // 0 = cập nhật tức thời
    public Color healthColor = Color.red;       // màu thanh máu (mặc định đỏ)
    public bool useGradient = false;            // bật để dùng gradient thay vì màu cố định
    public Gradient colorByHealth;              // tuỳ chọn: đổi màu theo % máu

    float displayed01 = 1f;

    void Awake()
    {
        // Force hideWhenFull = false để tránh prefab cũ override
        hideWhenFull = false;
    }

    void OnEnable()
    {
        if (!target) target = GetComponentInParent<Health>();

        // Đặt màu đỏ cho fill image khi khởi tạo
        if (fillImage && !useGradient)
        {
            fillImage.color = healthColor;
        }

        // Đảm bảo images luôn được bật
        if (fillImage) fillImage.enabled = true;
        if (backgroundImage) backgroundImage.enabled = true;

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

        // Chỉ dùng gradient nếu useGradient = true
        if (useGradient && colorByHealth != null && colorByHealth.colorKeys.Length > 0)
            fillImage.color = colorByHealth.Evaluate(displayed01);
        else
            fillImage.color = healthColor; // dùng màu cố định

        // LUÔN hiển thị thanh máu (không ẩn)
        if (backgroundImage) backgroundImage.enabled = true;
        fillImage.enabled = true;
    }

    public void ImmediateRefresh()
    {
        if (!target || !fillImage) return;
        displayed01 = Mathf.Clamp01(target.Current / (float)target.Max);
        fillImage.fillAmount = displayed01;

        // Chỉ dùng gradient nếu useGradient = true
        if (useGradient && colorByHealth != null && colorByHealth.colorKeys.Length > 0)
            fillImage.color = colorByHealth.Evaluate(displayed01);
        else
            fillImage.color = healthColor; // dùng màu cố định

        // LUÔN hiển thị thanh máu (không ẩn)
        if (backgroundImage) backgroundImage.enabled = true;
        fillImage.enabled = true;
    }
}
