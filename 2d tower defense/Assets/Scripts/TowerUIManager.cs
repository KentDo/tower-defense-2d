using UnityEngine;
using UnityEngine.UI;

public class TowerUIManager : MonoBehaviour
{
    public static TowerUIManager Instance;

    [Header("Refs")]
    public GameObject uiPanel;      // TowerUI
    public Button upgradeButton;    // Btn_Upgrade
    public Button sellButton;       // Btn_Sell

    private Tower target;           // tower đang được chọn

    void Awake()
    {
        Instance = this;
        Hide();
    }

    // Gọi khi click vào 1 tower
    public void Show(Tower tower)
    {
        target = tower;
        uiPanel.SetActive(true);

        // Đặt panel nổi ngay trên tower
        Vector3 screen = Camera.main.WorldToScreenPoint(tower.transform.position);
        uiPanel.transform.position = screen + new Vector3(0f, 80f, 0f);

        // Nếu đã max level thì disable nút Upgrade
        upgradeButton.interactable = (tower.level < tower.maxLevel);
    }

    public void Hide()
    {
        uiPanel.SetActive(false);
        target = null;
    }

    // Gắn vào OnClick của Btn_Upgrade
    public void OnUpgradePressed()
    {
        if (target == null) return;
        target.Upgrade();      // logic trong Tower.cs
        Show(target);          // refresh lại trạng thái nút
    }

    // Gắn vào OnClick của Btn_Sell
    public void OnSellPressed()
    {
        if (target == null) return;
        target.Sell();         // logic trong Tower.cs
        Hide();                // ẩn UI vì tower đã bị destroy
    }
    



}
