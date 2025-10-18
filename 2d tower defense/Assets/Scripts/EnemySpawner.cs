using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
<<<<<<< Updated upstream
    [Header("Prefab & Path")]
=======
    [Header("Path / Spawn point")]
    public LevelManager levelManager;
    [Tooltip("Để trống = spawn tại waypoint[0]")]
    public Transform spawnPointOverride;

    [Header("Waves (đa round)")]
    public bool autoStart = true;
    public Wave[] waves;

    [Header("Wave Data (CSV)")]
    public string csvRelativePath; // VD: "Waves/waves_map02.csv"
    public GameObject[] enemyPrefabs; // Kéo tất cả prefab enemy vào đây

    [Header("Legacy (1 round đơn) — dùng khi Waves rỗng")]
>>>>>>> Stashed changes
    public GameObject enemyPrefab;
    public LevelManager levelManager;
    public bool spawnAtFirstWaypoint = true;

    [Header("Wave Settings")]
    public int count = 10;
    public float startDelay = 0f;
    public float interval = 1.2f;

    [Header("Loop Settings")]
    public bool loop = false;
    public float loopDelay = 4f;

<<<<<<< Updated upstream
<<<<<<< Updated upstream
    [Header("Auto Start")]
    public bool playOnStart = true;
=======
=======
>>>>>>> Stashed changes
    [Header("Reward")]
    public int waveCompletionReward = 20;

    [Header("Debug")]
    public int currentWaveIndex = -1;  // -1 = idle
    public int alive = 0;

    // ====== State nội bộ ======
    bool spawningWave;
    bool waveFinishedSpawning;

    // ====== Public API / Props / Events ======
    public bool IsRunning => (currentWaveIndex != -1) || spawningWave || alive > 0;
    public int TotalWaves => (waves != null) ? waves.Length : 0;
    public bool CanStartNextWave => waveFinishedSpawning && alive <= 0 && currentWaveIndex >= 0 && currentWaveIndex < TotalWaves - 1;

    /// <summary>Bắn khi vào wave mới: (currentWave, total) — currentWave là 1-based.</summary>
    public System.Action<int, int> onWaveChanged;
    /// <summary>Bắn khi kết thúc tất cả waves.</summary>
    public System.Action onAllWavesFinished;

    // Cờ cho phép UI bỏ qua delay giữa waves khi người chơi bấm Next
    bool _skipInterWaveDelay = false;
