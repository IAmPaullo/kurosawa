using Gameplay.Core.Events;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThemeColorApplier : MonoBehaviour
{
    public enum ColorSource
    {
        TextColor,
        ButtonColor,
        GradientSample
    }
    [SerializeField, Required] private ThemeRuntimeSO runtime;
    [Title("Target")]
    [SerializeField] private Image image;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;

    [Header("Theme Mapping")]
    [SerializeField] private ColorSource colorSource = ColorSource.GradientSample;

    [Range(0f, 1f)]
    [SerializeField] private float gradientTime = 0.5f;

    [Header("Alpha")]
    [SerializeField] private bool preserveCurrentAlpha = true;
    [Range(0f, 1f)]
    [SerializeField] private float forcedAlpha = 1f;

    EventBinding<ThemeUpdateEvent> themeUpdateBinding;

    void Reset()
    {
        TryGetComponent(out image);
        TryGetComponent(out spriteRenderer);
    }

    void Awake()
    {
        if (image == null) TryGetComponent(out image);
        if (spriteRenderer == null) TryGetComponent(out spriteRenderer);
        if (textMeshProUGUI == null) TryGetComponent(out textMeshProUGUI);

        themeUpdateBinding = new(OnThemeUpdate);
    }

    void OnEnable()
    {
        EventBus<ThemeUpdateEvent>.Register(themeUpdateBinding);

        if (runtime != null && runtime.CurrentTheme != null)
            ApplyTheme(runtime.CurrentTheme);
    }
    private void OnDestroy()
    {
        EventBus<ThemeUpdateEvent>.Deregister(themeUpdateBinding);
    }

    public void OnThemeUpdate(ThemeUpdateEvent evt)
    {
        ApplyTheme(evt.Theme);
    }

    public void ApplyTheme(ThemeSO theme)
    {
        if (theme == null) return;

        Color newColor = ResolveColor(theme);

        if (image != null)
        {
            float alpha = preserveCurrentAlpha ? image.color.a : forcedAlpha;
            newColor.a = alpha;
            image.color = newColor;
        }

        if (spriteRenderer != null)
        {
            float alpha = preserveCurrentAlpha ? spriteRenderer.color.a : forcedAlpha;
            newColor.a = alpha;
            spriteRenderer.color = newColor;
        }
        if (textMeshProUGUI != null)
        {
            float alpha = preserveCurrentAlpha ? textMeshProUGUI.color.a : forcedAlpha;
            newColor.a = alpha;
            textMeshProUGUI.color = newColor;
        }
    }

    Color ResolveColor(ThemeSO theme)
    {
        switch (colorSource)
        {
            case ColorSource.TextColor:
                return theme.TextColor;

            case ColorSource.ButtonColor:
                return theme.ButtonColor;

            case ColorSource.GradientSample:
                if (theme.MainGradient == null) return Color.white;
                return theme.MainGradient.Evaluate(Mathf.Clamp01(gradientTime));

            default:
                return Color.white;
        }
    }
}
