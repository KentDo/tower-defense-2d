using System.Collections.Generic;

public static class GameSession
{
    // Lưu các chỉ mục câu đã dùng để tránh lặp
    public static HashSet<int> UsedQuestionIndices = new HashSet<int>();

    // Reset khi đã dùng hết
    public static void ResetQuestionsIfNeeded(int total)
    {
        if (UsedQuestionIndices.Count >= total)
            UsedQuestionIndices.Clear();
    }

    public static void ClearAll()
    {
        UsedQuestionIndices.Clear();
    }
}
