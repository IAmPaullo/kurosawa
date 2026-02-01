namespace Gameplay.Core.Events
{
    public struct RequestPauseEvent : IEvent { }
    public struct RequestNextLevelEvent : IEvent { }

    public struct RequestRestartEvent : IEvent { }

    public struct ThemeUpdateEvent : IEvent
    {
        public GradientSO Theme;
    }
}
