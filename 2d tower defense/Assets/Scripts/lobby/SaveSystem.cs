using UnityEngine;

public static class SaveSystem
{
    private const string KEY_LAST_LEVEL = "last_level";
    private const string KEY_HAS_SAVE   = "has_save";

    public static bool HasSave => PlayerPrefs.GetInt(KEY_HAS_SAVE, 0) == 1;

    public static string GetLastLevel(string fallback = "Level1")
        => PlayerPrefs.GetString(KEY_LAST_LEVEL, fallback);

    public static void SetLastLevel(string levelName)
    {
        if (string.IsNullOrEmpty(levelName)) return;
        PlayerPrefs.SetString(KEY_LAST_LEVEL, levelName);
        PlayerPrefs.SetInt(KEY_HAS_SAVE, 1);
        PlayerPrefs.Save();
    }

    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(KEY_LAST_LEVEL);
        PlayerPrefs.DeleteKey(KEY_HAS_SAVE);
        PlayerPrefs.Save();
    }
}
