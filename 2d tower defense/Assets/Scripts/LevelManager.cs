using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections; // thêm để dùng coroutine

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Game State")]
    public int lives = 20;

    // === HUD subscribe vào đây ===
    public event Action<int> onLivesChanged;

    [Header("Path Settings")]
    public Transform[] waypoints;

    // ==== trạng thái game ====
    bool isGameOverTriggered = false;
    bool isInvulnerable = false; // 🛡 base miễn sát thương tạm thời

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        onLivesChanged?.Invoke(lives);
    }

    public int PathCount => waypoints?.Length ?? 0;
    public Transform GetPathPoint(int i) => (i >= 0 && i < PathCount) ? waypoints[i] : null;

    // ========== BASE DAMAGE ==========
    public void SetLives(int value)
    {
        if (isGameOverTriggered) return;

        lives = Mathf.Max(0, value);
        onLivesChanged?.Invoke(lives);

        if (lives <= 0)
        {
            isGameOverTriggered = true;
            QuizManager quiz = FindObjectOfType<QuizManager>();
            if (quiz != null)
                quiz.ShowQuiz();
            else
                Debug.LogError("Không tìm thấy QuizManager trong scene!");
        }
    }

    public void DamageBase(int amount)
    {
        // 🛡 nếu đang miễn sát thương thì bỏ qua
        if (isInvulnerable) return;
        SetLives(lives - Mathf.Abs(amount));
    }

    // ========== ENDPOINT ==========
    public void OnEnemyReachEnd(GameObject enemy, int damage = 1)
    {
        DamageBase(damage);
        if (enemy) Destroy(enemy);
    }

    // ========== HỒI SINH ==========
    public void RevivePlayer(int extraLives = 1)
    {
        isGameOverTriggered = false;
        SetLives(lives + extraLives);
    }

    // 🛡 Bật bất tử trong thời gian tạm
    public void ActivateInvulnerability(float duration)
    {
        if (!isInvulnerable)
            StartCoroutine(InvulnerabilityCoroutine(duration));
    }

    private IEnumerator InvulnerabilityCoroutine(float duration)
    {
        isInvulnerable = true;
        Debug.Log($"🛡 Base bất tử trong {duration} giây...");
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
        Debug.Log("⛔ Hết thời gian bất tử.");
    }
}
