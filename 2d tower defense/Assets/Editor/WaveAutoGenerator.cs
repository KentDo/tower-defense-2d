#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class WaveAutoGenerator : EditorWindow
{
    GameObject enemyPrefab;
    int numWaves = 10;
    int countPerWave = 5;
    float hpMultiplier = 1.1f;
    int seed = 123;

    [MenuItem("Tools/Wave Auto Generator")]
    public static void ShowWindow()
    {
        GetWindow<WaveAutoGenerator>("Wave Auto Generator");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Wave Settings", EditorStyles.boldLabel);
        enemyPrefab = (GameObject)EditorGUILayout.ObjectField("Enemy Prefab", enemyPrefab, typeof(GameObject), false);
        numWaves = EditorGUILayout.IntField("Number of Waves", numWaves);
        countPerWave = EditorGUILayout.IntField("Enemy/ Wave", countPerWave);
        hpMultiplier = EditorGUILayout.FloatField("HP Multiplier", hpMultiplier);
        seed = EditorGUILayout.IntField("Seed", seed);

        if (GUILayout.Button("Preview"))
        {
            PreviewWaves();
        }

        if (GUILayout.Button("Export CSV"))
        {
            ExportCSV();
        }
    }

    void PreviewWaves()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 1; i <= numWaves; i++)
        {
            float hp = Mathf.Pow(hpMultiplier, i - 1);
            sb.AppendLine($"Wave {i}: {countPerWave} x {enemyPrefab?.name ?? "Enemy"} | HP x {hp:0.00}");
        }
        EditorUtility.DisplayDialog("Preview Waves", sb.ToString(), "OK");
    }

    void ExportCSV()
    {
        string path = EditorUtility.SaveFilePanel("Export CSV", "", "waves.csv", "csv");
        if (string.IsNullOrEmpty(path)) return;

        using (var sw = new StreamWriter(path))
        {
            sw.WriteLine("WaveIndex,EnemyPrefabName,Count,SpawnInterval,HealthMultiplier");
            for (int i = 1; i <= numWaves; i++)
            {
                float hp = Mathf.Pow(hpMultiplier, i - 1);
                sw.WriteLine($"{i},{enemyPrefab?.name ?? "Enemy"},{countPerWave},0.6,{hp:0.00}");
            }
        }
        EditorUtility.DisplayDialog("Export CSV", "Đã lưu file CSV thành công!", "OK");
    }
}
#endif
