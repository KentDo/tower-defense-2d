using UnityEngine;
using UnityEngine.SceneManagement;
using System;   // cần cho Action<>

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Game State")]
    public int lives = 20;

    // HUD subscribe vào đây
    public event Action<int> onLivesChanged;

    [Header("Path Settings")]
    public Transform[] waypoints;

    [Header("Scene Config")]
    [Tooltip("Tên scene Lobby để bỏ qua khi auto-save last level")]
    public string lobbySceneName = "Lobby";

    [Tooltip("Tự lưu tên scene hiện tại khi scene được load (trừ Lobby)")]
    public bool autoSaveLastLevel = true;

    void Awake()
{
    if (Instance && Instance != this)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
    // ❌ XÓA dòng dưới vì không cần giữ qua scene
    // DontDestroyOnLoad(gameObject);
}


    void OnEnable()
    {
        // Theo dõi khi scene được load để auto-save
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // Bắn event lần đầu để HUD hiển thị đúng ngay khi vào game
        onLivesChanged?.Invoke(lives);

        // Trường hợp LevelManager được đặt trực tiếp trong map (không đi qua Lobby),
        // vẫn lưu tên scene hiện tại (nếu không phải Lobby)
        TryAutoSaveCurrentScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Mỗi lần đổi scene, bắn lại HUD và (tuỳ chọn) auto-save
        onLivesChanged?.Invoke(lives);
        TryAutoSaveCurrentScene();
    }

    private void TryAutoSaveCurrentScene()
    {
        if (!autoSaveLastLevel) return;

        string sceneName = SceneManager.GetActiveScene().name;
        if (!string.IsNullOrEmpty(sceneName) && sceneName != lobbySceneName)
        {
            SaveSystem.SetLastLevel(sceneName);
            // Debug.Log($"✅ Saved last level: {sceneName}");
        }
    }

    // ========== PATH ==========
    public int PathCount => waypoints?.Length ?? 0;
    public Transform GetPathPoint(int i) => (i >= 0 && i < PathCount) ? waypoints[i] : null;

    // ========== BASE DAMAGE ==========
    public void SetLives(int value)
    {
        lives = Mathf.Max(0, value);
        onLivesChanged?.Invoke(lives);
        if (lives <= 0)
        {
            Debug.Log("Game Over");
            // TODO: show UI / reload scene
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
