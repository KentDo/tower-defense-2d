// Assets/Scripts/Core/BuildManager.cs
using UnityEngine;
using System;

[Serializable]
public class TowerItem
{
    public string name;
    public GameObject prefab;
    public int cost = 50;
    public float placeRadius = 0.45f; // gợi ý cho Placer kiểm tra va chạm
}

public class BuildManager : MonoBehaviour
{
    // ---------- Singleton ----------
    public static BuildManager I { get; private set; }
    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    // ---------- Coins ----------
    [Header("Economy")]
    public int coins = 100;
    public Action<int> onCoinsChanged;   // bắn khi tiền thay đổi

    public bool CanAfford(int cost) => coins >= cost;

    public bool Spend(int cost)
    {
        if (!CanAfford(cost)) return false;
        coins -= cost;
        onCoinsChanged?.Invoke(coins);
        return true;
    }

    public void Add(int amount)
    {
        if (amount <= 0) return;
        coins += amount;
        onCoinsChanged?.Invoke(coins);
    }

    // ---------- Towers (chỉ 2 loại) ----------
    [Header("Towers (CHỈ 2 loại)")]
    public TowerItem[] towers = new TowerItem[2]; // set trong Inspector đúng 2 phần tử

    // ---------- Selection ----------
    public int SelectedIndex { get; private set; } = -1;
    public Action<int> onSelectionChanged; // bắn khi chọn đổi
    public TowerItem Selected
    {
        get
        {
            if (SelectedIndex < 0 || towers == null || SelectedIndex >= towers.Length) return null;
            return towers[SelectedIndex];
        }
    }

    /// <summary>
    /// Chỉ cho chọn index 0..1 và CHỈ khi đủ tiền (shop bấm sẽ gọi hàm này).
    /// Không trừ tiền ở đây — trừ khi đặt thành công (Placer).
    /// </summary>
    public void SelectIndex(int idx)
    {
        if (idx < 0 || idx > 1) return;
        if (towers == null || towers.Length == 0) return;

        if (idx >= towers.Length || towers[idx] == null || towers[idx].prefab == null) return;

        if (!CanAfford(towers[idx].cost)) return;

        SelectedIndex = idx;
        onSelectionChanged?.Invoke(SelectedIndex);
    }

    public void Unselect()
    {
        if (SelectedIndex != -1)
        {
            SelectedIndex = -1;
            onSelectionChanged?.Invoke(SelectedIndex);
        }
    }

    // ========= NEW: Reset state khi Retry =========
    public void Reinitialize()
    {
        // đủ dùng cho flow hiện tại: reset tiền/selection + phát sự kiện cho HUD
        coins = 100;
        SelectedIndex = -1;
        onSelectionChanged?.Invoke(SelectedIndex);
        onCoinsChanged?.Invoke(coins);
        Debug.Log("[BuildManager] Reinitialized after retry");
    }
}
