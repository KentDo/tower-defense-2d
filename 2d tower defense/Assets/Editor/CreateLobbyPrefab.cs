// Assets/Editor/CreateLobbyPrefab.cs
// Menu: Tools → TD UI → Create/Refresh Lobby Prefab & Scene
// Tạo Lobby prefab + scene. SettingsPanel chỉ có: Fullscreen, VSync, Close.
// Đảm bảo có EventSystem đúng module trong SCENE.

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;
using TMPro;

public static class CreateLobbyPrefab
{
    [MenuItem("Tools/TD UI/Create/Refresh Lobby Prefab & Scene")]
    public static void CreateLobby()
    {
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Scenes");

        // ===== Prefab in memory =====
        var rootGO = new GameObject("LobbyCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = rootGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = rootGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Background
        var bg = CreateUIObject("Background", rootGO);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.05f, 0.06f, 0.08f, 1f); // #0D0F14
        StretchFull(bg.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

        // Title
        var title = CreateTMP("Title", rootGO, "Tower Defense", 64, TextAlignmentOptions.Center);
        var rtTitle = title.GetComponent<RectTransform>();
        rtTitle.anchorMin = rtTitle.anchorMax = new Vector2(0.5f, 1f);
        rtTitle.pivot = new Vector2(0.5f, 1f);
        rtTitle.anchoredPosition = new Vector2(0, -80);
        rtTitle.sizeDelta = new Vector2(800, 120);

        // Buttons column
        var btnCol = CreateUIObject("Buttons", rootGO);
        var rtCol = btnCol.GetComponent<RectTransform>();
        rtCol.anchorMin = rtCol.anchorMax = rtCol.pivot = new Vector2(0.5f, 0.5f);
        rtCol.sizeDelta = new Vector2(480, 520);
        var vlayout = btnCol.AddComponent<VerticalLayoutGroup>();
        vlayout.childAlignment = TextAnchor.MiddleCenter;
        vlayout.spacing = 18;
        vlayout.childControlWidth = vlayout.childControlHeight = false;

        // Buttons
        Button btnPlay = CreateButton(btnCol.transform, "Play", out _);
        Button btnReplay = CreateButton(btnCol.transform, "Replay", out _);
        Button btnContinue = CreateButton(btnCol.transform, "Continue", out _);
        Button btnSettings = CreateButton(btnCol.transform, "Settings", out _);
        Button btnQuit = CreateButton(btnCol.transform, "Quit", out _);

        // Settings Panel (no dropdown)
        var settingsPanel = CreateUIObject("SettingsPanel", rootGO);
        settingsPanel.transform.SetAsLastSibling(); // on top
        var settingsRT = settingsPanel.GetComponent<RectTransform>();
        settingsRT.anchorMin = settingsRT.anchorMax = settingsRT.pivot = new Vector2(0.5f, 0.5f);
        settingsRT.sizeDelta = new Vector2(700, 420);

        var panelBG = settingsPanel.AddComponent<Image>();
        panelBG.color = new Color(0f, 0f, 0f, 0.80f);

        var panelLayout = settingsPanel.AddComponent<VerticalLayoutGroup>();
        panelLayout.childAlignment = TextAnchor.UpperCenter;
        panelLayout.spacing = 16;
        panelLayout.padding = new RectOffset(24, 24, 24, 24);

        var stitle = CreateTMP("SettingsTitle", settingsPanel, "SETTINGS", 48, TextAlignmentOptions.Center);
        stitle.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 80);

        // Row toggles
        var rowToggles = CreateUIObject("RowToggles", settingsPanel);
        var h1 = rowToggles.AddComponent<HorizontalLayoutGroup>();
        h1.spacing = 20;
        h1.childAlignment = TextAnchor.MiddleCenter;
        h1.childControlWidth = h1.childControlHeight = false;
        rowToggles.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 60);

        Toggle tFullscreen = CreateLabeledToggle(rowToggles.transform, "Fullscreen", out _);
        Toggle tVsync = CreateLabeledToggle(rowToggles.transform, "VSync", out _);

        // Close
        Button btnClose = CreateButton(settingsPanel.transform, "Close", out _);

