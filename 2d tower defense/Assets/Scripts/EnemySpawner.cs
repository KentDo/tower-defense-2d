using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab & Path")]
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

    [Header("Auto Start")]
    public bool playOnStart = true;

    void Start()
    {
        if (!levelManager) levelManager = LevelManager.Instance;
        if (playOnStart) StartWave();
    }

    public void StartWave(int? overrideCount = null)
    {
        StopAllCoroutines();
        StartCoroutine(SpawnWave(overrideCount ?? count));
    }

    IEnumerator SpawnWave(int c)
    {
        if (!enemyPrefab)
        {
            Debug.LogWarning("[Spawner] Chưa gán enemyPrefab");
            yield break;
        }

        if (!levelManager || levelManager.PathCount < 2)
        {
            Debug.LogWarning("[Spawner] Không có LevelManager hoặc path < 2");
            yield break;
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
}
