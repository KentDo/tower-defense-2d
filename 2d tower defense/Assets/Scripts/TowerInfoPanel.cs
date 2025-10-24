using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class TowerInfoPanel : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text txtName;
    public TMP_Text txtLevel;
    public TMP_Text txtDamage;
    public TMP_Text txtFireRate;
    public TMP_Text txtRange;
    public Button btnUpgrade;
    public Button btnSell;

    private TMP_Text txtUpgradeLabel;
    private TMP_Text txtSellLabel;

    private Tower currentTower;
    public Tower CurrentTower => currentTower;

    void Start()
    {
        Hide();

        // ✅ Gán label text cho nút
        txtUpgradeLabel = btnUpgrade.GetComponentInChildren<TMP_Text>();
        txtSellLabel = btnSell.GetComponentInChildren<TMP_Text>();

        btnUpgrade.onClick.AddListener(OnUpgrade);
        btnSell.onClick.AddListener(OnSell);
    }

    public void Show(Tower tower)
    {
        if (tower == null) return;

        Debug.Log($"[TowerInfoPanel] Show info for {tower.name}");

        // ✅ Bỏ đăng ký cũ (nếu đang xem Tower khác)
        if (currentTower != null)
            currentTower.onStatsChanged -= UpdateDisplay;

        currentTower = tower;

        // ✅ Đăng ký cập nhật UI mỗi khi Tower thay đổi stats
        currentTower.onStatsChanged += UpdateDisplay;

        gameObject.SetActive(true);

        // ✅ Cập nhật toàn bộ thông tin khi mở panel lần đầu
        UpdateDisplay(currentTower);
    }

    void UpdateDisplay(Tower t)
    {
        if (!t) return;

        txtName.text = t.towerName;
        txtLevel.text = $"Level: {t.level}";
        txtDamage.text = $"DMG: {t.damage:F1}";
        txtFireRate.text = $"ROF: {t.fireRate:F2}/s";
        txtRange.text = $"Range: {t.range:F1}";

        int upgradeCost = Mathf.RoundToInt(t.upgradeCost);
        int sellValue = Mathf.RoundToInt(upgradeCost * t.sellPercent);

        // ✅ Text nút Upgrade & Sell chính xác
        if (txtUpgradeLabel) txtUpgradeLabel.text = $"Upgrade ({upgradeCost})";
        if (txtSellLabel) txtSellLabel.text = $"Sell (+{sellValue})";

        // ✅ disable button nếu không đủ tiền
        if (BuildManager.I != null)
            btnUpgrade.interactable = BuildManager.I.CanAfford(upgradeCost);
    }

    public void Hide()
    {
        if (currentTower != null)
            currentTower.onStatsChanged -= UpdateDisplay;

        currentTower = null;
        gameObject.SetActive(false);
    }

    void OnUpgrade()
    {
        if (currentTower && currentTower.Upgrade())
        {
            UpdateDisplay(currentTower);
        }
    }

    void OnSell()
    {
        if (currentTower)
        {
            currentTower.Sell();
            Hide();
        }
    }

    // ✅ Ẩn panel khi click ra ngoài
    void Update()
    {
        if (!gameObject.activeSelf)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 world = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            var hit = Physics2D.OverlapPoint(world);

            if (hit)
            {
                Tower t = hit.GetComponent<Tower>();
                if (t != null)
                {
                    if (t != currentTower)
                        Show(t);
                    return;
                }
            }

            Hide();
        }
    }
}
