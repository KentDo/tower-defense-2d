using System.Collections;
using UnityEngine;

#region Wave data
[System.Serializable]
public class SpawnEntry
{
    public GameObject prefab;     // Prefab quái (có EnemyController)
    public int count = 5;         // Bao nhiêu con
    public float startDelay = 0f; // Trễ trước khi bắt đầu dòng spawn này
    public float interval = 0.6f; // Nhịp giữa 2 con
}

[System.Serializable]
public class Wave
{
    public string name = "Wave";
    public SpawnEntry[] entries;
    public float nextWaveDelay = 3f;

    // NEW: hệ số máu của wave (1 = giữ nguyên, 1.5 = +50%)
    public float hpMultiplier = 1f;
}
#endregion

public class EnemySpawner : MonoBehaviour
{
    [Header("Path / Spawn point")]
    public LevelManager levelManager;
    [Tooltip("Để trống = spawn tại waypoint[0]")]
    public Transform spawnPointOverride;

    [Header("Waves (đa round)")]
    public bool autoStart = true;
    public Wave[] waves;

    [Header("Legacy (1 round đơn) — dùng khi Waves rỗng")]
    public GameObject enemyPrefab;
    public bool spawnAtFirstWaypoint = true;
    public int count = 10;
    public float startDelay = 0f;
    public float interval = 1.2f;
    public bool loop = false;
    public float loopDelay = 4f;
    public bool IsRunning => (currentWaveIndex != -1) || spawningWave || alive > 0;

    [Header("Debug")]
    public int currentWaveIndex = -1;
    public int alive = 0;

    bool spawningWave;
    bool waveFinishedSpawning;

    void Start()
    {
        if (!levelManager) levelManager = LevelManager.Instance;

        if (autoStart)
        {
            if (HasWavesConfigured())
                StartCoroutine(RunWaves());
            else
                StartCoroutine(RunLegacy());
        }
    }

    // ========= API =========
    public void StartWavesByButton()
    {

        StopAllCoroutines();
        if (HasWavesConfigured()) StartCoroutine(RunWaves());
        else StartCoroutine(RunLegacy());
    }

    // ========= Core: multi-wave =========
    bool HasWavesConfigured()
    {
        if (waves == null || waves.Length == 0) return false;
        // có ít nhất 1 entry hợp lệ?
        foreach (var w in waves)
        {
            if (w != null && w.entries != null)
                foreach (var e in w.entries)
                    if (e != null && e.prefab && e.count > 0) return true;
        }
        return false;
    }

    IEnumerator RunWaves()
    {
        if (!ValidatePath()) yield break;

        for (int w = 0; w < waves.Length; w++)
        {
            var wave = waves[w];
            if (wave == null) continue;

            currentWaveIndex = w;
            spawningWave = true;
            waveFinishedSpawning = false;

            // spawn từng entry trong wave (tuần tự)
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

            // đợi clear (không còn quái sống)
            yield return new WaitUntil(() => alive <= 0);

            if (wave.nextWaveDelay > 0f)
                yield return new WaitForSeconds(wave.nextWaveDelay);
        }

        currentWaveIndex = -1;
        Debug.Log("[Spawner] All waves finished.");
    }

    // ========= Legacy single-wave (giữ cho dự án cũ) =========
    IEnumerator RunLegacy()
    {
        if (!ValidatePath()) yield break;

        do
        {
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

            // Lấy hệ số của wave hiện tại (nếu hợp lệ)
            if (currentWaveIndex >= 0 && currentWaveIndex < waves.Length)
            {
                float mul = Mathf.Max(0.01f, waves[currentWaveIndex].hpMultiplier);
                int scaled = Mathf.RoundToInt(ec.MaxHP * mul);
                ec.SetMaxHP(Mathf.Max(1, scaled));
            }
        }

        // relay đếm alive như cũ...
        var relay = go.GetComponent<EnemyLifecycleRelay>() ?? go.AddComponent<EnemyLifecycleRelay>();
        relay.Init(this);
        alive++;
    }


    Vector3 GetSpawnPos()
    {
        if (spawnPointOverride) return spawnPointOverride.position;
        if (levelManager && levelManager.PathCount > 0 && levelManager.GetPathPoint(0))
            return levelManager.GetPathPoint(0).position;
        return transform.position;
    }

    bool ValidatePath()
    {
        if (!levelManager || levelManager.PathCount < 1)
        {
            Debug.LogWarning("[Spawner] Không có LevelManager hoặc PathCount < 1");
            return false;
        }
        return true;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(GetSpawnPos(), 0.2f);
        if (levelManager && levelManager.GetPathPoint(0))
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, GetSpawnPos());
        }
    }
#endif

    // ===== Relay (gắn trên enemy) =====
    public void NotifyEnemyRemoved(EnemyLifecycleRelay relay)
    {
        if (relay == null || relay._counted) return;
        relay._counted = true;
        alive = Mathf.Max(0, alive - 1);
        // Debug.Log($"[Spawner] Enemy removed. Alive={alive}, waveIdx={currentWaveIndex}");
    }
}

/// <summary>
/// Gắn vào enemy để báo Spawner khi enemy bị Destroy (chết/đến đích).
/// </summary>
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
}
