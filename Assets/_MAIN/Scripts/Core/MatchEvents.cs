namespace Gameplay.Core.Events
{
    public struct MatchPrepareEvent : IEvent
    {
        public Data.LevelDataSO LevelData;
    }

    // Comando para iniciar de fato (Animar reveal, liberar input, dar zoom)
    public struct MatchStartEvent : IEvent { }
    public struct MatchEndEvent : IEvent { }


    public struct LevelCompletedEvent : IEvent { }
}
