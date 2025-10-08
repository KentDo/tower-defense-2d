using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public LevelManager path;
    public float speed = 2.5f;
    public float reach = 0.05f;

    int idx;
    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    [Header("Health")]
    [SerializeField] int maxHP = 10;
    public int CurrentHP { get; private set; }
    public int MaxHP => maxHP;

    public Action<EnemyController> onDeath;
    public Action<int, int> onHealthChanged;

    [Header("Facing")]
    public float deadZone = 0.001f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        CurrentHP = maxHP;
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

        var dir0 = (Vector2)(path.GetPathPoint(1).position - path.GetPathPoint(0).position);
        SetFacing(dir0);
    }

    void FixedUpdate()
    {
        if (idx >= path.PathCount) return;
        Vector2 target = path.GetPathPoint(idx).position;
        Vector2 dir = (target - (Vector2)transform.position).normalized;

        SetFacing(dir);
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);

        if (Vector2.Distance(transform.position, target) <= reach)
        {
            idx++;
            if (idx >= path.PathCount)
            {
                path.OnEnemyReachEnd(gameObject, 1);
                return;
            }
            Vector2 nextDir = (Vector2)(path.GetPathPoint(idx).position - path.GetPathPoint(idx - 1).position);
            SetFacing(nextDir);
        }
    }

    void SetFacing(Vector2 dir)
    {
        if (dir.sqrMagnitude < deadZone) return;
        int d;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            d = 1;
            sr.flipX = dir.x < 0;
        }
        else
        {
            d = (dir.y > 0) ? 2 : 0;
        }
        anim.SetInteger("Dir", d);
    }

    // ========= Health =========
    public void TakeDamage(int dmg)
    {
        if (dmg <= 0 || CurrentHP <= 0) return;
        CurrentHP = Mathf.Max(0, CurrentHP - dmg);
        onHealthChanged?.Invoke(CurrentHP, maxHP);
        if (CurrentHP == 0) Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || CurrentHP <= 0) return;
        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
        onHealthChanged?.Invoke(CurrentHP, maxHP);
    }

    void Die()
    {
        onDeath?.Invoke(this);
        Destroy(gameObject);
    }
}
