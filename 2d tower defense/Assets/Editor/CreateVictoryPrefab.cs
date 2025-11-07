using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class CreateVictoryPrefab
{
    [MenuItem("Tools/TD UI/Create Canvas_Victory Prefab")]
    public static void Create()
    {
        const string prefabFolder = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabFolder))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // Canvas setup
        var canvasGO = new GameObject("Canvas_Victory", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Panel background
        var panel = new GameObject("PanelVictory", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasGO.transform, false);
        var img = panel.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.6f);

        var rect = (RectTransform)panel.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Title text
        var txtGO = new GameObject("TxtVictory", typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(panel.transform, false);
        var txt = txtGO.GetComponent<TextMeshProUGUI>();
        txt.text = "🎉 CHIẾN THẮNG 🎉";
        txt.alignment = TextAlignmentOptions.Center;
        txt.enableAutoSizing = true;
        txt.fontSizeMin = 40; txt.fontSizeMax = 120;
        txt.color = new Color(1f, 0.9f, 0.3f); // vàng nhạt
        var txtRT = (RectTransform)txtGO.transform;
        txtRT.anchorMin = new Vector2(0.5f, 0.8f);
        txtRT.anchorMax = new Vector2(0.5f, 0.8f);
        txtRT.pivot = new Vector2(0.5f, 0.5f);
        txtRT.anchoredPosition = Vector2.zero;
        txtRT.sizeDelta = new Vector2(800, 200);

        // Buttons container
        var container = new GameObject("ButtonsContainer", typeof(RectTransform));
        container.transform.SetParent(panel.transform, false);
        var cRT = (RectTransform)container.transform;
        cRT.anchorMin = new Vector2(0.5f, 0.4f);
        cRT.anchorMax = new Vector2(0.5f, 0.4f);
        cRT.pivot = new Vector2(0.5f, 0.5f);
        cRT.anchoredPosition = Vector2.zero;
        cRT.sizeDelta = new Vector2(600, 240);

        var vlg = container.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 30;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        // Button helper
        Button MakeButton(string name, string label)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(container.transform, false);
            var btnImg = go.GetComponent<Image>();
            btnImg.color = new Color(1, 1, 1, 0.15f);
            var btn = go.GetComponent<Button>();

            var txtBtnGO = new GameObject("Text (TMP)", typeof(TextMeshProUGUI));
            txtBtnGO.transform.SetParent(go.transform, false);
            var txtB = txtBtnGO.GetComponent<TextMeshProUGUI>();
            txtB.text = label;
            txtB.alignment = TextAlignmentOptions.Center;
            txtB.color = Color.white;
            txtB.enableAutoSizing = true;
            txtB.fontSizeMin = 28; txtB.fontSizeMax = 60;
            var tRT = (RectTransform)txtBtnGO.transform;
            tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
            tRT.offsetMin = Vector2.zero; tRT.offsetMax = Vector2.zero;

            var bRT = (RectTransform)go.transform;
            bRT.sizeDelta = new Vector2(0, 100);
            return btn;
        }

        var btnRetry = MakeButton("BtnRetry", "Chơi lại");
        var btnMenu = MakeButton("BtnMainMenu", "Về Menu chính");

        // Gắn script VictoryUI
        var script = canvasGO.AddComponent<VictoryUI>();
        script.btnRetry = btnRetry;
        script.btnMainMenu = btnMenu;

        // Lưu prefab
        var path = $"{prefabFolder}/Canvas_Victory.prefab";
        PrefabUtility.SaveAsPrefabAsset(canvasGO, path);
        Object.DestroyImmediate(canvasGO);

        Debug.Log($"[CreateVictoryPrefab] ✅ Created prefab at: {path}");
    }
}
