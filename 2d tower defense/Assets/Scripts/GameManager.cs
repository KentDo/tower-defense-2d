using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject quizPrefab; // kéo PanelQuiz prefab vào đây

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Instantiate QuizManager nếu chưa có
            if (FindObjectOfType<QuizManager>() == null && quizPrefab != null)
            {
                GameObject quizObj = Instantiate(quizPrefab);
                quizObj.name = "PanelQuiz";
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