        // Attach scripts
        var lobbyUI = rootGO.AddComponent<LobbyUI>();
        lobbyUI.btnPlay = btnPlay;
        lobbyUI.btnReplay = btnReplay;
        lobbyUI.btnContinue = btnContinue;
        lobbyUI.btnSettings = btnSettings;
        lobbyUI.btnQuit = btnQuit;
        lobbyUI.settingsPanel = settingsPanel;
        lobbyUI.defaultFirstLevel = "Map1"; // hợp tên scene của bạn

        var ssm = settingsPanel.AddComponent<SimpleSettingsManager>();
        ssm.toggleFullscreen = tFullscreen;
        ssm.toggleVSync = tVsync;
        lobbyUI.simpleSettingsManager = ssm;

        btnClose.onClick.AddListener(() => lobbyUI.OnCloseSettings());
        settingsPanel.SetActive(false);

        // Save prefab
        string prefabPath = "Assets/Prefabs/LobbyCanvas.prefab";
        PrefabUtility.SaveAsPrefabAsset(rootGO, prefabPath);
        Object.DestroyImmediate(rootGO);

        // ===== Scene + EventSystem =====
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        PrefabUtility.InstantiatePrefab(prefab);
        EnsureEventSystemInScene();

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Lobby.unity");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Done", "Created/Refreshed Lobby prefab & Lobby scene.", "OK");
    }

    // ---------- Helpers ----------
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

    static GameObject CreateUIObject(string name, GameObject parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 200);
        return go;
    }

    static void StretchFull(RectTransform rt, Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }

    static Button CreateButton(Transform parent, string label, out GameObject root)
    {
        root = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        root.transform.SetParent(parent, false);
        var rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(380, 72);

        var img = root.GetComponent<Image>();
        img.color = new Color(0.20f, 0.22f, 0.30f, 1f);

        var txt = CreateTMP("Text", root, label, 28, TextAlignmentOptions.Center);
        var txtRT = txt.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;

        var btn = root.GetComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.35f, 1f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.22f, 1f);
        btn.colors = colors;
        return btn;
    }

    static Toggle CreateLabeledToggle(Transform parent, string label, out GameObject root)
    {
        root = new GameObject(label + "Toggle", typeof(RectTransform));
        root.transform.SetParent(parent, false);
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 48);

        var tRoot = new GameObject("Toggle", typeof(RectTransform), typeof(Image), typeof(Toggle));
        tRoot.transform.SetParent(root.transform, false);
        var tRT = tRoot.GetComponent<RectTransform>();
        tRT.anchorMin = tRT.anchorMax = new Vector2(0, 0.5f);
        tRT.pivot = new Vector2(0, 0.5f);
        tRT.anchoredPosition = Vector2.zero;
        tRT.sizeDelta = new Vector2(32, 32);
        var tBG = tRoot.GetComponent<Image>();
        tBG.color = new Color(0.2f, 0.2f, 0.28f, 1f);

        var check = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        check.transform.SetParent(tRoot.transform, false);
        var cRT = check.GetComponent<RectTransform>();
        cRT.anchorMin = cRT.anchorMax = new Vector2(0.5f, 0.5f);
        cRT.pivot = new Vector2(0.5f, 0.5f);
        cRT.sizeDelta = new Vector2(18, 18);
        var cImg = check.GetComponent<Image>();
        cImg.color = Color.white;

        var toggle = tRoot.GetComponent<Toggle>();
        toggle.targetGraphic = tBG;
        toggle.graphic = cImg;
        toggle.isOn = true;

        var labelTMP = CreateTMP("Label", root, label, 26, TextAlignmentOptions.MidlineLeft);
        var lrt = labelTMP.GetComponent<RectTransform>();
        lrt.anchorMin = lrt.anchorMax = new Vector2(0, 0.5f);
        lrt.pivot = new Vector2(0, 0.5f);
        lrt.anchoredPosition = new Vector2(48, 0);
        lrt.sizeDelta = new Vector2(200, 48);

        return toggle;
    }

    static GameObject CreateTMP(string name, GameObject parent, string text, int size, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = Color.white;
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 80);
        return go;
    }

    static void EnsureEventSystemInScene()
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
}
#endif
