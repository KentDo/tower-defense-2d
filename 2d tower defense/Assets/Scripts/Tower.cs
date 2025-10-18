using UnityEngine;
using System;

public class Tower : MonoBehaviour
{
    [Header("Info & Economy")]
    public string towerName = "Archer";
    public int startCoinsCost = 50;         // giá mua (do BuildManager trừ khi đặt)
    public int upgradeCost = 50;            // giá nâng cấp hiện tại
    [Range(0f, 1f)] public float sellPercent = 0.6f;

    public event Action<Tower> onStatsChanged; // Panel UI subscribe

    [Header("Weapon")]
    public Transform muzzle;
    public GameObject projectile;
    public Animator weaponAnimator;
    public float fireRate = 1.2f;
    public float range = 4f;
    public float damage = 1f;               // thêm: damage cơ bản (nếu projectile đọc)
    public float turnSpeed = 360f;
    public LayerMask enemyMask;

    Transform target;
    float cd;

    [Header("Renderers & Sprites")]
    public SpriteRenderer baseSR;
    public SpriteRenderer weaponSR;
    public Transform weaponPivot;

    public Sprite[] baseLevels;
    public Sprite[] weaponLevels;
    public Vector2[] weaponOffsets;

    [Header("Level & Scaling")]
    [Range(0, 2)] public int startLevel = 0;
    public int level { get; private set; } = 0;  // API: public get
    // Hệ số tăng theo mỗi cấp (có thể chỉnh tùy loại trụ)
    public float dmgPerLevelMul = 1.25f;
    public float firePerLevelMul = 1.10f;
    public float rangePerLevelAdd = 0.25f;
    public float upgradeCostMul = 1.40f;

    public int Level => level; // giữ alias cũ nếu nơi khác đang gọi

    void Awake()
    {
        SetLevel(startLevel);
    }

    void Update()
    {
        cd -= Time.deltaTime;

        // tìm target trong tầm
        if (!target || Vector2.Distance(transform.position, target.position) > range)
            target = FindNearestEnemy();

        bool inRange = target != null;
        if (weaponAnimator) weaponAnimator.SetBool("HasTarget", inRange);
        if (!inRange) return;

        // xoay nòng (WeaponPivot) thay vì root
        Vector2 aim = (Vector2)target.position - (Vector2)transform.position;
        float ang = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
        Quaternion to = Quaternion.AngleAxis(ang - 90f, Vector3.forward);

        if (weaponPivot != null)
        {
            weaponPivot.rotation = Quaternion.RotateTowards(
                weaponPivot.rotation, to, turnSpeed * Time.deltaTime
            );
        }

        // bắn
        if (cd <= 0f)
        {
            cd = 1f / Mathf.Max(0.01f, fireRate);
            Shoot();
        }
    }

    Transform FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, enemyMask);
        float best = float.PositiveInfinity; Transform pick = null;
        foreach (var h in hits)
        {
            float d = (h.transform.position - transform.position).sqrMagnitude;
            if (d < best) { best = d; pick = h.transform; }
        }
        return pick;
    }

    void Shoot()
    {
        if (!target || !projectile || !muzzle || !weaponPivot) return;

        var go = Instantiate(projectile, muzzle.position, weaponPivot.rotation);

        // Nếu projectile hỗ trợ damage, truyền vào
        var homing = go.GetComponent<ArrowHoming>();
        if (homing)
        {
            homing.Init(target, enemyMask);
            // Nếu ArrowHoming có field damage, set thêm:
            // homing.damage = Mathf.RoundToInt(damage);
        }

        if (weaponAnimator) weaponAnimator.SetTrigger("Shoot");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    // ========== API NÂNG CẤP/BÁN (Chuẩn hóa) ==========
    public bool Upgrade()
    {
        var bm = BuildManager.I;
        if (!bm) { Debug.LogWarning("[Tower] BuildManager not found."); return false; }
        if (!bm.Spend(upgradeCost)) return false;

        level++;
        damage *= dmgPerLevelMul;
        fireRate *= firePerLevelMul;
        range += rangePerLevelAdd;
        upgradeCost = Mathf.RoundToInt(upgradeCost * upgradeCostMul);

        // Cập nhật visual theo level
        ApplyLevelSprites();

        onStatsChanged?.Invoke(this);
        return true;
    }

    public int Sell()
    {
        int refund = Mathf.RoundToInt(upgradeCost * sellPercent);
        BuildManager.I?.Add(refund);
        Destroy(gameObject);
        return refund;
    }

    // ========== Level & Sprites ==========
    public void SetLevel(int lv)
    {
        // clamp theo số sprite (nếu bạn có ít sprite hơn số level thực)
        int maxSpriteLevel = Mathf.Min(
            baseLevels != null ? baseLevels.Length - 1 : 0,
            weaponLevels != null ? weaponLevels.Length - 1 : 0
        );
        level = Mathf.Clamp(lv, 0, Mathf.Max(0, maxSpriteLevel));

        ApplyLevelSprites();

        // reset chỉ số theo level khởi điểm (nếu muốn scale ngay từ Start)
        // *Giữ nguyên fireRate/range/damage bạn set trong Inspector cho level 0*
        for (int i = 0; i < level; i++)
        {
            damage *= dmgPerLevelMul;
            fireRate *= firePerLevelMul;
            range += rangePerLevelAdd;
            upgradeCost = Mathf.RoundToInt(upgradeCost * upgradeCostMul);
        }
    }

    void ApplyLevelSprites()
    {
        if (baseSR && baseLevels != null && baseLevels.Length > 0)
        {
            int i = Mathf.Clamp(level, 0, baseLevels.Length - 1);
            baseSR.sprite = baseLevels[i];
        }
        if (weaponSR && weaponLevels != null && weaponLevels.Length > 0)
        {
            int i = Mathf.Clamp(level, 0, weaponLevels.Length - 1);
            weaponSR.sprite = weaponLevels[i];
        }
        if (weaponPivot && weaponOffsets != null && weaponOffsets.Length > 0)
        {
            int i = Mathf.Clamp(level, 0, weaponOffsets.Length - 1);
            weaponPivot.localPosition = weaponOffsets[i];
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            // preview sprite theo startLevel trên Editor
            int prev = level;
            level = startLevel;
            ApplyLevelSprites();
            level = prev;
        }
    }
#endif
}
