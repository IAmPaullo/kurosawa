using Gameplay.Boot.Events;
using Gameplay.Core.Events;
using Sirenix.OdinInspector;
using UnityEngine;

public class ThemeController : MonoBehaviour
{
    const string LastThemeKey = "UiThemeLastName";

    [SerializeField] private ThemeRuntimeSO runtimeTheme;
    [SerializeField] private ThemeLibrarySO themeLibrary;

    public void Setup()
    {
        ApplySavedThemeOrDefault();
    }

    public void SelectTheme(ThemeSO theme)
    {
        if (!theme) return;
        if (runtimeTheme == null) return;

        runtimeTheme.CurrentTheme = theme;

        PlayerPrefs.SetString(LastThemeKey, theme.name);
        PlayerPrefs.Save();

        EventBus<ThemeUpdateEvent>.Raise(new() { Theme = theme });
    }

    private void ApplySavedThemeOrDefault()
    {
        if (!themeLibrary || themeLibrary.Entries.Count == 0) return;

        string savedName = PlayerPrefs.GetString(LastThemeKey, string.Empty);

        ThemeSO chosenTheme = null;

        if (!string.IsNullOrEmpty(savedName))
        {
            for (int i = 0; i < themeLibrary.Entries.Count; i++)
            {
                var t = themeLibrary.Entries[i];
                if (t && t.name == savedName)
                {
                    chosenTheme = t;
                    break;
                }
            }
        }

        if (!chosenTheme)
            chosenTheme = themeLibrary.Entries[0];

        runtimeTheme.CurrentTheme = chosenTheme;
        EventBus<ThemeUpdateEvent>.Raise(new() { Theme = chosenTheme });
    }


    private EventBinding<RequestThemeEvent> themeRequestBind;
    private EventBinding<MainMenuStartEvent> mainMenuStartBind;
    private void OnEnable()
    {
        themeRequestBind = new(OnRequestTheme);
        mainMenuStartBind = new(() => Setup());
        EventBus<RequestThemeEvent>.Register(themeRequestBind);
        EventBus<MainMenuStartEvent>.Register(mainMenuStartBind);
    }
    private void OnDestroy()
    {
        EventBus<RequestThemeEvent>.Deregister(themeRequestBind);
        EventBus<MainMenuStartEvent>.Deregister(mainMenuStartBind);
    }

    private void OnRequestTheme(RequestThemeEvent _)
    {
        if (runtimeTheme == null) return;

        EventBus<ThemeUpdateEvent>.Raise(new() { Theme = runtimeTheme.CurrentTheme });
    }
}