using Gameplay.Core;
using Sirenix.OdinInspector;
using UnityEngine;
[CreateAssetMenu(fileName = "Piece", menuName = "Piece/New Piece Data")]
public class PieceSO : ScriptableObject
{

    [Title("Data")]
    public string ID = "LineStraight_00";
    [ShowInInspector] public PieceType Type { get; private set; } = PieceType.Wire;
    [ShowInInspector, EnumToggleButtons] public Direction BaseConnections { get; private set; } = Direction.Up | Direction.Down;

    [Title("Visuals")]
    [PreviewField] public Sprite Icon;

}