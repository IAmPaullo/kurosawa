using Gameplay.Core.Events;
using Gameplay.Views;
using UnityEngine;

public class ThemeMaterialController : MonoBehaviour
{
    [SerializeField] private Material skyboxMaterial;
    [SerializeField] private Material fakeFogMaterial;

    private static readonly int TopColorId = Shader.PropertyToID("_TopColor");
    private static readonly int BottomColorId = Shader.PropertyToID("_BottomColor");
    private static readonly int StarColorId = Shader.PropertyToID("_StarColor");

    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private EventBinding<ThemeUpdateEvent> themeUpdateBind;
    private void OnEnable()
    {
        themeUpdateBind = new(OnThemeUpdate);
        EventBus<ThemeUpdateEvent>.Register(themeUpdateBind);
    }

    private void OnDestroy()
    {
        EventBus<ThemeUpdateEvent>.Deregister(themeUpdateBind);
    }
    private void OnThemeUpdate(ThemeUpdateEvent evt)
    {
        ThemeSO theme = evt.Theme;

        Color topColor = theme.SkyTopColor;
        Color bottomColor = theme.SkyBottomColor;
        Color starColor = theme.StarsColor;
        Color fakeFogColor = theme.FakeFogColor;


        skyboxMaterial.SetColor(TopColorId, topColor);
        skyboxMaterial.SetColor(BottomColorId, bottomColor);
        skyboxMaterial.SetColor(StarColorId, starColor);

        Debug.LogWarning(skyboxMaterial.GetColor(StarColorId));
        fakeFogMaterial.SetColor(EmissionColorId, fakeFogColor);

    }

}
