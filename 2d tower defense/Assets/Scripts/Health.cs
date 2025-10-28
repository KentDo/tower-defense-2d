using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private bool destroyOnDeath = false;

    public int Max => maxHealth;
    public int Current { get; private set; }
    public bool IsDead => Current <= 0;

    public event Action<Health, int> onDamaged;   // (self, damage)
    public event Action<Health, int> onHealed;    // (self, amount)
    public event Action<Health> onChanged;        // bất kỳ thay đổi
    public event Action<Health> onDied;           // gọi 1 lần khi chết

    private bool diedInvoked = false;

    void Awake()
    {
        // Khởi tạo HP nếu chưa có
        if (Current <= 0) Current = Mathf.Max(1, maxHealth);
    }

    // ===== API dùng khi spawn/scale theo wave =====
    public void SetMax(int newMax, bool refill = true)
    {
        maxHealth = Mathf.Max(1, newMax);
        if (refill) Current = maxHealth;
        else Current = Mathf.Clamp(Current, 0, maxHealth);

        onChanged?.Invoke(this);
        CheckDeathAndNotify(); // <— đảm bảo bắn chết nếu Current == 0
    }

    public void SetCurrent(int value)
    {
        Current = Mathf.Clamp(value, 0, maxHealth);
        onChanged?.Invoke(this);
        CheckDeathAndNotify(); // <— đảm bảo bắn chết nếu Current == 0
    }
    // =============================================

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || IsDead) return;
        int before = Current;
        Current = Mathf.Clamp(Current - damage, 0, maxHealth);

        int dealt = before - Current;
        if (dealt > 0) onDamaged?.Invoke(this, dealt);

        onChanged?.Invoke(this);
        CheckDeathAndNotify();
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead) return;
        int before = Current;
        Current = Mathf.Clamp(Current + amount, 0, maxHealth);

        int healed = Current - before;
        if (healed > 0)
        {
            onHealed?.Invoke(this, healed);
            onChanged?.Invoke(this);
        }
        // Heal không cần CheckDeathAndNotify
    }

    // Gọi chết chủ động (nếu muốn hạ gục ngay)
    public void Kill()
    {
        if (IsDead && diedInvoked) return;
        Current = 0;
        onChanged?.Invoke(this);
        CheckDeathAndNotify();
    }

    void OnEnable()
    {
        // Pooling: nếu revive, nạp lại HP
        if (Current <= 0)
        {
            Current = maxHealth;
            diedInvoked = false;
            onChanged?.Invoke(this);
        }
    }

    // ===== Core: đảm bảo bắn chết đúng 1 lần và (tuỳ chọn) Destroy =====
    private void CheckDeathAndNotify()
    {
        if (Current > 0) return;
        if (diedInvoked) return;

        diedInvoked = true;
        onDied?.Invoke(this);         // EnemyController sẽ nghe và chơi anim chết
        if (destroyOnDeath) Destroy(gameObject); // KHÔNG khuyến nghị nếu muốn thấy anim
    }
}
