using Gameplay.VFX;
using UnityEngine;

namespace Gameplay.Core.Events
{
    public struct PlayVFXEvent : IEvent
    {
        public VFXType Type;
        public Vector3 Position;
    }
}