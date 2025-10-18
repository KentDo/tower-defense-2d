// Assets/Scripts/Core/BuildManager.cs
using UnityEngine;
using System;

[Serializable]
public class TowerItem
{
    public string name;
    public GameObject prefab;
    public int cost = 50;
    public float placeRadius = 0.45f; // dùng cho Placer
}

public class BuildManager : MonoBehaviour
{
    public static BuildManager I { get; private set; }
    void Awake() { I = this; }

    [Header("Shop Data")]
    public TowerItem[] towers;
    public int coins = 200;

    public event Action<int> onCoinsChanged;   // HUD/Shop subscribe
    public event Action<int> onSelectionChanged; // Shop/Placer subscribe

    int selected = -1;
    public int SelectedIndex => selected;
    public TowerItem Selected => (selected >= 0 && selected < towers.Length) ? towers[selected] : null;

    public void SelectIndex(int idx)
    {
        if (idx < 0 || idx >= towers.Length) { Unselect(); return; }
        selected = idx;
        onSelectionChanged?.Invoke(selected);
    }
    public void Unselect()
    {
        selected = -1;
        onSelectionChanged?.Invoke(selected);
    }

    public bool CanAfford(int cost) => coins >= cost;
    public bool Spend(int cost)
    {
        if (!CanAfford(cost)) return false;
        coins -= cost; onCoinsChanged?.Invoke(coins);
        return true;
    }
    public void Add(int amount)
    {
        if (amount <= 0) return;
        coins += amount; onCoinsChanged?.Invoke(coins);
    }
}
