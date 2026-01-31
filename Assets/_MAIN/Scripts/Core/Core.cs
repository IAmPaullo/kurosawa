using System;

namespace Gameplay.Core
{

    [Flags]
    public enum Direction
    {
        None = 0,
        Up = 1,
        Right = 2,
        Down = 4,
        Left = 8
    }
    public enum PieceType
    {
        Wire, //connection
        Source, // energy
        Lamp, //target
    }
}


namespace Gameplay.Core.Events
{
    public struct LevelCompletedEvent : IEvent { }
}