using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Game State")]
    public int lives = 20;

    [Header("Path Settings")]
    public Transform[] waypoints;   // drag các waypoint vào đây

    void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ========== PATH ==========
    public int PathCount => waypoints?.Length ?? 0;
    public Transform GetPathPoint(int i) =>
        (i >= 0 && i < PathCount) ? waypoints[i] : null;

    // ========== BASE DAMAGE ==========
    public void DamageBase(int amount)
    {
        lives -= amount;
        Debug.Log($"Base hit! Lives = {lives}");
        if (lives <= 0)
        {
            Debug.Log("Game Over");
            // TODO: show UI / reload scene
        }
    }

    // ========== ENDPOINT ==========
    public void OnEnemyReachEnd(GameObject enemy, int damage = 1)
    {
        DamageBase(damage);
        Destroy(enemy);
    }
}
