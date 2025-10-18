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

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private bool useRotation = false;  // tắt xoay để dùng animation
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float waypointReachDistance = 0.1f;

    [Header("Death / Reward (optional)")]
    [SerializeField] private int goldOnDeath = 0; // tuỳ dự án dùng hay không
    [SerializeField] private GameObject deathVfx = null;

    // Sự kiện cho logic khác nếu muốn nghe enemy chết (tuỳ chọn)
    public event Action<EnemyController> onDeath;

    // ===== Movement state =====
    private int currentWaypointIndex = 0;
    private bool reachedEnd = false;
    private Animator animator;
    private Vector2 lastDirection;

    // ===== API công khai (tương thích) =====
    public int CurrentHP => health ? health.Current : 0;
    public int MaxHP => health ? health.Max : 0;

    void Awake()
    {
        if (!health) health = GetComponent<Health>();
        animator = GetComponent<Animator>(); // lấy animator nếu có
    }

    void Start()
    {
        // Kiểm tra path và đặt vị trí ban đầu
        if (path == null || path.PathCount == 0)
        {
            Debug.LogWarning("[EnemyController] Không có path! Enemy sẽ không di chuyển.");
            enabled = false;
            return;
        }

        // Đặt vị trí ban đầu tại waypoint đầu tiên
        if (path.GetPathPoint(0) != null)
        {
            transform.position = path.GetPathPoint(0).position;
        }
    }

    void Update()
    {
        if (!reachedEnd && path != null && path.PathCount > 0)
        {
            MoveAlongPath();
        }
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

    // ===== MOVEMENT =====
    void MoveAlongPath()
    {
        // Kiểm tra xem còn waypoint không
        if (currentWaypointIndex >= path.PathCount)
        {
            ReachEnd();
            return;
        }

        Transform targetWaypoint = path.GetPathPoint(currentWaypointIndex);
        if (targetWaypoint == null)
        {
            currentWaypointIndex++;
            return;
        }

        // Di chuyển về phía waypoint hiện tại
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Cập nhật animator nếu có (cho animation theo hướng)
        if (animator != null && direction != Vector3.zero)
        {
            // Sử dụng parameters: MoveX, MoveY cho Blend Tree
            animator.SetFloat("MoveX", direction.x);
            animator.SetFloat("MoveY", direction.y);
            animator.SetBool("IsMoving", true);
            lastDirection = direction;
        }

        // Xoay enemy theo hướng di chuyển (chỉ khi bật useRotation)
        if (useRotation && direction != Vector3.zero && rotationSpeed > 0f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Kiểm tra xem đã đến waypoint chưa
        float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint.position);
        if (distanceToWaypoint <= waypointReachDistance)
        {
            currentWaypointIndex++;
        }
    }

    void ReachEnd()
    {
        if (reachedEnd) return;
        reachedEnd = true;

        // Gây sát thương cho base và tự hủy
        if (path != null)
        {
            path.OnEnemyReachEnd(gameObject, 1);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Vẽ đường đi trong Scene view để debug
    void OnDrawGizmos()
    {
        if (path == null || path.PathCount == 0) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < path.PathCount - 1; i++)
        {
            Transform current = path.GetPathPoint(i);
            Transform next = path.GetPathPoint(i + 1);
            if (current != null && next != null)
            {
                Gizmos.DrawLine(current.position, next.position);
            }
        }
    }
}
