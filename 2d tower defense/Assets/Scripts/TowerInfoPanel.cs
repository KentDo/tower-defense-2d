using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        // Tự lấy text con nếu chưa set thủ công
        txtUpgradeLabel = btnUpgrade.GetComponentInChildren<TMP_Text>();
        txtSellLabel = btnSell.GetComponentInChildren<TMP_Text>();

        btnUpgrade.onClick.AddListener(OnUpgrade);
        btnSell.onClick.AddListener(OnSell);
    }

    public void Show(Tower tower)
    {
        if (currentTower != null)
            currentTower.onStatsChanged -= UpdateDisplay;

        currentTower = tower;
        currentTower.onStatsChanged += UpdateDisplay;
        gameObject.SetActive(true);
        UpdateDisplay(currentTower);
    }

    public void Hide()
    {
        if (currentTower != null)
            currentTower.onStatsChanged -= UpdateDisplay;
        currentTower = null;
        gameObject.SetActive(false);
    }

    void UpdateDisplay(Tower t)
    {
        if (!t) return;

        txtName.text = t.towerName;
        txtLevel.text = $"Level: {t.level}";

        // ========== TÍNH CHỈ SỐ SAU KHI NÂNG CẤP ==========
        float nextDmg = t.damage * t.dmgPerLevelMul;
        float nextFire = t.fireRate * t.firePerLevelMul;
        float nextRange = t.range + t.rangePerLevelAdd;

        // hiển thị hiện tại → sau nâng cấp
        txtDamage.text = $"DMG: {t.damage:F1} → <color=#00FF00>{nextDmg:F1}</color>";
        txtFireRate.text = $"ROF: {t.fireRate:F2} → <color=#00FF00>{nextFire:F2}</color>/s";
        txtRange.text = $"Range: {t.range:F2} → <color=#00FF00>{nextRange:F2}</color>";

        // ========== CẬP NHẬT NÚT ==========
        int upgrade = Mathf.RoundToInt(t.upgradeCost);
        int sell = Mathf.RoundToInt(t.upgradeCost * t.sellPercent);

        if (txtUpgradeLabel) txtUpgradeLabel.text = $"Upgrade ({upgrade})";
        if (txtSellLabel) txtSellLabel.text = $"Sell (+{sell})";

        // Nếu không đủ tiền -> disable
        if (BuildManager.I)
            btnUpgrade.interactable = BuildManager.I.CanAfford(upgrade);
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

    // Ẩn panel khi click ra ngoài
    void Update()
    {
        if (gameObject.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                var hit = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                if (!hit || hit.GetComponent<Tower>() == null)
                    Hide();
            }
        }
    }
}
