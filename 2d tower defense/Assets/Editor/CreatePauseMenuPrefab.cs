using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class CreatePauseMenuPrefab
{
    [MenuItem("Tools/TD UI/Create Canvas_Pause Prefab")]
    public static void Create()
    {
        const string prefabFolder = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabFolder))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // Canvas
        var canvasGO = new GameObject("Canvas_Pause", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 60; // trên các HUD khác

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Nền mờ
        var panel = new GameObject("PauseMenuPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasGO.transform, false);
        var prt = (RectTransform)panel.transform;
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;

        var pimg = panel.GetComponent<Image>();
        pimg.color = new Color(0f, 0f, 0f, 0.55f);

        // Tiêu đề
        var titleGO = new GameObject("TxtTitle", typeof(TextMeshProUGUI));
        titleGO.transform.SetParent(panel.transform, false);
        var title = titleGO.GetComponent<TextMeshProUGUI>();
        title.text = "TẠM DỪNG";
        title.alignment = TextAlignmentOptions.Center;
        title.enableAutoSizing = true;
        title.fontSizeMin = 40; title.fontSizeMax = 120;
        title.color = Color.white;
        var trt = (RectTransform)titleGO.transform;
        trt.anchorMin = new Vector2(0.5f, 0.7f);
        trt.anchorMax = new Vector2(0.5f, 0.7f);
        trt.pivot = new Vector2(0.5f, 0.5f);
        trt.sizeDelta = new Vector2(800, 160);

        // Container nút
        var container = new GameObject("ButtonsContainer", typeof(RectTransform));
        container.transform.SetParent(panel.transform, false);
        var crt = (RectTransform)container.transform;
        crt.anchorMin = new Vector2(0.5f, 0.45f);
        crt.anchorMax = new Vector2(0.5f, 0.45f);
        crt.pivot = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = new Vector2(520, 260);

        var vlg = container.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 18;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        Button MakeBtn(string name, string label)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(container.transform, false);

            var img = go.GetComponent<Image>();
            img.color = new Color(1, 1, 1, 0.15f);

            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(0, 78);

            var textGO = new GameObject("Text (TMP)", typeof(TextMeshProUGUI));
            textGO.transform.SetParent(go.transform, false);
            var t = textGO.GetComponent<TextMeshProUGUI>();
            t.text = label;
            t.color = Color.white;
            t.alignment = TextAlignmentOptions.Center;
            t.enableAutoSizing = true;
            t.fontSizeMin = 28; t.fontSizeMax = 56;

            var trt2 = (RectTransform)textGO.transform;
            trt2.anchorMin = Vector2.zero; trt2.anchorMax = Vector2.one;
            trt2.offsetMin = Vector2.zero; trt2.offsetMax = Vector2.zero;

            return go.GetComponent<Button>();
        }

        var btnResume = MakeBtn("BtnResume", "Tiếp tục");
        var btnRestart = MakeBtn("BtnRestart", "Chơi lại");
        var btnMainMenu = MakeBtn("BtnMainMenu", "Về Menu chính");

        // Hook tự gắn sự kiện vào PauseResumeButton trong scene
        var hook = canvasGO.AddComponent<PauseMenuHook>();
        hook.pauseMenuPanel = panel;
        hook.btnResume = btnResume;
        hook.btnRestart = btnRestart;
        hook.btnMainMenu = btnMainMenu;

        // Ẩn mặc định
        panel.SetActive(false);

        // Lưu prefab
        var path = $"{prefabFolder}/Canvas_Pause.prefab";
        PrefabUtility.SaveAsPrefabAsset(canvasGO, path);
        Object.DestroyImmediate(canvasGO);

        Debug.Log($"[CreatePauseMenuPrefab] ✅ Created: {path}\n" +
                  "- Kéo prefab vào scene.\n" +
                  "- Không cần wire OnClick: hook sẽ tự tìm PauseResumeButton và gắn sự kiện.");
    }
}
