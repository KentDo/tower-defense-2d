using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class QuizManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text txtQuestion;
    public Button[] answerButtons;
    public GameObject panelBackground;

    [Header("Scene References")]
    public string fallbackMap = "Map1";
    public string gameOverScene = "GameOverScene";

    private List<QuizQuestion> questions = new List<QuizQuestion>();
    private int correctIndex = -1;

    void Start()
    {
        LoadQuestions();

        GameSession.ResetQuestionsIfNeeded(questions.Count);

        ShowQuestion();
    }

    void LoadQuestions()
    {
        var jsonFile = Resources.Load<TextAsset>("questions");
        if (jsonFile == null)
        {
            Debug.LogError("Không tìm thấy Resources/questions.json!");
            return;
        }

        var wrapped = JsonUtility.FromJson<QuestionsWrapper>(jsonFile.text);
        questions = new List<QuizQuestion>(wrapped.questions);
    }

    void ShowQuestion()
    {
        int idx;
        do idx = Random.Range(0, questions.Count);
        while (GameSession.UsedQuestionIndices.Contains(idx));

        GameSession.UsedQuestionIndices.Add(idx);

        var q = questions[idx];
        txtQuestion.text = q.question;
        correctIndex = q.correctIndex;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int choice = i;
            var label = answerButtons[i].GetComponentInChildren<TMP_Text>();
            label.text = q.answers[i];
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => Answer(choice));
        }
    }

    void Answer(int chosenIndex)
    {
        if (chosenIndex == correctIndex)
        {
            // ✅ Trả lời đúng thì hồi sinh 1 mạng
            LevelManager.Instance.SetLives(1);

            Debug.Log("Đúng! Load lại map: " + GameSession.CurrentMap);
            SceneManager.LoadScene(GameSession.CurrentMap);
        }
        else
        {
            Debug.Log("Sai! Thua hẳn!");
            SceneManager.LoadScene(gameOverScene);
        }
    }

    [System.Serializable]
    class QuestionsWrapper
    {
        public QuizQuestion[] questions;
    }
}
