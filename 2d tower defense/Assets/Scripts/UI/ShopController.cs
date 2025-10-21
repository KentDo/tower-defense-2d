using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopController : MonoBehaviour
{
    public ToastBinder toast;

    [System.Serializable]
    public class TowerButton
    {
        public Button button;
        public TMP_Text nameText;
        public TMP_Text priceText;
        public int index; // index của tower trong BuildManager
    }

    [Header("Towers")]
    public TowerButton[] towerButtons;

    private BuildManager buildManager;

    void Start()
    {
        buildManager = FindObjectOfType<BuildManager>();
        if (!toast) toast = FindObjectOfType<ToastBinder>();

        // Cập nhật UI ngay khi có thay đổi số tiền
        buildManager.onCoinsChanged += _ => UpdateButtons();

        // Gán nút
        foreach (var btn in towerButtons)
        {
            int idx = btn.index;
            btn.button.onClick.AddListener(() => OnTowerSelected(idx));
        }

        UpdateButtons();
    }

    void UpdateButtons()
    {
        for (int i = 0; i < towerButtons.Length; i++)
        {
            TowerButton btn = towerButtons[i];
            if (btn.priceText) btn.priceText.text = $"{buildManager.towers[btn.index].cost}$";
            if (btn.nameText) btn.nameText.text = buildManager.towers[btn.index].name;

            // Khóa nếu không đủ tiền
            bool canAfford = buildManager.CanAfford(buildManager.towers[btn.index].cost);
            btn.button.interactable = canAfford;
        }
    }

    void OnTowerSelected(int index)
    {
        TowerItem tower = buildManager.towers[index];

        if (!buildManager.CanAfford(tower.cost))
        {
            toast.Show("💸 Không đủ tiền!");
            return;
        }

        buildManager.SelectIndex(index);
    }
}
