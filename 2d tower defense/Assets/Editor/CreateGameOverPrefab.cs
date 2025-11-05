#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class CreateGameOverScene
{
    [MenuItem("Tools/TD UI/Create GameOver Scene")]
    public static void Create()
    {
        EnsureFolder("Assets/Scenes");
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Canvas
        var canvasGO = new GameObject("Canvas_GameOver", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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
        var tmp = title.GetComponent<TextMeshProUGUI>();
        tmp.text = "GAME OVER";
        tmp.fontSize = 96;
        tmp.color = Color.red;
        tmp.alignment = TextAlignmentOptions.Center;
        var titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = titleRT.anchorMax = new Vector2(0.5f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0, -150);
        titleRT.sizeDelta = new Vector2(800, 200);

        // Buttons container
        var container = new GameObject("Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup));
        container.transform.SetParent(canvasGO.transform, false);
        var layout = container.GetComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 30;
        var containerRT = container.GetComponent<RectTransform>();
        containerRT.anchorMin = containerRT.anchorMax = containerRT.pivot = new Vector2(0.5f, 0.5f);
        containerRT.sizeDelta = new Vector2(400, 300);

        Button retryBtn = CreateButton(container.transform, "Retry", out _);
        Button menuBtn = CreateButton(container.transform, "Main Menu", out _);

        // Attach GameOverUI
        var ui = canvasGO.AddComponent<GameOverUI>();
        ui.gameplaySceneName = "Map1";

        // Assign Retry event
        retryBtn.onClick.AddListener(ui.Retry);

        // Assign MainMenu event (manual load Lobby)
        menuBtn.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Lobby");
        });

        EnsureEventSystem();

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/GameOverScene.unity");
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Done", "✅ GameOver Scene Created Successfully!", "OK");
    }

    static Button CreateButton(Transform parent, string label, out GameObject go)
    {
        go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 80);

        var img = go.GetComponent<Image>();
        img.color = new Color(0.2f, 0.25f, 0.35f, 1f);

        var txt = new GameObject("Text", typeof(TextMeshProUGUI));
        txt.transform.SetParent(go.transform, false);
        var tmp = txt.GetComponent<TextMeshProUGUI>();
        tmp.text = label.ToUpper();
        tmp.fontSize = 36;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        Stretch(txt.GetComponent<RectTransform>());

        return go.GetComponent<Button>();
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
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    static void EnsureFolder(string path)
    {
        var parts = path.Split('/');
        string current = parts[0];
        if (!AssetDatabase.IsValidFolder(current))
            AssetDatabase.CreateFolder("", current);
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif
