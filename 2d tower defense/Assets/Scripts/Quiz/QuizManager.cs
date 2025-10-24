using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuizManager : MonoBehaviour
{
    public GameObject panelQuiz;
    public TMP_Text txtQuestion;
    public Button[] answerButtons;

    private List<QuizQuestion> questions = new List<QuizQuestion>();
    private int correctIndex = -1;

    void Awake()
    {
        LoadQuestions();
        panelQuiz.SetActive(false);
    }

    public void ShowQuiz()
    {
        if (questions.Count == 0) return;

        panelQuiz.SetActive(true);
        Time.timeScale = 0f; // pause gameplay
        ShowQuestion();
    }

    void LoadQuestions()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("questions");
        if (jsonFile == null)
        {
            Debug.LogError("Không tìm thấy Resources/questions.json!");
            return;
        }

        QuestionsWrapper wrapper = JsonUtility.FromJson<QuestionsWrapper>(jsonFile.text);
        questions = new List<QuizQuestion>(wrapper.questions);
    }

    void ShowQuestion()
    {
        int idx;
        do idx = Random.Range(0, questions.Count);
        while (GameSession.UsedQuestionIndices.Contains(idx));

        GameSession.UsedQuestionIndices.Add(idx);

        QuizQuestion q = questions[idx];
        txtQuestion.text = q.question;
        correctIndex = q.correctIndex;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int choice = i;
            TMP_Text label = answerButtons[i].GetComponentInChildren<TMP_Text>();
            label.text = q.answers[i];

            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => Answer(choice));
        }
    }

    void Answer(int chosenIndex)
    {
        panelQuiz.SetActive(false);
        Time.timeScale = 1f; // resume gameplay

        if (chosenIndex == correctIndex)
        {
            if (LevelManager.Instance != null)
                LevelManager.Instance.SetLives(1); // hồi 1 máu
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
        }
    }

    [System.Serializable]
    private class QuestionsWrapper
    {
        public QuizQuestion[] questions;
    }
}
