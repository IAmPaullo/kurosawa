using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Core.Data
{
    [CreateAssetMenu(fileName = "Piece", menuName = "Piece/New Piece Data")]
    public class PieceSO : ScriptableObject
    {
        [Title("Data")]
        public string ID = "LineStraight_00";

        [SerializeField] private PieceType pieceType = PieceType.Wire;
        public PieceType PieceType => pieceType;

        [SerializeField, EnumToggleButtons]
        private Direction baseConnections = Direction.Up | Direction.Down;

        public Direction BaseConnections => baseConnections;

        [Title("Visuals")]
        [PreviewField] public Sprite Icon;
    }
}