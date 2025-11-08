using UnityEngine;
using System;

public class Tower : MonoBehaviour
{
    [Header("Info & Economy")]
    public string towerName = "Archer";
    public int startCoinsCost = 50;
    public int upgradeCost = 50;
    [Range(0f, 1f)] public float sellPercent = 0.6f;

    public event Action<Tower> onStatsChanged;

    [Header("Combat")]
    public float fireRate = 1.2f;   // phát/giây
    public float range = 4f;
    public float damage = 50f;
    public float turnSpeed = 360f;   // độ/giây
    public LayerMask enemyMask;

    [Header("Shoot Points & Prefabs")]
    public Transform muzzle;         // PHẢI là con của 'weapon'
    public GameObject projectile;    // fallback projectile (nếu thiếu theo level)

    [Header("Projectile by Level (optional)")]
    public GameObject[] projectileLevels; // Prefab đạn cho từng level (index = level). Null -> dùng 'projectile'

    [Header("Hierarchy (pivot vs weapon)")]
    public Transform weaponPivot;    // KHÔNG quay (điểm gắn/offset theo level)
    public Transform weapon;         // QUAY THẰNG NÀY
    public Animator weaponAnimator;  // Animator đặt trên 'weapon'
    [Tooltip("Hiệu chỉnh hướng gốc của sprite vũ khí. 0 nếu sprite nhìn sang phải; -90 nếu nhìn lên.")]
    public float aimVisualOffsetDeg = -90f;

    [Header("Visuals by Level (optional)")]
    public SpriteRenderer baseSR;        // sprite thân
    public SpriteRenderer weaponSR;      // sprite vũ khí (nếu tách renderer)
    public Sprite[] baseLevels;          // sprite thân theo level
    public Sprite[] weaponLevels;        // sprite vũ khí theo level
    public Vector2[] weaponOffsets;      // OFFSET đặt cho weaponPivot (không quay)
    public RuntimeAnimatorController[] weaponControllers; // controller theo level (chỉ đổi khi PLAY)

    [Header("Level & Scaling")]
    [Range(0, 2)] public int startLevel = 0;
    public int level { get; private set; } = 0;
    public float dmgPerLevelMul = 1.25f;
    public float firePerLevelMul = 1.10f;
    public float rangePerLevelAdd = 0.25f;
    public float upgradeCostMul = 1.40f;

    public int Level => level;

    // Internals
    Transform target;
    float cd;

    // ========= Lifecycle =========

    void Awake()
    {
        AutoWireReferences();

        // Kiểm tra nhanh để tránh cấu hình sai
        if (!weapon) Debug.LogError("[Tower] 'weapon' (child quay) chưa được gán!", this);
        if (!weaponPivot) Debug.LogWarning("[Tower] 'weaponPivot' chưa gán (offset level sẽ không dùng).", this);
        if (muzzle && muzzle.parent != weapon)
            Debug.LogWarning("[Tower] 'muzzle' nên là CON của 'weapon' để quay theo.", this);
    }

    void Start()
    {
        // Gọi ở Start thay vì Awake để an toàn với Editor/Animator
        SetLevel(startLevel);
    }

    void AutoWireReferences()
    {
        if (!weaponPivot)
        {
            var p = transform.Find("weaponPivot");
            if (p) weaponPivot = p;
        }
        if (!weapon && weaponPivot)
        {
            var w = weaponPivot.Find("weapon");
            if (w) weapon = w;
        }
        if (!weaponAnimator && weapon)
        {
            weaponAnimator = weapon.GetComponent<Animator>();
        }
    }

    void Update()
    {
        cd -= Time.deltaTime;

        // tìm target trong tầm
        if (!target || Vector2.Distance(transform.position, target.position) > range)
            target = FindNearestEnemy();

        bool inRange = target != null;
        if (weaponAnimator) weaponAnimator.SetBool("HasTarget", inRange);
        if (!inRange || !weapon) return;

        // QUAY CHỈ THẰNG CON 'weapon' (localRotation quanh Z)
        Vector2 origin = weapon ? (Vector2)weapon.position
                                : (weaponPivot ? (Vector2)weaponPivot.position
                                               : (Vector2)transform.position);
        Vector2 aim = (Vector2)target.position - origin;
        float angDeg = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
        Quaternion goal = Quaternion.AngleAxis(angDeg + aimVisualOffsetDeg, Vector3.forward);

        weapon.localRotation = Quaternion.RotateTowards(
            weapon.localRotation, goal, turnSpeed * Time.deltaTime
        );

        // bắn
        if (cd <= 0f)
        {
            cd = 1f / Mathf.Max(0.01f, fireRate);
            Shoot();
        }
    }

