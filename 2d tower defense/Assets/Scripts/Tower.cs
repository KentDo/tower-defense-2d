using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Weapon")]
    public Transform muzzle;
    public GameObject projectile;
    public Animator weaponAnimator;
    public float fireRate = 1.2f;
    public float range = 4f;
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

    [Range(0, 2)] public int startLevel = 0;
    int level;
    public int Level => level;

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

        // xoay nòng (WeaponPivot) thay vì xoay root
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
        if (!target) return;
        // Spawn đạn theo hướng WeaponPivot
        var go = Instantiate(projectile, muzzle.position, weaponPivot.rotation);

        var homing = go.GetComponent<ArrowHoming>();
        if (homing) homing.Init(target, enemyMask);

        if (weaponAnimator) weaponAnimator.SetTrigger("Shoot");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    public void SetLevel(int lv)
    {
        if (baseLevels == null || baseLevels.Length == 0 ||
            weaponLevels == null || weaponLevels.Length == 0)
        {
            Debug.LogWarning("[Tower] Chưa gán đủ sprite levels!");
            return;
        }

        level = Mathf.Clamp(lv, 0, Mathf.Min(baseLevels.Length, weaponLevels.Length) - 1);
        if (baseSR) baseSR.sprite = baseLevels[level];
        if (weaponSR) weaponSR.sprite = weaponLevels[level];
        if (weaponPivot && weaponOffsets != null && level < weaponOffsets.Length)
            weaponPivot.localPosition = weaponOffsets[level];
    }

    public void Upgrade() => SetLevel(level + 1);

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying && baseSR && weaponSR && baseLevels.Length > 0 && weaponLevels.Length > 0)
            SetLevel(startLevel);
    }
#endif
}
