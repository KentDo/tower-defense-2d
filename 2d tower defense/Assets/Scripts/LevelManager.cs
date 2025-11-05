using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Game State")]
    public int lives = 20;

    public event Action<int> onLivesChanged;

    [Header("Path Settings")]
    public Transform[] waypoints;

    private bool isGameOverTriggered;
    private bool isInvulnerable;

    void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Start()
    {
        GameSession.SetLastGameplayScene(SceneManager.GetActiveScene().name);
        onLivesChanged?.Invoke(lives);
    }

    public int PathCount => waypoints?.Length ?? 0;

    public Transform GetPathPoint(int index)
    {
        return (index >= 0 && index < PathCount) ? waypoints[index] : null;
    }

    public void SetLives(int value)
    {
        if (isGameOverTriggered) return;

        lives = Mathf.Max(0, value);
        onLivesChanged?.Invoke(lives);

        if (lives <= 0)
        {
            isGameOverTriggered = true;
            var quiz = FindObjectOfType<QuizManager>();
            if (quiz != null)
            {
                quiz.ShowQuiz();
            }
            else
            {
                Debug.LogError("[LevelManager] QuizManager not found in scene.");
            }
        }
    }

    public void DamageBase(int amount)
    {
        if (isInvulnerable) return;
        SetLives(lives - Mathf.Abs(amount));
    }

    public void OnEnemyReachEnd(GameObject enemy, int damage = 1)
    {
        DamageBase(damage);
        if (enemy) Destroy(enemy);
    }

    public void RevivePlayer(int extraLives = 1)
    {
        isGameOverTriggered = false;
        SetLives(lives + Mathf.Max(0, extraLives));
    }

    public void ActivateInvulnerability(float duration)
    {
        if (!isInvulnerable)
        {
            StartCoroutine(InvulnerabilityCoroutine(duration));
        }
    }

    private IEnumerator InvulnerabilityCoroutine(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }
}
