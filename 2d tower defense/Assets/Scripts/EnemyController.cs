using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;       // tự lấy nếu để trống

    [Header("Path (được Spawner gán)")]
    // Một số script di chuyển cần tham chiếu này.
    public LevelManager path;

    [Header("Death / Reward (optional)")]
    [SerializeField] private int goldOnDeath = 0; // tuỳ dự án dùng hay không
    [SerializeField] private GameObject deathVfx = null;

    // Sự kiện cho logic khác nếu muốn nghe enemy chết (tuỳ chọn)
    public event Action<EnemyController> onDeath;

    // ===== API công khai (tương thích) =====
    public int CurrentHP => health ? health.Current : 0;
    public int MaxHP => health ? health.Max : 0;

    void Awake()
    {
        if (!health) health = GetComponent<Health>();
    }

    void OnValidate()
    {
        if (!health) health = GetComponent<Health>();
    }

    /// <summary>Đặt Max HP (thường gọi khi spawn / theo wave). Sẽ refill đầy.</summary>
    public void SetMaxHP(int newMax)
    {
        if (health) health.SetMax(newMax, refill: true);
    }

    /// <summary>Gây sát thương cho enemy.</summary>
    public void TakeDamage(int damage)
    {
        if (!health) return;
        health.TakeDamage(damage);
        if (health.IsDead)
        {
            Kill();
        }
    }

    /// <summary>Hồi máu (nếu cần).</summary>
    public void Heal(int amount)
    {
        if (health) health.Heal(amount);
    }

    /// <summary>Logic khi chết: FX/tiền thưởng/sự kiện…</summary>
    public void Kill()
    {
        if (deathVfx) Instantiate(deathVfx, transform.position, Quaternion.identity);

        // TODO: nếu có hệ thống vàng/điểm thì gọi ở đây
        // LevelManager.Instance.AddGold(goldOnDeath);

        onDeath?.Invoke(this); // vẫn phát sự kiện để tương thích
        Destroy(gameObject);   // hoặc trả về pool nếu dùng pooling
    }
}
