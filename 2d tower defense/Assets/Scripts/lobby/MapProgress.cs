using UnityEngine;

public static class MapProgress
{
    // Keys
    private static string K_Unlock(string scene) => $"unlock_{scene}";
    private static string K_Stars(string scene) => $"stars_{scene}";

    // Mặc định Level1 mở sẵn
    public static bool IsUnlocked(string scene)
    {
        if (scene == "Level1") return true;
        return PlayerPrefs.GetInt(K_Unlock(scene), 0) == 1;
    }

    public static void Unlock(string scene, bool on = true)
    {
        PlayerPrefs.SetInt(K_Unlock(scene), on ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static int GetStars(string scene) => Mathf.Clamp(PlayerPrefs.GetInt(K_Stars(scene), 0), 0, 3);

    public static void SetStars(string scene, int stars)
    {
        PlayerPrefs.SetInt(K_Stars(scene), Mathf.Clamp(stars, 0, 3));
        PlayerPrefs.Save();
    }

    // Gọi khi thắng màn: set sao cho màn hiện tại, và mở khóa màn kế tiếp (nếu có)
    public static void CompleteLevelAndUnlockNext(string currentScene, int starsEarned, string nextSceneIfAny = null)
    {
        SetStars(currentScene, Mathf.Max(GetStars(currentScene), starsEarned));
        if (!string.IsNullOrEmpty(nextSceneIfAny))
            Unlock(nextSceneIfAny, true);
    }
}
