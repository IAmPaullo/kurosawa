using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ButtonImproved : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Title("General")]
    [SerializeField] private Button button;                  // optional: para desabilitar efeitos quando !interactable
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool resetOnDisable = true;

    [Title("Timer")]
    [ToggleLeft] public bool enableTimer = false;
    [ShowIf(nameof(enableTimer))] public float timerUntilEnable = 1.75f;

    [Title("Hover Move")]
    [ToggleLeft] public bool moveOnHover = true;
    [ShowIf(nameof(moveOnHover))] public Vector2 moveOffset = new(6, -4);
    [ShowIf(nameof(moveOnHover))] public float moveDuration = 0.12f;
    [ShowIf(nameof(moveOnHover))] public Ease moveEase = Ease.OutQuad;

    [Title("Hover Scale")]
    [ToggleLeft] public bool scaleOnHover = true;
    [ShowIf(nameof(scaleOnHover))] public float hoverScale = 1.05f;
    [ShowIf(nameof(scaleOnHover))] public float scaleDuration = 0.12f;
    [ShowIf(nameof(scaleOnHover))] public Ease scaleEase = Ease.OutQuad;

    [Title("Hover Color")]
    [ToggleLeft] public bool colorOnHover = false;
    [ShowIf(nameof(colorOnHover))] public Graphic colorTarget;
    [ShowIf(nameof(colorOnHover))] public Color hoverColor = Color.white;
    [ShowIf(nameof(colorOnHover))] public float colorDuration = 0.12f;

    [Title("Hover Glow (child Image)")]
    [ToggleLeft] public bool glowOnHover = false;
    [ShowIf(nameof(glowOnHover))] public Image glowImage;
    [ShowIf(nameof(glowOnHover))][Range(0, 1)] public float glowAlpha = 0.35f;
    [ShowIf(nameof(glowOnHover))] public float glowDuration = 0.12f;

    [Title("Press Feedback")]
    [ToggleLeft] public bool pressScale = true;
    [ShowIf(nameof(pressScale))] public float pressedScale = 0.97f;
    [ShowIf(nameof(pressScale))] public float pressDuration = 0.08f;

    [Title("Sounds")]
    [ToggleLeft] public bool playHoverSound = false;
    [ShowIf(nameof(playHoverSound))] public AudioSource audioSource;
    [ShowIf(nameof(playHoverSound))] public AudioClip hoverEnterClip, hoverExitClip;
    [ToggleLeft] public bool playClickSound = false;
    [ShowIf(nameof(playClickSound))] public AudioClip clickClip;
    [ShowIf(nameof(playHoverSound))][Range(0, 1)] public float volume = 1f;

    // Internals
    RectTransform _rt;
    Vector2 _basePos;
    Vector3 _baseScale;
    Color _baseColor;
    bool _hovering;

    Tween _moveT, _scaleT, _colorT, _glowT;

    void Reset()
    {
        button = GetComponent<Button>();
        _rt = GetComponent<RectTransform>();
        colorTarget = colorTarget ? colorTarget : GetComponent<Graphic>();
        if (!audioSource && (playHoverSound || playClickSound))
        {
            audioSource = gameObject.GetComponent<AudioSource>();
        }
    }

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _basePos = _rt.anchoredPosition;
        _baseScale = transform.localScale;

        if (colorTarget)
            _baseColor = colorTarget.color;

        if (glowImage)
            SetGlowAlpha(0f);

        if (enableTimer)
        {
            button.interactable = false;
            DOVirtual.DelayedCall(timerUntilEnable, () => button.interactable = true);
        }
        else
            button.interactable = true;

    }

    bool Interactable() => button == null || button.interactable;

    public void OnPointerEnter(PointerEventData e)
    {
        if (!Interactable()) return;
        _hovering = true;

        if (moveOnHover)
        {
            _moveT?.Kill();
            _moveT = _rt.DOAnchorPos(_basePos + moveOffset, moveDuration)
                        .SetEase(moveEase).SetUpdate(useUnscaledTime);
        }
        if (scaleOnHover)
        {
            _scaleT?.Kill();
            _scaleT = transform.DOScale(_baseScale * hoverScale, scaleDuration)
                               .SetEase(scaleEase).SetUpdate(useUnscaledTime);
        }
        if (colorOnHover && colorTarget)
        {
            _colorT?.Kill();
            _colorT = colorTarget.DOColor(hoverColor, colorDuration).SetUpdate(useUnscaledTime);
        }
        if (glowOnHover && glowImage)
        {
            _glowT?.Kill();
            _glowT = glowImage.DOFade(glowAlpha, glowDuration).SetUpdate(useUnscaledTime);
        }
        if (playHoverSound && hoverEnterClip && audioSource)
            audioSource.PlayOneShot(hoverEnterClip, volume);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (!Interactable()) return;
        _hovering = false;

        if (moveOnHover)
        {
            _moveT?.Kill();
            _moveT = _rt.DOAnchorPos(_basePos, moveDuration)
                        .SetEase(moveEase).SetUpdate(useUnscaledTime);
        }
        if (scaleOnHover)
        {
            _scaleT?.Kill();
            _scaleT = transform.DOScale(_baseScale, scaleDuration)
                               .SetEase(scaleEase).SetUpdate(useUnscaledTime);
        }
        if (colorOnHover && colorTarget)
        {
            _colorT?.Kill();
            _colorT = colorTarget.DOColor(_baseColor, colorDuration).SetUpdate(useUnscaledTime);
        }
        if (glowOnHover && glowImage)
        {
            _glowT?.Kill();
            _glowT = glowImage.DOFade(0f, glowDuration).SetUpdate(useUnscaledTime);
        }
        if (playHoverSound && hoverExitClip && audioSource)
            audioSource.PlayOneShot(hoverExitClip, volume);
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (!Interactable()) return;
        if (pressScale)
        {
            _scaleT?.Kill();
            _scaleT = transform.DOScale(_baseScale * pressedScale, pressDuration)
                               .SetEase(Ease.OutQuad).SetUpdate(useUnscaledTime);
        }
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (!Interactable()) return;
        if (pressScale)
        {
            _scaleT?.Kill();
            float target = (_hovering && scaleOnHover) ? hoverScale : 1f;
            _scaleT = transform.DOScale(_baseScale * target, scaleDuration)
                               .SetEase(scaleEase).SetUpdate(useUnscaledTime);
        }
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (playClickSound && clickClip && audioSource)
            audioSource.PlayOneShot(clickClip, volume);
    }

    void OnDisable()
    {
        if (!resetOnDisable) return;

        _moveT?.Kill(); _scaleT?.Kill(); _colorT?.Kill(); _glowT?.Kill();
        _rt.anchoredPosition = _basePos;
        transform.localScale = _baseScale;

        if (colorTarget) colorTarget.color = _baseColor;
        if (glowImage) SetGlowAlpha(0f);

        _hovering = false;
    }

    void SetGlowAlpha(float a)
    {
        var c = glowImage.color; c.a = a; glowImage.color = c;
    }
}