    // ========= Targeting & Shooting =========

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

    GameObject GetProjectileForLevel()
    {
        if (projectileLevels != null && projectileLevels.Length > 0)
        {
            int i = Mathf.Clamp(level, 0, projectileLevels.Length - 1);
            if (projectileLevels[i] != null)
                return projectileLevels[i];
        }
        return projectile; // fallback
    }

    void Shoot()
    {
        if (!target || !muzzle || !weapon) return;

        var projPrefab = GetProjectileForLevel();
        if (!projPrefab) return;

        // Dùng rotation của muzzle để bảo đảm hướng nòng
        var go = Instantiate(projPrefab, muzzle.position, muzzle.rotation);

        // truyền damage nếu projectile hỗ trợ
        var homing = go.GetComponent<ArrowHoming>();
        if (homing)
        {
            homing.Init(target, enemyMask);
            homing.damage = Mathf.RoundToInt(damage);
        }

        if (weaponAnimator)
        {
            weaponAnimator.ResetTrigger("Shoot");
            weaponAnimator.SetTrigger("Shoot");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    // ========= Economy =========

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

        ApplyLevelVisuals();
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

    // ========= Level & Visuals =========

    public void SetLevel(int lv)
    {
        level = Mathf.Max(0, lv);
        ApplyLevelVisuals();

        // scale chỉ số ngay từ level khởi điểm (giữ thông số inspector là level 0)
        for (int i = 0; i < level; i++)
        {
            damage *= dmgPerLevelMul;
            fireRate *= firePerLevelMul;
            range += rangePerLevelAdd;
            upgradeCost = Mathf.RoundToInt(upgradeCost * upgradeCostMul);
        }

        onStatsChanged?.Invoke(this);
    }

    void ApplyLevelVisuals()
    {
        // sprite thân
        if (baseSR && baseLevels != null && baseLevels.Length > 0)
        {
            int i = Mathf.Clamp(level, 0, baseLevels.Length - 1);
            baseSR.sprite = baseLevels[i];
        }
        // sprite vũ khí (nếu dùng riêng) — chỉ set bằng code nếu Animator KHÔNG điều khiển sprite
        bool animatorControlsWeapon = weaponAnimator && Application.isPlaying && weaponControllers != null && weaponControllers.Length > 0;
        if (weaponSR && weaponLevels != null && weaponLevels.Length > 0 && !animatorControlsWeapon)
        {
            int i = Mathf.Clamp(level, 0, weaponLevels.Length - 1);
            weaponSR.sprite = weaponLevels[i];
        }
        // OFFSET weaponPivot theo level (weaponPivot không quay)
        if (weaponPivot && weaponOffsets != null && weaponOffsets.Length > 0)
        {
            int i = Mathf.Clamp(level, 0, weaponOffsets.Length - 1);
            weaponPivot.localPosition = weaponOffsets[i];
        }
        // đổi AnimatorController theo level (CHỈ khi đang PLAY để tránh lỗi Editor)
        if (Application.isPlaying && weaponAnimator && weaponControllers != null && weaponControllers.Length > 0)
        {
            int i = Mathf.Clamp(level, 0, weaponControllers.Length - 1);
            var ctrl = weaponControllers[i];
            if (ctrl && weaponAnimator.runtimeAnimatorController != ctrl)
            {
                weaponAnimator.runtimeAnimatorController = ctrl;
                weaponAnimator.Rebind();
                weaponAnimator.Update(0f);
            }
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            // Preview an toàn trong Editor (không động vào AnimatorController)
            int prev = level;
            level = startLevel;

            // sprite thân
            if (baseSR && baseLevels != null && baseLevels.Length > 0)
            {
                int i = Mathf.Clamp(level, 0, baseLevels.Length - 1);
                baseSR.sprite = baseLevels[i];
            }
            // sprite vũ khí (không dùng Animator trong Editor)
            if (weaponSR && weaponLevels != null && weaponLevels.Length > 0)
            {
                int i = Mathf.Clamp(level, 0, weaponLevels.Length - 1);
                weaponSR.sprite = weaponLevels[i];
            }
            // offset pivot
            if (weaponPivot && weaponOffsets != null && weaponOffsets.Length > 0)
            {
                int i = Mathf.Clamp(level, 0, weaponOffsets.Length - 1);
                weaponPivot.localPosition = weaponOffsets[i];
            }

            level = prev;
        }
    }
#endif
}