>>>>>>> Stashed changes

    void Start()
    {
        if (!levelManager) levelManager = LevelManager.Instance;
<<<<<<< Updated upstream
        if (playOnStart) StartWave();
=======

        // Đọc dữ liệu từ CSV nếu có đường dẫn
        if (!string.IsNullOrEmpty(csvRelativePath))
        {
            LoadWavesFromCSV(csvRelativePath);
        }

        // Đọc dữ liệu từ CSV nếu có đường dẫn
        if (!string.IsNullOrEmpty(csvRelativePath))
        {
            LoadWavesFromCSV(csvRelativePath);
        }

        if (autoStart)
        {
            if (HasWavesConfigured()) StartCoroutine(RunWaves(0));
            else StartCoroutine(RunLegacy());
        }
>>>>>>> Stashed changes
    }

    public void StartWave(int? overrideCount = null)
    {
        StopAllCoroutines();
        StartCoroutine(SpawnWave(overrideCount ?? count));
    }

    IEnumerator SpawnWave(int c)
    {
<<<<<<< Updated upstream
        if (!enemyPrefab)
        {
            Debug.LogWarning("[Spawner] Chưa gán enemyPrefab");
            yield break;
=======
        if (!CanStartNextWave) return false;
        _skipInterWaveDelay = true;
        return true;
    }

    // ========= Core: multi-wave =========
    bool HasWavesConfigured()
    {
        if (waves == null || waves.Length == 0) return false;
        foreach (var w in waves)
        {
            if (w != null && w.entries != null)
            {
                foreach (var e in w.entries)
                    if (e != null && e.prefab && e.count > 0)
                        return true;
            }
        }
        return false;
    }

    IEnumerator RunWaves(int startIndex = 0)
    {
        if (!ValidatePath()) yield break;

        int total = TotalWaves;
        for (int w = Mathf.Max(0, startIndex); w < total; w++)
        {
            var wave = waves[w];
            if (wave == null) continue;

            currentWaveIndex = w;
            spawningWave = true;
            waveFinishedSpawning = false;

            onWaveChanged?.Invoke(w + 1, total);

            // Spawn entries
            if (wave.entries != null)
            {
                foreach (var e in wave.entries)
                {
                    if (e == null || !e.prefab || e.count <= 0) continue;

                    if (e.startDelay > 0f) yield return new WaitForSeconds(e.startDelay);

                    for (int i = 0; i < e.count; i++)
                    {
                        SpawnOne(e.prefab);
                        if (i < e.count - 1 && e.interval > 0f)
                            yield return new WaitForSeconds(e.interval);
                    }
                }
            }

            spawningWave = false;
            waveFinishedSpawning = true;

            // Chờ clear (alive == 0)
            yield return new WaitUntil(() => alive <= 0);

            // === CỘNG THƯỞNG SAU KHI CLEAR WAVE ===
            if (waveCompletionReward > 0)
            {
                // PlayerMoney.Instance.AddMoney(waveCompletionReward);
                Debug.Log($"[Spawner] Đã thưởng {waveCompletionReward} coins cho người chơi sau wave {currentWaveIndex + 1}!");
            }

            // Delay giữa waves — có thể bị skip nếu người chơi bấm Next
            if (!_skipInterWaveDelay && wave.nextWaveDelay > 0f)
                yield return new WaitForSeconds(wave.nextWaveDelay);

            _skipInterWaveDelay = false; // reset cho vòng sau
>>>>>>> Stashed changes
        }

        if (!levelManager || levelManager.PathCount < 2)
        {
<<<<<<< Updated upstream
            Debug.LogWarning("[Spawner] Không có LevelManager hoặc path < 2");
            yield break;
=======
            if (!enemyPrefab)
            {
                Debug.LogWarning("[Spawner] Legacy: Chưa gán enemyPrefab");
                yield break;
            }

            Vector3 spawnPos = spawnAtFirstWaypoint
                ? levelManager.GetPathPoint(0).position
                : (spawnPointOverride ? spawnPointOverride.position : transform.position);

            if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

            for (int i = 0; i < Mathf.Max(0, count); i++)
            {
                SpawnOne(enemyPrefab, spawnPos);
                if (i < count - 1 && interval > 0f)
                    yield return new WaitForSeconds(interval);
            }

            if (loop && loopDelay > 0f)
                yield return new WaitForSeconds(loopDelay);
        }
        while (loop);
    }

    // ========= Spawn helpers =========
    void SpawnOne(GameObject prefab) => SpawnOne(prefab, GetSpawnPos());

    void SpawnOne(GameObject prefab, Vector3 where)
    {
        var go = Instantiate(prefab, where, Quaternion.identity);

        var ec = go.GetComponent<EnemyController>();
        if (ec)
        {
            ec.path = levelManager;
            if (currentWaveIndex >= 0 && currentWaveIndex < TotalWaves)
            {
                float mul = Mathf.Max(0.01f, waves[currentWaveIndex].hpMultiplier);
                int scaled = Mathf.RoundToInt(ec.MaxHP * mul);
                ec.SetMaxHP(Mathf.Max(1, scaled));
            }
>>>>>>> Stashed changes
        }

        Vector3 spawnPos = spawnAtFirstWaypoint
            ? levelManager.GetPathPoint(0).position
            : transform.position;

        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        for (int i = 0; i < c; i++)
        {
            var go = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            var enemy = go.GetComponent<EnemyController>();
            if (enemy != null)
                enemy.path = levelManager;

            yield return new WaitForSeconds(interval);
        }

        if (loop)
        {
            yield return new WaitForSeconds(loopDelay);
            StartWave(c);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.2f);

        if (levelManager && levelManager.GetPathPoint(0))
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, levelManager.GetPathPoint(0).position);
        }
    }
#endif
<<<<<<< Updated upstream
=======

    // ===== Relay callback =====
    public void NotifyEnemyRemoved(EnemyLifecycleRelay relay)
    {
        if (relay == null || relay._counted) return;
        relay._counted = true;
        alive = Mathf.Max(0, alive - 1);
    }

    // =================== ĐỌC CSV ===================
    public void LoadWavesFromCSV(string relativePath)
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
        if (!File.Exists(fullPath))
        {
            Debug.LogError("[Spawner] Không tìm thấy file CSV: " + fullPath);
            return;
        }

        var lines = File.ReadAllLines(fullPath);
        var loadedWaves = new List<Wave>();
        for (int i = 1; i < lines.Length; i++) // bỏ qua header
        {
            var parts = lines[i].Split(',');
            if (parts.Length < 5) continue;
            Wave wave = new Wave();
            wave.name = $"Wave {parts[0]}";
            wave.entries = new SpawnEntry[]
            {
                new SpawnEntry
                {
                    prefab = FindEnemyPrefabByName(parts[1]),
                    count = int.Parse(parts[2]),
                    interval = float.Parse(parts[3])
                }
            };
            wave.hpMultiplier = float.Parse(parts[4]);
            loadedWaves.Add(wave);
        }
        waves = loadedWaves.ToArray();
    }

    GameObject FindEnemyPrefabByName(string name)
    {
        foreach (var prefab in enemyPrefabs)
            if (prefab != null && prefab.name == name)
                return prefab;
        Debug.LogWarning($"[Spawner] Không tìm thấy prefab tên {name}");
        return null;
    }
}

public class EnemyLifecycleRelay : MonoBehaviour
{
    EnemySpawner spawner;
    public bool _counted = false;

    public void Init(EnemySpawner s)
    {
        spawner = s;
        var ec = GetComponent<EnemyController>();
        if (ec != null)
        {
            ec.onDeath += _ => { if (spawner) spawner.NotifyEnemyRemoved(this); };
        }
    }

    void OnDestroy()
    {
        if (spawner) spawner.NotifyEnemyRemoved(this);
    }
>>>>>>> Stashed changes
}
