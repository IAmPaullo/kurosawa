using UnityEngine;

namespace Gameplay.Core
{
    public static class ThemePrefs
    {
        const string LastThemeKey = "UiThemeLastId";

        public static void SaveLastThemeId(string themeId)
        {
            PlayerPrefs.SetString(LastThemeKey, themeId);
            PlayerPrefs.Save();
        }
        public static string LoadLastThemeId()
        {
            return PlayerPrefs.GetString(LastThemeKey, string.Empty);
        }
    }
}