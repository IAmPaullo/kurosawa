using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "New UI Theme", menuName = "UI/Theme Data")]
public class GradientSO : ScriptableObject
{
    [Title("Colors")]
    public Gradient MainGradient;
    public Color TextColor = Color.white;
    public Color ButtonColor = Color.white;
    [ColorUsage(true, true)]
    public Color GlowColor = Color.white;


    [ShowInInspector, ReadOnly, BoxGroup("Gradient Preview")]
    public Color TopColor => GetTopColor();
    [SerializeField, Range(0, 1), BoxGroup("Gradient Preview")]
    private float topColorThreshold = 0f;
    [ShowInInspector, ReadOnly, BoxGroup("Gradient Preview")]
    public Color BottomColor => GetBottomColor();
    [SerializeField, Range(0, 1), BoxGroup("Gradient Preview")]
    private float bottomColorThreshold = 1f;

    [Title("Settings")]
    public bool UseDarkText = false;


    Color GetTopColor()
    {
        if (MainGradient == null)
            return Color.black;
        return MainGradient.Evaluate(topColorThreshold);
    }
    Color GetBottomColor()
    {
        if (MainGradient == null)
            return Color.black;
        return MainGradient.Evaluate(bottomColorThreshold);
    }
}
