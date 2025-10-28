using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class QuizManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panelQuiz;       // Panel tổng
    [SerializeField] private TMP_Text txtQuestion;       // Câu hỏi
    [SerializeField] private Button[] answerButtons;     // 4 nút trả lời

    [Header("Visuals")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color correctColor = new Color(0.6f, 1f, 0.6f);
    [SerializeField] private Color wrongColor = new Color(1f, 0.6f, 0.6f);
    [SerializeField] private float revealDelay = 0.6f;   // thời gian highlight

    private List<QuizQuestion> questions = new List<QuizQuestion>();
    private int currentIndex = -1;
    private bool isShowing;
    private bool inputLocked;

    // Singleton (nếu muốn gọi ở chỗ khác)
    public static QuizManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
        // Ẩn panel lúc khởi tạo
        if (panelQuiz != null) panelQuiz.SetActive(false);
    }

    void Start()
    {
        LoadQuestionsFromResources();

        // Gắn listener một lần
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int idx = i;
            answerButtons[i].onClick.AddListener(() => OnClickAnswer(idx));
        }
    }

    void Update()
    {
        if (!isShowing || inputLocked) return;

        var kb = Keyboard.current;
        if (kb == null) return; // không có bàn phím (mobile, v.v.)

        if (kb.aKey.wasPressedThisFrame) OnClickAnswer(0);
        else if (kb.bKey.wasPressedThisFrame) OnClickAnswer(1);
        else if (kb.cKey.wasPressedThisFrame) OnClickAnswer(2);
        else if (kb.dKey.wasPressedThisFrame) OnClickAnswer(3);
    }

    private void LoadQuestionsFromResources()
    {
        TextAsset json = Resources.Load<TextAsset>("questions");
        if (json == null)
        {
            Debug.LogError("[QuizManager] Không tìm thấy Resources/questions.json");
            return;
        }

        // Đọc theo key "questions" thay vì "items"
        var list = JsonUtility.FromJson<QuizQuestionList>(json.text);
        if (list != null && list.questions != null)
            questions = list.questions;
        else
            Debug.LogError("[QuizManager] Parse questions.json lỗi hoặc sai key.");
    }


    // Gọi hàm này từ LevelManager khi lives <= 0
    public void ShowQuiz()
    {
        if (questions == null || questions.Count == 0)
        {
            Debug.LogWarning("[QuizManager] Không có câu hỏi → coi như sai → Game Over");
            GoToGameOver();
            return;
        }

        if (panelQuiz != null) panelQuiz.SetActive(true);
        isShowing = true;
        inputLocked = false;
        Time.timeScale = 0f; // Pause game

        // Reset màu button
        foreach (var b in answerButtons)
        {
            if (b == null) continue;
            var img = b.targetGraphic as Graphic;
            if (img != null) img.color = normalColor;

            var txt = b.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.alpha = 1f;
            b.interactable = true;
        }

        // Lấy câu ngẫu nhiên không lặp
        GameSession.ResetQuestionsIfNeeded(questions.Count);
        do currentIndex = Random.Range(0, questions.Count);
        while (GameSession.UsedQuestionIndices.Contains(currentIndex));
        GameSession.UsedQuestionIndices.Add(currentIndex);

        // Bind UI
        var q = questions[currentIndex];
        if (txtQuestion) txtQuestion.text = q.question;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            var label = answerButtons[i].GetComponentInChildren<TMP_Text>();
            if (label) label.text = (i < q.answers.Count) ? q.answers[i] : $"Option {i + 1}";
        }
    }

    private void OnClickAnswer(int btnIndex)
    {
        if (inputLocked) return;
        inputLocked = true;

        var q = questions[currentIndex];
        bool isCorrect = (btnIndex == q.correctIndex);

        // highlight
        StartCoroutine(RevealAndResolve(isCorrect, btnIndex, q.correctIndex));
    }

    private IEnumerator RevealAndResolve(bool isCorrect, int chosen, int correct)
    {
        // Đổi màu nút đã chọn + nút đúng
        for (int i = 0; i < answerButtons.Length; i++)
        {
            var img = answerButtons[i].targetGraphic as Graphic;
            if (img == null) continue;

            if (i == correct) img.color = correctColor;
            else if (i == chosen && !isCorrect) img.color = wrongColor;
            else img.color = normalColor;

            answerButtons[i].interactable = false;
        }

        yield return new WaitForSecondsRealtime(revealDelay);

        if (isCorrect)
        {
            // Hồi 1 máu & tiếp tục
            ResumeGameWith1Life();
        }
        else
        {
            // Sai → Game Over
            GoToGameOver();
        }
    }

    private void ResumeGameWith1Life()
    {
        // Ẩn panel, hồi 1 máu, resume time
        if (panelQuiz) panelQuiz.SetActive(false);
        isShowing = false;
        inputLocked = false;

        var lm = LevelManager.Instance;
        if (lm != null)
        {
            lm.RevivePlayer(1);             // hồi +1 máu
            lm.ActivateInvulnerability(5f); // 🛡 bất tử 5 giây
        }

        Time.timeScale = 1f;
    }


    private void GoToGameOver()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOverScene");
    }
}
