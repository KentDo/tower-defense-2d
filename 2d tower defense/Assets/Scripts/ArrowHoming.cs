using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class ArrowHoming : MonoBehaviour
{
    [Header("Flight")]
    public float speed = 8f;
    public float turnRateDeg = 720f;   // độ/giây khi rẽ bám mục tiêu
    public float lifeTime = 5f;

    [Header("Hit")]
    public int damage = 50;  // 2 phát đạn = 100 damage để giết quái có 100 HP
    public LayerMask enemyMask;

    Rigidbody2D rb;
    Transform target;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void OnEnable()
    {
        // bay thẳng theo "up" ban đầu tới khi bám target
        rb.linearVelocity = transform.up * speed;
        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        if (target == null) return;

        Vector2 toTarget = (Vector2)(target.position - transform.position);
        if (toTarget.sqrMagnitude < 0.0001f) return;

        // hướng hiện tại của đạn (dựa theo velocity)
        Vector2 fwd = rb.linearVelocity.sqrMagnitude > 0.0001f ? rb.linearVelocity.normalized : (Vector2)transform.up;

        // quay dần về phía mục tiêu với tốc độ turnRateDeg
        float currentAng = Mathf.Atan2(fwd.y, fwd.x) * Mathf.Rad2Deg;
        float targetAng = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
        float newAng = Mathf.MoveTowardsAngle(currentAng, targetAng, turnRateDeg * Time.fixedDeltaTime);

        // cập nhật velocity & xoay sprite (sprite hướng "up")
        Vector2 newDir = new Vector2(Mathf.Cos(newAng * Mathf.Deg2Rad), Mathf.Sin(newAng * Mathf.Deg2Rad));
        rb.linearVelocity = newDir * speed;
        transform.rotation = Quaternion.Euler(0f, 0f, newAng - 90f);
    }

    /// <summary>Gọi sau khi spawn.</summary>
    public void Init(Transform t, LayerMask mask)
    {
        target = t;
        enemyMask = mask;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Lọc theo LayerMask (tránh ăn va chạm linh tinh)
        if ((enemyMask.value & (1 << other.gameObject.layer)) == 0) return;

        // Lấy EnemyController ở object hoặc cha
        var enemy = other.GetComponent<EnemyController>();
        if (!enemy) enemy = other.GetComponentInParent<EnemyController>();
        if (!enemy) return;

        enemy.TakeDamage(damage);
        Destroy(gameObject);
    }
}
