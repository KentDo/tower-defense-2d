#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;
using TMPro;

public static class CreateMapSelectPrefab
{
    [MenuItem("Tools/TD UI/Create MapSelect Scene")]
    public static void CreateMapSelectScene()
    {
        EnsureFolder("Assets/Scenes");

        // === Tạo Scene mới ===
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Canvas
        var canvasGO = new GameObject("Canvas_MapSelect", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Background
        var bg = new GameObject("Background", typeof(Image));
        bg.transform.SetParent(canvasGO.transform, false);
        var bgImg = bg.GetComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.85f);
        Stretch(bg.GetComponent<RectTransform>());

        // Title
        var title = new GameObject("Title", typeof(TextMeshProUGUI));
        title.transform.SetParent(canvasGO.transform, false);
        var titleTMP = title.GetComponent<TextMeshProUGUI>();
        titleTMP.text = "SELECT MAP";
        titleTMP.fontSize = 64;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Color.white;
        var titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 1f);
        titleRT.anchorMax = new Vector2(0.5f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0, -100);
        titleRT.sizeDelta = new Vector2(800, 120);

        // Map Buttons container
        var btnContainer = new GameObject("MapButtons", typeof(RectTransform), typeof(VerticalLayoutGroup));
        btnContainer.transform.SetParent(canvasGO.transform, false);
        var rt = btnContainer.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(400, 500);

        var vlayout = btnContainer.GetComponent<VerticalLayoutGroup>();
        vlayout.childAlignment = TextAnchor.MiddleCenter;
        vlayout.spacing = 24;
        vlayout.childControlHeight = false;
        vlayout.childControlWidth = false;

        // Tạo các nút Map1–3
        Button btn1 = CreateButton(btnContainer.transform, "Map1", out _);
        Button btn2 = CreateButton(btnContainer.transform, "Map2", out _);
        Button btn3 = CreateButton(btnContainer.transform, "Map3", out _);

        // Return button
        Button btnReturn = CreateButton(canvasGO.transform, "Return to Menu", out GameObject returnGO);
        var rtReturn = returnGO.GetComponent<RectTransform>();
        rtReturn.anchorMin = rtReturn.anchorMax = new Vector2(0.5f, 0f);
        rtReturn.pivot = new Vector2(0.5f, 0f);
        rtReturn.anchoredPosition = new Vector2(0, 50);
        rtReturn.sizeDelta = new Vector2(360, 80);

        // Gắn script điều khiển
        var controller = canvasGO.AddComponent<MapSelectUI>();
        controller.btnMap1 = btn1;
        controller.btnMap2 = btn2;
        controller.btnMap3 = btn3;
        controller.btnReturn = btnReturn;

        // EventSystem
        EnsureEventSystem();

        // Lưu Scene
        string scenePath = "Assets/Scenes/MapSelect.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Done", "✅ MapSelect scene created successfully!", "OK");
    }

    // ===== Helpers =====
    static Button CreateButton(Transform parent, string label, out GameObject root)
    {
        root = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        root.transform.SetParent(parent, false);

        var rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(350, 80);
        var img = root.GetComponent<Image>();
        img.color = new Color(0.22f, 0.24f, 0.33f, 1f);

        var txt = new GameObject("Text", typeof(TextMeshProUGUI));
        txt.transform.SetParent(root.transform, false);
        var tmp = txt.GetComponent<TextMeshProUGUI>();
        tmp.text = label.ToUpper();
        tmp.fontSize = 32;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        var txtRT = txt.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;

        var btn = root.GetComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.35f, 0.45f, 1f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.25f, 1f);
        btn.colors = colors;

        return btn;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;

#if ENABLE_INPUT_SYSTEM
        new GameObject("EventSystem",
            typeof(EventSystem),
            typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
#else
        new GameObject("EventSystem",
            typeof(EventSystem),
            typeof(StandaloneInputModule));
#endif
    }

    static void EnsureFolder(string path)
    {
        var parts = path.Split('/');
        string curr = parts[0];
        if (!AssetDatabase.IsValidFolder(curr)) AssetDatabase.CreateFolder("", curr);
        for (int i = 1; i < parts.Length; i++)
        {
            string next = curr + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(curr, parts[i]);
            curr = next;
        }
    }
}
#endif
