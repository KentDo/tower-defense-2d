using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class CreateCountdownHUD
{
    [MenuItem("Tools/TD UI/Create Canvas_Countdown Prefab")]
    public static void Create()
    {
        const string prefabFolder = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabFolder))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // Canvas
        var canvasGO = new GameObject("Canvas_Countdown", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50; // trên HUD khác

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Overlay mờ (có thể tắt)
        var dim = new GameObject("DimOverlay", typeof(Image));
        dim.transform.SetParent(canvasGO.transform, false);
        var dimImg = dim.GetComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.45f); // đen 45%
        var dimRT = (RectTransform)dim.transform;
        dimRT.anchorMin = Vector2.zero; dimRT.anchorMax = Vector2.one;
        dimRT.offsetMin = Vector2.zero; dimRT.offsetMax = Vector2.zero;

        // Text TMP căn giữa
        var txtGO = new GameObject("TxtCountdown", typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(canvasGO.transform, false);
        var tmp = txtGO.GetComponent<TextMeshProUGUI>();
        tmp.text = "Bắt đầu sau 5s";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 36; tmp.fontSizeMax = 120;
        tmp.color = Color.white;
        var tRT = (RectTransform)txtGO.transform;
        tRT.anchorMin = new Vector2(0.5f, 0.5f);
        tRT.anchorMax = new Vector2(0.5f, 0.5f);
        tRT.pivot = new Vector2(0.5f, 0.5f);
        tRT.anchoredPosition = Vector2.zero;
        tRT.sizeDelta = new Vector2(800, 200);

        // Gợi ý: có thể ẩn overlay mờ mặc định nếu không thích
        // dim.SetActive(false);

        // Lưu prefab
        var path = $"{prefabFolder}/Canvas_Countdown.prefab";
        PrefabUtility.SaveAsPrefabAsset(canvasGO, path);
        Object.DestroyImmediate(canvasGO);

        Debug.Log($"[CreateCountdownHUD] ✅ Created: {path}\n- Kéo 'TxtCountdown' vào EnemySpawner.countdownText.");
    }
}
