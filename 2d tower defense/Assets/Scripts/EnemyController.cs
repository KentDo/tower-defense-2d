using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events; // >>> thêm

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;

    [Header("Path (assigned by spawner)")]
    public LevelManager path;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [Tooltip("Khoảng cách để coi như đã đến waypoint (linear).")]
    [SerializeField] private float waypointReachDistance = 0.1f;

    [Header("Rotation (optional)")]
    [SerializeField] private bool useRotation = false;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Facing / Visuals (Dir-int)")]
    [SerializeField] private Transform rotationRoot;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteToFlip;
    [SerializeField] private bool flipSpriteOnHorizontal = true;

    [Tooltip("Sprite side gốc nhìn sang phải? (đa số asset)")]
    [SerializeField] private bool sideSpriteFacesRight = true;

    [Tooltip("Ngưỡng bỏ qua thay đổi hướng nhỏ (linear).")]
    [SerializeField] private float deadZone = 0.03f;

    [Tooltip("Độ lệch biên (hysteresis) để tránh nhấp nháy giữa trục ngang/dọc (0–0.5).")]
    [Range(0f, 0.5f)]
    [SerializeField] private float axisHysteresis = 0.1f;

    // Dir mapping: 0=Down, 1=Side, 2=Up (Left dùng flipX)
    private const string DIR_PARAM = "Dir";

    [Header("Death / Reward (optional)")]
    [SerializeField] private int goldOnDeath = 0;
    [SerializeField] private GameObject deathVfx = null;
    [SerializeField] private float deathCleanupDelay = 0.35f;
    [SerializeField] private bool shrinkOnDeath = true;

    [Header("Reward hooks")] // >>> thêm
    [SerializeField] private UnityEvent<int> onGoldReward; // kéo-thả AddGold(int) ở Inspector
    private bool goldGranted = false;                      // chống thưởng trùng
    public static event Action<int> OnAnyEnemyGoldRewarded; // lắng nghe bằng code (tuỳ chọn)

    [Header("Death Animation Options (Event-based)")]
    [SerializeField] private bool waitDeathAnimationEvent = true;
    [SerializeField] private bool useAnimatorDeath = true;
    [SerializeField] private string deathTriggerName = "Die";
    [SerializeField] private string deathStateName = "Death";
    [SerializeField] private float deathAnimFallbackSeconds = 0.6f;

    public event Action<EnemyController> onDeath;

    // ===== Internals =====
    private Rigidbody2D rb;
    private int currentWaypointIndex;
    private bool reachedEnd;
    private bool hasDied;
    private Coroutine deathRoutine;

    private float reachSqr;
    private float deadZoneSqr;
    private int lastDir = 0;
    private bool lastFlipX = false;

    public int CurrentHP => health ? health.Current : 0;
    public int MaxHP => health ? health.Max : 0;

    void Awake()
    {
        if (!health) health = GetComponent<Health>();
        if (!animator) animator = GetComponentInChildren<Animator>(true);
        if (!spriteToFlip) spriteToFlip = GetComponentInChildren<SpriteRenderer>(true);
        if (!rotationRoot) rotationRoot = transform;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        reachSqr = waypointReachDistance * waypointReachDistance;
        deadZoneSqr = deadZone * deadZone;

        if (health) health.onDied += HandleDeath;
    }

    void OnDestroy()
    {
        if (health) health.onDied -= HandleDeath;
    }

    void Start()
    {
        if (path == null || path.PathCount == 0)
        {
            Debug.LogWarning("[EnemyController] No path assigned. Enemy will not move.");
            enabled = false;
            return;
        }

        var firstPoint = path.GetPathPoint(0);
        if (firstPoint != null)
        {
            rb.position = firstPoint.position;
            transform.position = firstPoint.position;
        }

        currentWaypointIndex = 1;
        lastFlipX = spriteToFlip ? spriteToFlip.flipX : false;
    }

    void Update()
    {
        if (hasDied || reachedEnd) return;
    }

    void FixedUpdate()
    {
        if (hasDied || reachedEnd || path == null) return;
        if (currentWaypointIndex >= path.PathCount)
        {
            ReachEnd();
            return;
        }

        Transform target = path.GetPathPoint(currentWaypointIndex);
        if (!target)
        {
            currentWaypointIndex++;
            return;
        }

        Vector2 to = (Vector2)target.position - rb.position;
        float distSqr = to.sqrMagnitude;

        float maxStep = moveSpeed * Time.fixedDeltaTime;

        if (distSqr <= Mathf.Max(reachSqr, maxStep * maxStep))
        {
            rb.MovePosition(target.position);
            currentWaypointIndex++;
            if (currentWaypointIndex >= path.PathCount)
            {
                ReachEnd();
                return;
            }

            Vector2 next = (Vector2)path.GetPathPoint(currentWaypointIndex).position - (Vector2)target.position;
            UpdateFacing(next);
            return;
        }

        Vector2 dir = to.normalized;
        rb.MovePosition(rb.position + dir * maxStep);
        UpdateFacing(dir);
    }

    void ReachEnd()
    {
        if (reachedEnd) return;
        reachedEnd = true;

        if (path != null) path.OnEnemyReachEnd(gameObject, 1);
        else Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        if (path == null || path.PathCount == 0) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < path.PathCount - 1; i++)
        {
            Transform current = path.GetPathPoint(i);
            Transform next = path.GetPathPoint(i + 1);
            if (current != null && next != null) Gizmos.DrawLine(current.position, next.position);
        }
    }

    void HandleDeath(Health _) => Kill();

    public void SetMaxHP(int newMax)
    {
        if (health) health.SetMax(newMax, refill: true);
    }

    public void TakeDamage(int amount)
    {
        if (!health || amount <= 0 || hasDied) return;
        health.TakeDamage(amount);
    }

    public void Heal(int amount)
    {
        if (health) health.Heal(amount);
    }

    // ===== Death sequence: TRIGGER → (EVENT or TIMER) → FADE/DESTROY =====
    public void Kill()
    {
        if (hasDied) return;
        hasDied = true;

        if (deathRoutine != null) StopCoroutine(deathRoutine);

        if (deathVfx)
        {
            var fx = Instantiate(deathVfx, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }

        DisableCollisions();

        // >>> TRAO VÀNG NGAY TẠI ĐÂY (một lần) <<<
        RewardOnDeath();

        onDeath?.Invoke(this);

        if (useAnimatorDeath && animator)
        {
            if (HasAnimatorParameter(deathTriggerName, AnimatorControllerParameterType.Trigger))
            {
                animator.ResetTrigger(deathTriggerName);
                animator.SetTrigger(deathTriggerName);
            }
            else
            {
                animator.CrossFadeInFixedTime(deathStateName, 0.05f, 0, 0f);
            }
        }

        if (waitDeathAnimationEvent) return;

        float animDuration = useAnimatorDeath ? FindClipLengthByName(deathStateName, deathAnimFallbackSeconds) : 0f;
        if (shrinkOnDeath)
            deathRoutine = StartCoroutine(DeathSequence(animDuration));
        else
            Destroy(gameObject, Mathf.Max(0.01f, animDuration));
    }

    // HÀM NÀY được Animation Event gọi ở cuối clip Death (đặt trên GameObject có Animator)
    public void OnDeathAnimEnd()
    {
        if (!hasDied) return;

        if (shrinkOnDeath)
        {
            if (deathRoutine == null) deathRoutine = StartCoroutine(DeathSequence(0f));
        }
        else
        {
            Destroy(gameObject, Mathf.Max(0.01f, deathCleanupDelay));
        }
    }

    // >>> Thưởng vàng (Inspector + event code)
    void RewardOnDeath()
    {
        if (goldGranted) return;
        goldGranted = true;

        if (goldOnDeath > 0)
        {
            // >>> Đẩy vàng vào BuildManager để HUDCoins nhận onCoinsChanged và cập nhật UI
            var bm = BuildManager.I;
            if (bm != null)
            {
                bm.Add(goldOnDeath);
            }
            else
            {
                Debug.LogWarning("[EnemyController] BuildManager.I is null -> HUD sẽ không cập nhật!");
            }

            // Giữ lại hooks nếu bạn vẫn muốn (không bắt buộc)
            // onGoldReward?.Invoke(goldOnDeath);
            // OnAnyEnemyGoldRewarded?.Invoke(goldOnDeath);
        }
    }


    // ===== Facing ổn định (Dir + flipX + hysteresis) =====
    void UpdateFacing(Vector2 dir)
    {
        if (dir.sqrMagnitude < deadZoneSqr) return;

        float ax = Mathf.Abs(dir.x);
        float ay = Mathf.Abs(dir.y);
        float bias = axisHysteresis * Mathf.Max(ax, ay);

        int newDir;
        if (lastDir == 1)
        {
            newDir = (ay > ax + bias) ? ((dir.y > 0f) ? 2 : 0) : 1;
        }
        else
        {
            bool toSide = ax > ay + bias;
            if (toSide) newDir = 1;
            else newDir = (lastDir == 2 || dir.y > 0f) ? 2 : 0;
        }

        if (newDir != lastDir)
        {
            lastDir = newDir;
            if (animator && HasAnimatorParameter(DIR_PARAM, AnimatorControllerParameterType.Int))
                animator.SetInteger(DIR_PARAM, lastDir);
        }

        if (lastDir == 1 && spriteToFlip && flipSpriteOnHorizontal)
        {
            bool movingLeft = dir.x < 0f;
            bool wantFlip = sideSpriteFacesRight ? movingLeft : !movingLeft;
            if (wantFlip != lastFlipX)
            {
                spriteToFlip.flipX = wantFlip;
                lastFlipX = wantFlip;
            }
        }

        if (useRotation && rotationRoot && rotationSpeed > 0f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, angle);
            rotationRoot.rotation = Quaternion.RotateTowards(rotationRoot.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    // ===== Helpers =====
    float FindClipLengthByName(string stateOrClipName, float fallback)
    {
        if (!animator || animator.runtimeAnimatorController == null)
            return Mathf.Max(0.05f, fallback);

        var clips = animator.runtimeAnimatorController.animationClips;
        if (clips != null)
        {
            for (int i = 0; i < clips.Length; i++)
            {
                var clip = clips[i];
                if (clip && string.Equals(clip.name, stateOrClipName, StringComparison.OrdinalIgnoreCase))
                    return Mathf.Max(0.05f, clip.length);
            }
        }
        return Mathf.Max(0.05f, fallback);
    }

    bool HasAnimatorParameter(string name, AnimatorControllerParameterType type)
    {
        if (!animator) return false;
        foreach (var p in animator.parameters)
            if (p.type == type && p.name == name) return true;
        return false;
    }

    void DisableCollisions()
    {
        foreach (var col in GetComponentsInChildren<Collider2D>()) col.enabled = false;

        foreach (var body in GetComponentsInChildren<Rigidbody2D>())
        {
            if (!body) continue;
            body.simulated = false;
#if UNITY_2022_2_OR_NEWER
            body.linearVelocity = Vector2.zero;
#else
            body.velocity = Vector2.zero;
#endif
            body.angularVelocity = 0f;
        }
    }

    IEnumerator DeathSequence(float waitAnimSeconds)
    {
        if (waitAnimSeconds > 0.01f)
            yield return new WaitForSeconds(waitAnimSeconds);

        var renderers = GetComponentsInChildren<SpriteRenderer>();
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, deathCleanupDelay);
        Vector3 initialScale = rotationRoot ? rotationRoot.localScale : transform.localScale;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float alpha = Mathf.Clamp01(1f - t);

            foreach (var sr in renderers)
            {
                if (!sr) continue;
                var c = sr.color; c.a = alpha; sr.color = c;
            }

            if (rotationRoot) rotationRoot.localScale = initialScale * alpha;
            else transform.localScale = initialScale * alpha;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
