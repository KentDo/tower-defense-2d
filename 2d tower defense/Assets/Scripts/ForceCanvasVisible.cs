using UnityEngine;

/// <summary>
/// Script đơn giản để đảm bảo Canvas của thanh máu luôn hiển thị đúng
/// Gắn vào GameObject có Canvas component (cùng với HealthBarFollow)
/// </summary>
[RequireComponent(typeof(Canvas))]
public class ForceCanvasVisible : MonoBehaviour
{
    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        SetupCanvas();
    }

    void Start()
    {
        SetupCanvas();
    }

    void SetupCanvas()
    {
        if (canvas == null) return;

        // Đảm bảo Canvas được setup đúng
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100; // cao hơn enemy sprite để không bị che

        // Đảm bảo canvas được bật
        canvas.enabled = true;
        gameObject.SetActive(true);
    }

    void LateUpdate()
    {
        // Đảm bảo canvas luôn được bật
        if (canvas != null && !canvas.enabled)
        {
            canvas.enabled = true;
        }
    }
}
