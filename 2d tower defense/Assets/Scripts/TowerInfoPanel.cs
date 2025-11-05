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
        txtUpgradeLabel = btnUpgrade ? btnUpgrade.GetComponentInChildren<TMP_Text>(true) : null;
        txtSellLabel = btnSell ? btnSell.GetComponentInChildren<TMP_Text>(true) : null;

        if (btnUpgrade) btnUpgrade.onClick.AddListener(OnUpgrade);
        if (btnSell) btnSell.onClick.AddListener(OnSell);

        Hide();
    }

    public void Show(Tower tower)
    {
        if (tower == null) return;

        if (currentTower != null)
        {
            currentTower.onStatsChanged -= UpdateDisplay;
        }

        currentTower = tower;
        currentTower.onStatsChanged += UpdateDisplay;

        gameObject.SetActive(true);
        UpdateDisplay(currentTower);
    }

    void UpdateDisplay(Tower t)
    {
        if (!t) return;

        if (txtName) txtName.text = t.towerName;
        if (txtLevel) txtLevel.text = $"Level: {t.level}";
        if (txtDamage) txtDamage.text = $"DMG: {t.damage:F1}";
        if (txtFireRate) txtFireRate.text = $"ROF: {t.fireRate:F2}/s";
        if (txtRange) txtRange.text = $"Range: {t.range:F1}";

        int upgradeCost = Mathf.RoundToInt(t.upgradeCost);
        int sellValue = Mathf.RoundToInt(upgradeCost * t.sellPercent);

        if (txtUpgradeLabel) txtUpgradeLabel.text = $"Upgrade ({upgradeCost})";
        if (txtSellLabel) txtSellLabel.text = $"Sell (+{sellValue})";

        if (btnUpgrade)
        {
            bool canUpgrade = BuildManager.I == null || BuildManager.I.CanAfford(upgradeCost);
            btnUpgrade.interactable = canUpgrade;
        }
    }

    public void Hide()
    {
        if (currentTower != null)
        {
            currentTower.onStatsChanged -= UpdateDisplay;
        }

        currentTower = null;
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
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

    void Update()
    {
        if (!gameObject.activeSelf)
            return;

        var mouse = Mouse.current;
        if (mouse == null)
            return;

        if (!mouse.leftButton.wasPressedThisFrame)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        var mainCamera = Camera.main;
        if (mainCamera == null)
            return;

        Vector2 world = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());
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
