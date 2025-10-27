// Assets/Editor/CreateQuizPrefab.cs
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class CreateQuizPrefab
{
    [MenuItem("Tools/TD UI/Create Canvas_Quiz Prefab")]
    public static void Create()
    {
        const string prefabFolder = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabFolder))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // Canvas (Overlay + Scaler)
        var canvasGO = new GameObject("Canvas_Quiz", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10; // nổi trên các canvas khác

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Panel phủ nền (đen mờ, stretch full)
        var panel = new GameObject("PanelQuiz", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasGO.transform, false);
        var prt = (RectTransform)panel.transform;
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;

        var pimg = panel.GetComponent<Image>();
        pimg.color = new Color(0, 0, 0, 0.5f); // đen 50%

        // Container căn giữa nội dung
        var content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(panel.transform, false);
        var crt = (RectTransform)content.transform;
        crt.anchorMin = new Vector2(0.5f, 0.5f);
        crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.pivot = new Vector2(0.5f, 0.5f);
        crt.anchoredPosition = Vector2.zero;
        crt.sizeDelta = new Vector2(900, 560);

        // Layout dọc cho toàn khối (Câu hỏi + Container đáp án)
        var topVLG = content.AddComponent<VerticalLayoutGroup>();
        topVLG.padding = new RectOffset(24, 24, 24, 24);
        topVLG.spacing = 24;
        topVLG.childAlignment = TextAnchor.UpperCenter;
        topVLG.childControlWidth = true;
        topVLG.childControlHeight = true;
        topVLG.childForceExpandWidth = true;
        topVLG.childForceExpandHeight = false;

        // TxtQuestion
        var txtQ = new GameObject("TxtQuestion", typeof(TextMeshProUGUI));
        txtQ.transform.SetParent(content.transform, false);
        var q = txtQ.GetComponent<TextMeshProUGUI>();
        q.enableAutoSizing = true;
        q.fontSizeMin = 28; q.fontSizeMax = 56;
        q.alignment = TextAlignmentOptions.Center;
        q.text = "Câu hỏi sẽ hiện ở đây…";
        q.color = Color.white;

        var qrt = (RectTransform)txtQ.transform;
        qrt.sizeDelta = new Vector2(0, 160);

        // Container các đáp án (giữa, đều nhau)
        var answers = new GameObject("AnswerContainer", typeof(RectTransform));
        answers.transform.SetParent(content.transform, false);
        var art = (RectTransform)answers.transform;
        art.sizeDelta = new Vector2(0, 360);

        var vlg = answers.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 16;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var fitter = answers.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Helper tạo nút
        Button MakeBtn(string name, string label)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(answers.transform, false);

            var img = go.GetComponent<Image>();
            img.color = new Color(1, 1, 1, 0.15f); // trắng nhạt

            var btnRT = (RectTransform)go.transform;
            btnRT.sizeDelta = new Vector2(0, 80); // Height cố định, width theo layout

            var btn = go.GetComponent<Button>();
            var nav = btn.navigation;
            nav.mode = Navigation.Mode.None;
            btn.navigation = nav;

            // Label TMP
            var labelGO = new GameObject("Text (TMP)", typeof(TextMeshProUGUI));
            labelGO.transform.SetParent(go.transform, false);
            var t = labelGO.GetComponent<TextMeshProUGUI>();
            t.text = label;
            t.enableAutoSizing = true;
            t.fontSizeMin = 24; t.fontSizeMax = 40;
            t.alignment = TextAlignmentOptions.MidlineLeft;
            t.color = Color.white;

            var lrt = (RectTransform)labelGO.transform;
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(16, 8);
            lrt.offsetMax = new Vector2(-16, -8);

            return btn;
        }

        var bA = MakeBtn("BtnA", "Đáp án A");
        var bB = MakeBtn("BtnB", "Đáp án B");
        var bC = MakeBtn("BtnC", "Đáp án C");
        var bD = MakeBtn("BtnD", "Đáp án D");

        // Ẩn panel mặc định (chỉ hiện khi gọi ShowQuiz)
        panel.SetActive(false);

        // Lưu prefab
        var path = $"{prefabFolder}/Canvas_Quiz.prefab";
        PrefabUtility.SaveAsPrefabAsset(canvasGO, path);
        Object.DestroyImmediate(canvasGO);

        Debug.Log($"[CreateQuizPrefab] Saved: {path}");
    }
}
