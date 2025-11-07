using UnityEngine;

public static class SaveSystem
{
    const string KEY_LAST_LEVEL = "last_level";

    public static void SetLastLevel(string levelName)
    {
        if (string.IsNullOrEmpty(levelName)) return;
        PlayerPrefs.SetString(KEY_LAST_LEVEL, levelName);
        PlayerPrefs.Save();
        // Debug.Log("[Save] last_level = " + levelName);
    }

    public static string GetLastLevel()
    {
        return PlayerPrefs.GetString(KEY_LAST_LEVEL, "");
    }

    public static void ClearLastLevel()
    {
        PlayerPrefs.DeleteKey(KEY_LAST_LEVEL);
    }
}
