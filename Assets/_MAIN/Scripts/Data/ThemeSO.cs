using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "New UI Theme", menuName = "UI/Theme Data")]
public class ThemeSO : ScriptableObject
{
    [ShowInInspector, ReadOnly]
    public string uniqueID = System.Guid.NewGuid().ToString();
    [BoxGroup("Main Colors")]
    public Gradient MainGradient;
    [BoxGroup("Main Colors")]
    public Color TextColor = Color.white;
    [BoxGroup("Main Colors")]
    public Color ButtonColor = Color.white;
    [BoxGroup("Main Colors")]
    [ColorUsage(true, true)]
    public Color GlowColor = Color.white;

    [ColorUsage(true, true), BoxGroup("Skybox Colors")]
    public Color SkyTopColor = Color.white;
    [ColorUsage(true, true), BoxGroup("Skybox Colors")]
    public Color SkyBottomColor = Color.white;
    [ColorUsage(true, true), BoxGroup("Skybox Colors")]
    public Color StarsColor = Color.white;


    [ColorUsage(true, true), BoxGroup("Fog Colors")]
    public Color FakeFogColor = Color.white;


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
