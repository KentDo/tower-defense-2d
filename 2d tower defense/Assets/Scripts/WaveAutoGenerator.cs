using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyPoolItem
{
    public string name;
    public GameObject prefab;
    [Min(1)] public int cost = 1;           // độ “đắt” của quái (để tính ngân sách)
    [Range(0f, 1f)] public float weight = 1f;// xác suất được chọn
    public int unlockWave = 1;               // chỉ xuất hiện từ wave này
    public Vector2Int groupSize = new Vector2Int(1, 1); // spawn theo nhóm
}

public class WaveAutoGenerator : MonoBehaviour
{
    [Header("Refs")]
    public EnemySpawner spawner;    // gắn EnemySpawner (cùng object cũng được)

    [Header("Auto Settings")]
    public int numWaves = 10;
    public int randomSeed = 0;      // 0 = dùng UnityEngine.Random (không cố định)
    public float interWaveDelay = 3f;

    [Header("Difficulty Budget")]
    public int baseBudget = 8;      // ngân sách wave 1
    public int budgetPerWave = 5;   // tăng thêm mỗi wave
    public float budgetMulPer5Waves = 1.2f; // mỗi 5 wave nhân thêm (tăng tốc độ khó)

    [Header("HP Multiplier Curve")]
    public float hpBase = 1.0f;     // hpMultiplier ở wave 1
    public float hpPerWave = 0.1f;  // cộng theo wave (vd 0.1 => +10% mỗi wave)
    public AnimationCurve hpCurve = AnimationCurve.Linear(1, 1, 10, 2f);
    // hpMultiplier = (hpBase + (w-1)*hpPerWave) * hpCurve.Evaluate(w)

    [Header("Spawn Rhythm")]
    public float entryStartDelay = 0f; // trễ trước dòng spawn trong wave
    public float intervalInsideGroup = 0.6f; // nhịp khi spawn trong cùng dòng

    [Header("Enemy Pool")]
    public EnemyPoolItem[] pool;

    void Reset()
    {
        spawner = GetComponent<EnemySpawner>();
    }

    void Awake()
    {
        if (!spawner) spawner = GetComponent<EnemySpawner>();
    }

    void Start()
    {
        if (!spawner)
        {
            Debug.LogWarning("[WaveAutoGenerator] Missing EnemySpawner.");
            return;
        }
        AutoBuildWaves();
        // cho spawner dùng waves auto:
        spawner.autoStart = spawner.autoStart; // giữ nguyên cờ của bạn (Start button vẫn hoạt động)
    }

    public void AutoBuildWaves()
    {
        if (pool == null || pool.Length == 0)
        {
            Debug.LogWarning("[WaveAutoGenerator] Enemy pool rỗng.");
            return;
        }

        var waves = new List<Wave>();
        System.Random rng = (randomSeed != 0) ? new System.Random(randomSeed) : null;

        for (int w = 1; w <= Mathf.Max(1, numWaves); w++)
        {
            int budget = CalcBudget(w);
            float hpMul = CalcHpMultiplier(w);

            // lọc pool theo unlock
            var candidates = new List<EnemyPoolItem>();
            float totalWeight = 0f;
            foreach (var it in pool)
            {
                if (it == null || it.prefab == null) continue;
                if (w < it.unlockWave) continue;
                if (it.weight <= 0f) continue;
                candidates.Add(it);
                totalWeight += it.weight;
            }

            if (candidates.Count == 0)
            {
                Debug.LogWarning($"[WaveAutoGenerator] Wave {w}: không có quái khả dụng (kiểm tra unlockWave/weight).");
                continue;
            }

            var entries = new List<SpawnEntry>();

            // rút quái theo ngân sách
            int safety = 1000; // tránh loop vô hạn
            while (budget > 0 && safety-- > 0)
            {
                var choice = WeightedPick(candidates, totalWeight, rng);
                if (choice == null) break;

                int group = UnityRandRange(choice.groupSize.x, choice.groupSize.y, rng);
                group = Mathf.Max(1, group);

                int cost = choice.cost * group;
                if (cost > budget)
                {
                    // Thử nhóm nhỏ hơn
                    if (group > 1)
                    {
                        int maxPossible = Mathf.Max(1, budget / choice.cost);
                        if (maxPossible <= 0) { break; }
                        group = maxPossible;
                        cost = choice.cost * group;
                    }
                    else
                    {
                        // chọn quái khác rẻ hơn
                        // tạm bỏ bốc chọn này 1 lần
                        continue;
                    }
                }

                entries.Add(new SpawnEntry
                {
                    prefab = choice.prefab,
                    count = group,
                    startDelay = entryStartDelay,
                    interval = intervalInsideGroup
                });

                budget -= cost;
            }

            waves.Add(new Wave
            {
                name = $"Wave {w}",
                entries = entries.ToArray(),
                nextWaveDelay = interWaveDelay,
                hpMultiplier = hpMul
            });
        }

        spawner.waves = waves.ToArray();
        Debug.Log($"[WaveAutoGenerator] Generated {waves.Count} waves.");
    }

    int CalcBudget(int wave)
    {
        // ngân sách cơ bản + tăng tuyến tính + boost theo mốc 5 waves
        float mul = Mathf.Pow(budgetMulPer5Waves, Mathf.Floor((wave - 1) / 5f));
        return Mathf.Max(1, Mathf.RoundToInt((baseBudget + (wave - 1) * budgetPerWave) * mul));
    }

    float CalcHpMultiplier(int wave)
    {
        float curve = (hpCurve != null && hpCurve.keys.Length > 0) ? hpCurve.Evaluate(wave) : 1f;
        return Mathf.Max(0.01f, (hpBase + (wave - 1) * hpPerWave) * curve);
    }

    EnemyPoolItem WeightedPick(List<EnemyPoolItem> list, float totalWeight, System.Random rng)
    {
        if (list == null || list.Count == 0) return null;

        float r;
        if (rng != null) r = (float)rng.NextDouble() * totalWeight;
        else r = UnityEngine.Random.value * totalWeight;

        float cum = 0f;
        foreach (var it in list)
        {
            cum += it.weight;
            if (r <= cum) return it;
        }
        return list[list.Count - 1];
    }

    int UnityRandRange(int a, int b, System.Random rng)
    {
        if (a > b) { int t = a; a = b; b = t; }
        if (rng != null) return rng.Next(a, b + 1);
        return UnityEngine.Random.Range(a, b + 1);
    }
}
