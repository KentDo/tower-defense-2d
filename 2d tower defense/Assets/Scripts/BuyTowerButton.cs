using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Gắn trực tiếp lên UI Image/Text… để trở thành nút mua trụ.
/// Khi click trái: nếu đủ tiền → BuildManager.SelectIndex(index) (TowerPlacer sẽ lo đặt).
/// Hỗ trợ hiển thị giá (Text/TMP) và overlay khóa khi không đủ tiền.
/// </summary>
[RequireComponent(typeof(Graphic))] // Image/Text… để nhận raycast
public class BuyTowerButton : MonoBehaviour, IPointerClickHandler
{
    [Header("Tower Index (khớp BuildManager.towers)")]
    public int index = 0;

    [Header("UI (tuỳ chọn)")]
    public TMP_Text priceTMP;     // dùng TMP
    public Text priceText;        // hoặc Text thường
    public GameObject lockOverlay; // ảnh mờ khi không đủ tiền

    [Header("Feedback (tuỳ chọn)")]
    public bool flashRedOnDeny = true;
    public float flashTime = 0.12f;

    BuildManager bm;
    TowerItem data;
    Color? priceOrig;

    void Awake()
    {
        // Auto-find BuildManager
        bm = BuildManager.I ?? FindObjectOfType<BuildManager>();

        // Nhớ màu gốc của price
        if (priceTMP) priceOrig = priceTMP.color;
        else if (priceText) priceOrig = priceText.color;

        // Bảo đảm Graphic nhận click
        var g = GetComponent<Graphic>();
        g.raycastTarget = true;
    }

    void OnEnable()
    {
        TryBindData();
        if (bm) bm.onCoinsChanged += OnCoinsChanged;
        RefreshAffordable();
    }

    void OnDisable()
    {
        if (bm) bm.onCoinsChanged -= OnCoinsChanged;
    }

    void TryBindData()
    {
        if (!bm || bm.towers == null || index < 0 || index >= bm.towers.Length)
        {
            data = null;
            return;
        }
        data = bm.towers[index];

        // Hiển thị giá
        string costStr = (data != null) ? data.cost.ToString() : "";
        if (priceTMP)  priceTMP.text  = costStr;
        if (priceText) priceText.text = costStr;
    }

    void OnCoinsChanged(int _)
    {
        RefreshAffordable();
    }

    void RefreshAffordable()
    {
        if (!bm || data == null) return;
        bool ok = bm.CanAfford(data.cost);

        if (lockOverlay) lockOverlay.SetActive(!ok);

        if (priceOrig.HasValue)
        {
            var onCol  = new Color(1f, 0.95f, 0.6f);
            var offCol = new Color(0.7f, 0.7f, 0.7f);
            if (priceTMP)  priceTMP.color  = ok ? onCol : offCol;
            if (priceText) priceText.color = ok ? onCol : offCol;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (!bm)
        {
            Debug.LogWarning("[BuyTowerButton] BuildManager null");
            return;
        }
        if (data == null)
        {
            Debug.LogWarning($"[BuyTowerButton] towers[{index}] null hoặc index sai");
            return;
        }

        if (bm.CanAfford(data.cost))
        {
            // Không trừ tiền ở đây. Chỉ chọn loại trụ,
            // TowerPlacer sẽ hiện ghost và trừ tiền khi đặt thành công.
            bm.SelectIndex(index);
        }
        else if (flashRedOnDeny)
        {
            StopAllCoroutines();
            StartCoroutine(FlashPrice(Color.red));
        }
    }

    System.Collections.IEnumerator FlashPrice(Color c)
    {
        if (priceTMP)  priceTMP.color  = c;
        if (priceText) priceText.color = c;
        yield return new WaitForSeconds(flashTime);
        if (priceOrig.HasValue)
        {
            if (priceTMP)  priceTMP.color  = priceOrig.Value;
            if (priceText) priceText.color = priceOrig.Value;
        }
    }
}
