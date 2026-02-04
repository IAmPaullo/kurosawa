namespace Gameplay.Core.Events
{
    public struct RequestMatchStartEvent : IEvent { }
    public struct RequestPauseEvent : IEvent { }
    public struct RequestResumeEvent : IEvent { }
    public struct RequestNextLevelEvent : IEvent { }

    public struct RequestRestartEvent : IEvent { }

    public struct ThemeUpdateEvent : IEvent
    {
        public ThemeSO Theme;
    }
    public struct RequestThemeEvent : IEvent { }
}
