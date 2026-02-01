using Gameplay.Core.Data;
using Sirenix.OdinInspector;
using System;

namespace Gameplay.Core
{
    [Serializable]
    public class NodeModel
    {

        [ShowInInspector, ReadOnly] public int X { get; private set; }
        [ShowInInspector, ReadOnly] public int Y { get; private set; }
        [ShowInInspector, ReadOnly, InfoBox("0=0° 1=90° 2=180° 3=270°")]
        public int RotationIndex { get; private set; }
        [ShowInInspector, ReadOnly] public bool IsPowered { get; set; }
        [ShowInInspector, ReadOnly] public PieceType PieceType => pieceDefinition.PieceType;

        private readonly PieceSO pieceDefinition;

        public NodeModel(int x, int y, PieceSO pieceDefinition, int initialRotation = 0)
        {
            X = x;
            Y = y;
            this.pieceDefinition = pieceDefinition;
            RotationIndex = initialRotation;
        }


        public Direction GetCurrentConnections()
        {
            Direction current = Direction.None;
            Direction baseDir = pieceDefinition.BaseConnections;

            if ((baseDir & Direction.Up) != 0) current |= RotateDirection(Direction.Up);
            if ((baseDir & Direction.Right) != 0) current |= RotateDirection(Direction.Right);
            if ((baseDir & Direction.Down) != 0) current |= RotateDirection(Direction.Down);
            if ((baseDir & Direction.Left) != 0) current |= RotateDirection(Direction.Left);

            return current;
        }

        public void Rotate()
        {
            RotationIndex = (RotationIndex + 1) % 4;
        }

        private Direction RotateDirection(Direction dir)
        {
            int directionIndex = (int)dir;
            for (int i = 0; i < RotationIndex; i++)
            {
                directionIndex = directionIndex << 1;

                if (directionIndex > 8) directionIndex = 1;
            }
            return (Direction)directionIndex;
        }
    }
}
