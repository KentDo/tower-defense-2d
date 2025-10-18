using UnityEngine;

public class HealthBarFollow : MonoBehaviour
{
    public Transform target;                    // để trống: script sẽ tự tìm Health ở cha
    public Vector3 worldOffset = new Vector3(0f, 0.5f, 0f);

    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            // Đảm bảo Canvas hiển thị đúng
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100; // cao hơn enemy sprite
        }
    }

    void OnEnable()
    {
        if (!target)
        {
            var h = GetComponentInParent<Health>();
            if (h) target = h.transform;
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        // Cập nhật vị trí và đảm bảo không bị xoay
        transform.position = target.position + worldOffset;
        transform.rotation = Quaternion.identity; // luôn giữ rotation = 0
    }
}
