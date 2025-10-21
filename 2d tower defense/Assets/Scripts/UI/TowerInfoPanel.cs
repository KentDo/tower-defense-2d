using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerInfoPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text nameText;
    public TMP_Text levelText;
    public TMP_Text dmgText;
    public TMP_Text rofText;
    public TMP_Text rangeText;
    public TMP_Text upgradeCostText;
    public TMP_Text sellValueText;

    public Button btnUpgrade;
    public Button btnSell;

    private Tower targetTower;
    private BuildManager buildManager;
    private ToastBinder toast;

    void Start()
    {
        buildManager = FindObjectOfType<BuildManager>();
        toast = FindObjectOfType<ToastBinder>();

        btnUpgrade.onClick.AddListener(OnUpgrade);
        btnSell.onClick.AddListener(OnSell);
    }

    public void Show(Tower tower)
    {
        targetTower = tower;
        gameObject.SetActive(true);
        Refresh();
    }

    void Refresh()
    {
        if (targetTower == null) return;

        nameText.text = targetTower.towerName;
        levelText.text = $"Lv {targetTower.level}";
        dmgText.text = $"DMG: {targetTower.damage}";
        rofText.text = $"ROF: {targetTower.fireRate:F1}";
        rangeText.text = $"Range: {targetTower.range:F1}";
        upgradeCostText.text = $"Upgrade: {targetTower.upgradeCost}$";

        // Tính trước giá bán dựa theo logic Tower.cs
        int estimatedSell = Mathf.RoundToInt(targetTower.upgradeCost * targetTower.sellPercent);
        sellValueText.text = $"Sell: +{estimatedSell}$";

        // Khóa nút nếu không đủ tiền để nâng cấp
        bool canUpgrade = buildManager.CanAfford(targetTower.upgradeCost);
        btnUpgrade.interactable = canUpgrade;
    }

    void OnUpgrade()
    {
        if (targetTower == null) return;

        if (!buildManager.CanAfford(targetTower.upgradeCost))
        {
            toast.Show("❌ Không đủ tiền để nâng cấp!");
            return;
        }

        // Tower tự gọi Spend() bên trong Upgrade()
        bool upgraded = targetTower.Upgrade();
        if (upgraded)
        {
            toast.Show($"⚙️ Đã nâng cấp {targetTower.towerName}!");
            Refresh();
        }
    }

    void OnSell()
    {
        if (targetTower == null) return;

        int refund = targetTower.Sell(); // Tower tự cộng tiền + phá object
        toast.Show($"💰 Đã bán {targetTower.towerName} (+{refund}$)");
        gameObject.SetActive(false);
    }
}
