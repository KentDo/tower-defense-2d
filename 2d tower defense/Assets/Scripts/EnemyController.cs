using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public LevelManager path;      // Waypoints
    public float speed = 2.5f;
    public float reach = 0.05f;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    int idx;           // waypoint index
    int lastDir = 1;   // 0=Down, 1=Side, 2=Up (để death chọn đúng clip)
    bool isDead;

    [Header("Health")]
    [SerializeField] int maxHP = 10;                 // HP gốc của prefab
    public int CurrentHP { get; private set; }
    public int MaxHP => maxHP;
    public Action<int, int> onHealthChanged;          // (optional) UI nghe sự kiện

    [Header("Facing (3 hướng)")]
    public float deadZone = 0.001f;
    [Range(0f, 1f)] public float axisBias = 0.15f;   // chống giật khi rẽ
    public bool invertSideFlip = false;              // nếu sprite Side mặc định nhìn trái, bật cái này

    [Header("Death")]
    public bool useAnimationEvent = true;            // nếu chưa gắn event → đặt false để dùng timer
    public float deathDuration = 0.6f;               // fallback thời gian clip Death

    public Action<EnemyController> onDeath;          // callback khi destroy xong

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        CurrentHP = maxHP; // khởi tạo HP
    }

    void Start()
    {
        if (!path || path.PathCount < 2)
        {
            enabled = false;
            return;
        }

        transform.position = path.GetPathPoint(0).position;
        idx = 1;

        Vector2 d0 = (Vector2)(path.GetPathPoint(1).position - path.GetPathPoint(0).position);
        SetFacing(d0);
    }

    void FixedUpdate()
    {
        if (isDead) return;
        if (idx >= path.PathCount) return;

        Vector2 target = path.GetPathPoint(idx).position;
        Vector2 toTarget = target - (Vector2)transform.position;
        Vector2 dir = toTarget.sqrMagnitude > 0.000001f ? toTarget.normalized : Vector2.zero;

        SetFacing(dir);
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);

        if (toTarget.magnitude <= reach)
        {
            idx++;
            if (idx >= path.PathCount)
            {
                // ======= TỚI CUỐI ĐƯỜNG =======
                // 1. Trừ máu base:
                var lm = LevelManager.Instance ?? FindObjectOfType<LevelManager>();
                if (lm) lm.OnEnemyReachEnd(gameObject, 1); // 1 là damage vào base

                // 2. (Optional) Báo cho Spawner nếu có relay:
                var relay = GetComponent<EnemyLifecycleRelay>();
                if (relay && !relay._counted)
                {
                    var spawner = FindObjectOfType<EnemySpawner>();
                    if (spawner) spawner.NotifyEnemyRemoved(relay);
                    relay._counted = true;
                }

                // 3. Destroy enemy:
                Destroy(gameObject);
                return;
            }

            Vector2 nextDir = (Vector2)(path.GetPathPoint(idx).position - path.GetPathPoint(idx - 1).position);
            SetFacing(nextDir);
        }
    }

    // ===== Facing: 0=Down, 1=Side, 2=Up; chỉ lật khi Side =====
    void SetFacing(Vector2 dir)
    {
        if (dir.sqrMagnitude < deadZone) return;

        float ax = Mathf.Abs(dir.x);
        float ay = Mathf.Abs(dir.y);

        int d;
        if (lastDir == 1) d = (ax >= ay + axisBias) ? 1 : (dir.y > 0f ? 2 : 0);
        else d = (ay >= ax + axisBias) ? (dir.y > 0f ? 2 : 0) : 1;

        if (d == 1)
        {
            bool flip = dir.x < 0f;
            if (invertSideFlip) flip = !flip;
            sr.flipX = flip;
        }
        else sr.flipX = false;

        lastDir = d;
        anim.SetInteger("Dir", d);
    }

    // ======= HEALTH API =======
    public void SetMaxHP(int newMax)
    {
        maxHP = Mathf.Max(1, newMax);
        CurrentHP = maxHP;
        onHealthChanged?.Invoke(CurrentHP, maxHP);
    }

    public void TakeDamage(int dmg)
    {
        if (isDead || dmg <= 0) return;
        CurrentHP = Mathf.Max(0, CurrentHP - dmg);
        onHealthChanged?.Invoke(CurrentHP, maxHP);
        if (CurrentHP == 0) Kill();
    }

    // ======= DEATH =======
    public void Kill()
    {
        if (isDead) return;
        isDead = true;

        rb.linearVelocity = Vector2.zero;

        anim.SetInteger("Dir", lastDir); // chốt hướng để vào Death_* đúng
        anim.SetTrigger("Die");

        if (!useAnimationEvent)
            Invoke(nameof(FinishDeath), Mathf.Max(0.05f, deathDuration));
    }

    // Gọi từ Animation Event cuối clip Death_D/S/U
    public void OnDeathAnimEnd()
    {
        if (useAnimationEvent) FinishDeath();
    }

    void FinishDeath()
    {
        onDeath?.Invoke(this);
        Destroy(gameObject);
    }
}
