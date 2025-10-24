using UnityEngine;
using System;   // <-- cần cho Action<>
using UnityEngine.SceneManagement;


public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Game State")]
    public int lives = 20;

    // === HUD subscribe vào đây ===
    public event Action<int> onLivesChanged;

    [Header("Path Settings")]
    public Transform[] waypoints;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // bắn event lần đầu để HUD hiển thị đúng ngay khi vào game
        onLivesChanged?.Invoke(lives);
    }

    // ========== PATH ==========
    public int PathCount => waypoints?.Length ?? 0;
    public Transform GetPathPoint(int i) => (i >= 0 && i < PathCount) ? waypoints[i] : null;

    // ========== BASE DAMAGE ==========
    bool isGameOverTriggered = false;

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
        SetLives(lives - Mathf.Abs(amount));
    }

    // ========== ENDPOINT ==========
    public void OnEnemyReachEnd(GameObject enemy, int damage = 1)
    {
        DamageBase(damage);
        if (enemy) Destroy(enemy);
    }
}
