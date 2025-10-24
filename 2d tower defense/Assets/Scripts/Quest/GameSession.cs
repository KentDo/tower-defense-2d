public static class GameSession
{
    public static string CurrentMap = "Map1";
    public static System.Collections.Generic.List<int> UsedQuestionIndices = new System.Collections.Generic.List<int>();

    public static void ResetQuestionsIfNeeded(int total)
    {
        if (UsedQuestionIndices.Count >= total)
            UsedQuestionIndices.Clear();
    }
}
