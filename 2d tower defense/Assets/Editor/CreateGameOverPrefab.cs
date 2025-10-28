// Assets/Editor/CreateGameOverPrefab.cs
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class CreateGameOverPrefab
{
    [MenuItem("Tools/TD UI/Create Canvas_GameOverUI Prefab")]
    public static void Create()
    {
        const string prefabFolder = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabFolder))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // Canvas
        var canvasGO = new GameObject("Canvas_GameOverUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        // TxtLose
        var lose = new GameObject("TxtLose", typeof(TextMeshProUGUI));
        lose.transform.SetParent(canvasGO.transform, false);
        var tmpLose = lose.GetComponent<TextMeshProUGUI>();
        tmpLose.text = "YOU LOSE";
        tmpLose.enableAutoSizing = true;
        tmpLose.fontSizeMin = 48; tmpLose.fontSizeMax = 96;
        tmpLose.alignment = TextAlignmentOptions.Center;

        var loseRT = (RectTransform)lose.transform;
        loseRT.anchorMin = new Vector2(0.5f, 0.5f);
        loseRT.anchorMax = new Vector2(0.5f, 0.5f);
        loseRT.anchoredPosition = new Vector2(0, 200);
        loseRT.sizeDelta = new Vector2(800, 160);

        // BtnRetry
        var btnGO = new GameObject("BtnRetry", typeof(Image), typeof(Button));
        btnGO.transform.SetParent(canvasGO.transform, false);
        var btn = btnGO.GetComponent<Button>();

        var btnRT = (RectTransform)btnGO.transform;
        btnRT.anchorMin = new Vector2(0.5f, 0.5f);
        btnRT.anchorMax = new Vector2(0.5f, 0.5f);
        btnRT.anchoredPosition = Vector2.zero;
        btnRT.sizeDelta = new Vector2(360, 120);

        var label = new GameObject("Text (TMP)", typeof(TextMeshProUGUI));
        label.transform.SetParent(btnGO.transform, false);
        var t = label.GetComponent<TextMeshProUGUI>();
        t.text = "Retry";
        t.enableAutoSizing = true;
        t.fontSizeMin = 28; t.fontSizeMax = 44;
        t.alignment = TextAlignmentOptions.Center;

        var lrt = (RectTransform)label.transform;
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;

        // Add GameOverUI holder
        var holder = new GameObject("UI_Root");
        holder.transform.SetParent(canvasGO.transform, false);
        holder.AddComponent<GameOverUI>(); // bạn sẽ có file GameOverUI.cs

        // Lưu prefab
        var path = $"{prefabFolder}/Canvas_GameOverUI.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(canvasGO, path);

        Object.DestroyImmediate(canvasGO);
        Debug.Log($"[CreateGameOverPrefab] Saved: {path}");
    }
}
