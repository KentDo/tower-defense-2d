using System.Collections.Generic;

public static class GameSession
{
    public static readonly HashSet<int> UsedQuestionIndices = new HashSet<int>();

    private static string lastGameplayScene;

    public static void ResetQuestionsIfNeeded(int total)
    {
        if (UsedQuestionIndices.Count >= total)
        {
            UsedQuestionIndices.Clear();
        }
    }

    public static void ClearAll()
    {
        UsedQuestionIndices.Clear();
    }

    public static void SetLastGameplayScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            lastGameplayScene = sceneName;
        }
    }

    public static bool TryGetLastGameplayScene(out string sceneName)
    {
        sceneName = lastGameplayScene;
        return !string.IsNullOrEmpty(sceneName);
    }
}
