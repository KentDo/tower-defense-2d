using System.Collections.Generic;

[System.Serializable]
public class QuizQuestion
{
    public string question;
    public List<string> answers;  // 4 đáp án
    public int correctIndex;      // 0..3
}

// Json wrapper để Resources.Load<TextAsset> dễ parse
[System.Serializable]
public class QuizQuestionList
{
    public List<QuizQuestion> questions;
}
