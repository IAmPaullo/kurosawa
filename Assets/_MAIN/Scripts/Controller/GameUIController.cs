using DG.Tweening;
using Gameplay.Core.Controllers;
using Gameplay.Core.Events;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private Canvas overlayUICanvas;
    [SerializeField] private Image background;
    [SerializeField] private CanvasGroup canvasGroup;
    private Transform canvasTransform;


    [SerializeField, BoxGroup("Groups")]
    private Transform topSideGroup;
    [SerializeField, BoxGroup("Groups")]
    private Transform rightSideGroup;
    [SerializeField, BoxGroup("Groups")]
    private Transform bottomSideGroup;

    [SerializeField, TabGroup("Buttons")]
    private Button settingsButton;
    [SerializeField, TabGroup("Buttons")]
    private Button returnToGameButton;
    [SerializeField, TabGroup("Buttons")]
    private Button pauseButton;
    [SerializeField, TabGroup("Buttons")]
    private Button rewardsButtons;

    [SerializeField, TabGroup("Labels")]
    private TextMeshProUGUI currentLevelText;


    [SerializeField, TabGroup("Player Progress UI")]
    private TextMeshProUGUI currentExperienceText;
    [SerializeField, TabGroup("Player Progress UI")]
    private TextMeshProUGUI targetExperienceText;

    [SerializeField, BoxGroup("Gradients")]
    private List<GradientSO> availableThemes;
    [ReadOnly, ShowInInspector, BoxGroup("Gradients")]
    public GradientSO CurrentTheme { get; private set; }


    [SerializeField, BoxGroup("Animation")]
    private float moveOffset = 500f;
    [SerializeField, BoxGroup("Animation")]
    private float animDuration = 0.5f;
    [SerializeField, BoxGroup("Animation")]
    private Ease moveInEase = Ease.Linear;
    [SerializeField, BoxGroup("Animation")]
    private Ease moveOutEase = Ease.Linear;

    private EventBinding<RequestPauseEvent> pauseRequestBind;
    private EventBinding<MatchEndEvent> matchEndBind;


    private Vector2 topTargetPos;
    private Vector2 rightTargetPos;
    private Vector2 bottomTargetPos;
    private Sequence uiSequence;

    private void Awake()
    {
        canvasTransform = overlayUICanvas.transform;
        SetupButtons();


        topTargetPos = GetAnchoredPos(topSideGroup);
        rightTargetPos = GetAnchoredPos(rightSideGroup);
        bottomTargetPos = GetAnchoredPos(bottomSideGroup);

        SetAnchoredPos(topSideGroup, topTargetPos + (Vector2.up * moveOffset));    // Sobe
        SetAnchoredPos(rightSideGroup, rightTargetPos + (Vector2.right * moveOffset)); // Vai pra direita
        SetAnchoredPos(bottomSideGroup, bottomTargetPos + (Vector2.down * moveOffset)); // Desce
    }

    private void OnEnable()
    {
        pauseRequestBind = new(OnPauseRequest);
        matchEndBind = new(OnMatchEnd);

        EventBus<RequestPauseEvent>.Register(pauseRequestBind);
        EventBus<MatchEndEvent>.Register(matchEndBind);
    }

    private void OnDisable()
    {
        EventBus<RequestPauseEvent>.Deregister(pauseRequestBind);
        EventBus<MatchEndEvent>.Deregister(matchEndBind);
    }

    public void SetupButtons()
    {
        returnToGameButton.onClick.AddListener(() => AnimateOut());
        pauseButton.onClick.AddListener(() => EventBus<RequestPauseEvent>.Raise(new()));

    }

    private void OnPauseRequest(RequestPauseEvent evt)
    {
        AnimateIn();

        if (availableThemes != null && availableThemes.Count > 0)
        {
            ApplyTheme(availableThemes[Random.Range(0, availableThemes.Count)]);
        }
    }

    private void OnMatchEnd(MatchEndEvent evt)
    {
        AnimateOut();
    }
    [Button]
    private void ApplyTheme(GradientSO theme)
    {
        CurrentTheme = theme;
        // Debug.Log("Theme Updated");
        EventBus<ThemeUpdateEvent>.Raise(new() { Theme = CurrentTheme });
    }


    [Button]
    private void AnimateIn()
    {
        uiSequence?.Kill();
        uiSequence = DOTween.Sequence();
        canvasGroup.interactable = true;

        uiSequence.Append(background.DOFade(.25f, .25f));
        uiSequence.Append(MoveContainer(topSideGroup, topTargetPos, animDuration, moveInEase));
        uiSequence.Append(MoveContainer(rightSideGroup, rightTargetPos, animDuration, moveInEase));
        uiSequence.Append(MoveContainer(bottomSideGroup, bottomTargetPos, animDuration, moveInEase));


        uiSequence.SetUpdate(true);
    }
    private Tween MoveContainer(Transform target, Vector2 targetPos, float duration, Ease ease)
    {
        Tween moveTween;
        RectTransform rt = target.GetComponent<RectTransform>();
        moveTween = rt.DOAnchorPos(targetPos, duration).SetEase(ease);
        return moveTween;
    }

    [Button]
    private void AnimateOut()
    {
        uiSequence?.Kill();
        uiSequence = DOTween.Sequence();
        canvasGroup.interactable = false;

        uiSequence.Join(background.DOFade(0, .25f));
        uiSequence.Join(topSideGroup.GetComponent<RectTransform>().DOAnchorPos(topTargetPos + (Vector2.up * moveOffset), animDuration)
            .SetEase(moveOutEase));
        uiSequence.Join(rightSideGroup.GetComponent<RectTransform>().DOAnchorPos(rightTargetPos + (Vector2.right * moveOffset), animDuration)
            .SetEase(moveOutEase));
        uiSequence.Join(bottomSideGroup.GetComponent<RectTransform>().DOAnchorPos(bottomTargetPos + (Vector2.down * moveOffset), animDuration)
            .SetEase(moveOutEase));

        uiSequence.SetUpdate(true);
    }


    private Vector2 GetAnchoredPos(Transform t) => t.GetComponent<RectTransform>().anchoredPosition;
    private void SetAnchoredPos(Transform t, Vector2 pos) => t.GetComponent<RectTransform>().anchoredPosition = pos;
}