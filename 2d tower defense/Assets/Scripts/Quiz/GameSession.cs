using System.Collections.Generic;

public static class GameSession
{
    public static List<int> UsedQuestionIndices = new List<int>();

    public static void ResetQuestionsIfNeeded(int total)
    {
        if (UsedQuestionIndices.Count >= total)
            UsedQuestionIndices.Clear();
    }
}
