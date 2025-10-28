using UnityEngine;
using UnityEngine.EventSystems;

public class HideTowerUIOnClickOutside : MonoBehaviour
{
    void Update()
    {
        // Kiểm tra khi người chơi nhấn chuột trái
        if (Input.GetMouseButtonDown(0))
        {
            // Nếu click vào UI (nút, panel, button...) thì bỏ qua
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            // Nếu click ra chỗ trống → ẩn panel TowerUI
            if (TowerUIManager.Instance != null)
            {
                TowerUIManager.Instance.Hide();
            }
        }
    }
}
