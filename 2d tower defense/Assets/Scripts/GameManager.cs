// GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int gold;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    // Không cần subscribe gì cả

    // Tuỳ chọn: vẫn giữ để gọi ở nơi khác nếu muốn
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        gold += amount;
        var bm = BuildManager.I;
        if (bm != null) bm.Add(amount);   // đẩy sang BuildManager để HUDCoins nhận onCoinsChanged
    }
}
