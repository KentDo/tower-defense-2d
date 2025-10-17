using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private bool destroyOnDeath = false;

    public int Max => maxHealth;
    public int Current { get; private set; }
    public bool IsDead => Current <= 0;

    // Sự kiện cho UI/logic khác (nếu cần nghe)
    public event Action<Health, int> onDamaged;   // (self, damage)
    public event Action<Health, int> onHealed;    // (self, amount)
    public event Action<Health> onChanged;        // bất kỳ thay đổi
    public event Action<Health> onDied;           // gọi 1 lần khi chết

    bool diedInvoked = false;

    void Awake()
    {
        if (Current <= 0) Current = Mathf.Max(1, maxHealth);
    }

    // ===== API dùng khi spawn/scale theo wave =====
    public void SetMax(int newMax, bool refill = true)
    {
        maxHealth = Mathf.Max(1, newMax);
        if (refill) Current = maxHealth;
        else Current = Mathf.Clamp(Current, 0, maxHealth);
        diedInvoked = false;
        onChanged?.Invoke(this);
    }

    public void SetCurrent(int value)
    {
        Current = Mathf.Clamp(value, 0, maxHealth);
        diedInvoked = false;
        onChanged?.Invoke(this);
    }
    // =============================================

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || IsDead) return;
        Current = Mathf.Clamp(Current - damage, 0, maxHealth);
        onDamaged?.Invoke(this, damage);
        onChanged?.Invoke(this);

        if (Current <= 0 && !diedInvoked)
        {
            diedInvoked = true;
            onDied?.Invoke(this);
            if (destroyOnDeath) Destroy(gameObject);
        }
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
    }

    // Hữu ích khi dùng pooling: mỗi lần bật lại, nạp máu
    void OnEnable()
    {
        if (Current <= 0)
        {
            Current = maxHealth;
            diedInvoked = false;
            onChanged?.Invoke(this);
        }
    }
}
