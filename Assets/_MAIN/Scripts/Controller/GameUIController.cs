using DG.Tweening;
using Gameplay.Boot.Events;
using Gameplay.Core.Events;
using Gameplay.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Core.Controllers
{
    public class GameUIController : MonoBehaviour
    {
        [SerializeField] private Canvas overlayUICanvas;
        [SerializeField] private Image background;
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField, BoxGroup("Groups")]
        private RectTransform topSideGroup;
        [SerializeField, BoxGroup("Groups")]
        private RectTransform rightSideGroup;
        [SerializeField, BoxGroup("Groups")]
        private RectTransform bottomSideGroup;

        [SerializeField, FoldoutGroup("Windows")]
        private UIWindowController themesWindow;
        [SerializeField, FoldoutGroup("Windows")]
        private UIWindowController meetMeWindow;

        [SerializeField, TabGroup("Buttons")]
        private Button playButton;
        [SerializeField, TabGroup("Buttons")]
        private Button settingsButton;
        [SerializeField, TabGroup("Buttons")]
        private Button returnToGameButton;
        [SerializeField, TabGroup("Buttons")]
        private Button pauseButton;
        [SerializeField, TabGroup("Buttons")]
        private Button rewardsButtons;
        [SerializeField, TabGroup("Buttons")]
        private Button meetMeButton;
        [SerializeField, TabGroup("Buttons")]
        private Button themesButton;

        [SerializeField, TabGroup("Labels")]
        private TextMeshProUGUI currentLevelText;

        [SerializeField, BoxGroup("End Game Panel")]
        private Transform endGameContainer;
        [SerializeField, BoxGroup("End Game Panel")]
        private Button nextLevelButton;
        [SerializeField, BoxGroup("End Game Panel")]
        private Button backToMenuButton;

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

        private EventBinding<MatchStartEvent> matchStartBind;
        private EventBinding<RequestPauseEvent> pauseRequestBind;
        private EventBinding<MatchEndEvent> matchEndBind;

        private Vector2 topTargetPos;
        private Vector2 rightTargetPos;
        private Vector2 bottomTargetPos;

        private Sequence uiSequence;

        private GameObject playButtonObj;
        private GameObject returnToGameButtonObj;
        private GameObject pauseButtonObj;
        private GameObject endGameContainerObj;

        private enum UIState
        {
            PreMatch,
            Playing,
            Paused,
            Ended
        }

        private UIState currentState;

        private void Awake()
        {
            playButtonObj = playButton ? playButton.gameObject : null;
            returnToGameButtonObj = returnToGameButton ? returnToGameButton.gameObject : null;
            pauseButtonObj = pauseButton ? pauseButton.gameObject : null;
            endGameContainerObj = endGameContainer ? endGameContainer.gameObject : null;

            SetupButtons();

            topTargetPos = GetAnchoredPos(topSideGroup);
            rightTargetPos = GetAnchoredPos(rightSideGroup);
            bottomTargetPos = GetAnchoredPos(bottomSideGroup);

            SetAnchoredPos(topSideGroup, topTargetPos + (Vector2.up * moveOffset));
            SetAnchoredPos(rightSideGroup, rightTargetPos + (Vector2.right * moveOffset));
            SetAnchoredPos(bottomSideGroup, bottomTargetPos + (Vector2.down * moveOffset));

            SetState(UIState.PreMatch, animateMenu: false);
        }

        private void OnEnable()
        {
            pauseRequestBind = new(OnPauseRequest);
            matchEndBind = new(OnMatchEnd);
            matchStartBind = new(OnMatchStart);

            EventBus<RequestPauseEvent>.Register(pauseRequestBind);
            EventBus<MatchEndEvent>.Register(matchEndBind);
            EventBus<MatchStartEvent>.Register(matchStartBind);
        }

        private void OnDisable()
        {
            EventBus<RequestPauseEvent>.Deregister(pauseRequestBind);
            EventBus<MatchEndEvent>.Deregister(matchEndBind);
            EventBus<MatchStartEvent>.Deregister(matchStartBind);
        }

        private void SetupButtons()
        {
            if (returnToGameButton)
            {
                returnToGameButton.onClick.RemoveAllListeners();
                returnToGameButton.onClick.AddListener(OnResumeClicked);
            }

            if (pauseButton)
            {
                pauseButton.onClick.RemoveAllListeners();
                pauseButton.onClick.AddListener(() => EventBus<RequestPauseEvent>.Raise(new()));
            }

            if (playButton)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(OnPlayClicked);
            }

            if (nextLevelButton)
            {
                nextLevelButton.onClick.RemoveAllListeners();
                nextLevelButton.onClick.AddListener(() => EventBus<RequestNextLevelEvent>.Raise(new()));
            }

            if (backToMenuButton)
            {
                backToMenuButton.onClick.RemoveAllListeners();
                backToMenuButton.onClick.AddListener(() =>
                {
                    if (SceneEvents.Instance != null)
                        SceneEvents.Instance.LoadMainMenuAsync();
                });
            }

            if (meetMeButton)
            {
                meetMeButton.onClick.RemoveAllListeners();
                meetMeButton.onClick.AddListener(() => meetMeWindow?.Open());
            }

            if (themesButton)
            {
                themesButton.onClick.RemoveAllListeners();
                themesButton.onClick.AddListener(() => themesWindow?.Open());
            }
        }

        private void OnPlayClicked()
        {
            EventBus<RequestMatchStartEvent>.Raise(new());
            SetState(UIState.Playing, animateMenu: false);
        }

        private void OnResumeClicked()
        {
            EventBus<RequestResumeEvent>.Raise(new());
            SetState(UIState.Playing, animateMenu: true);
        }

        private void OnMatchStart(MatchStartEvent _)
        {
            SetState(UIState.Playing, animateMenu: false);
        }

        private void OnPauseRequest(RequestPauseEvent _)
        {
            SetState(UIState.Paused, animateMenu: true);
        }

        private void OnMatchEnd(MatchEndEvent _)
        {
            SetState(UIState.Ended, animateMenu: true);
        }

        [Button]
        private void ApplyTheme(GradientSO theme)
        {
            CurrentTheme = theme;
            EventBus<ThemeUpdateEvent>.Raise(new() { Theme = CurrentTheme });
        }

        private void SetState(UIState state, bool animateMenu)
        {
            currentState = state;

            bool showPlay = state == UIState.PreMatch;
            bool showPause = state == UIState.Playing;
            bool showResume = state == UIState.Paused;
            bool showEndGame = state == UIState.Ended;

            if (playButtonObj) playButtonObj.SetActive(showPlay);
            if (pauseButtonObj) pauseButtonObj.SetActive(showPause);
            if (returnToGameButtonObj) returnToGameButtonObj.SetActive(showResume);
            if (endGameContainerObj) endGameContainerObj.SetActive(showEndGame);

            if (state != UIState.Paused)
                CloseAllWindows();

            if (animateMenu)
            {
                if (state == UIState.Playing || state == UIState.PreMatch)
                    AnimateOut();
                else
                    AnimateIn();
            }
            else
            {
                if (state == UIState.Playing || state == UIState.PreMatch)
                    SetMenuHidden();
                else
                    SetMenuVisible();
            }
        }

        private void CloseAllWindows()
        {
            themesWindow?.Close();
            meetMeWindow?.Close();
        }

        private void AnimateIn()
        {
            uiSequence?.Kill();
            uiSequence = DOTween.Sequence().SetUpdate(true);
            canvasGroup.interactable = true;

            uiSequence.Append(background.DOFade(.25f, .25f));
            uiSequence.Append(MoveContainer(topSideGroup, topTargetPos, animDuration, moveInEase));
            uiSequence.Append(MoveContainer(rightSideGroup, rightTargetPos, animDuration, moveInEase));
            uiSequence.Append(MoveContainer(bottomSideGroup, bottomTargetPos, animDuration, moveInEase));
        }

        private void AnimateOut()
        {
            uiSequence?.Kill();
            uiSequence = DOTween.Sequence().SetUpdate(true);
            canvasGroup.interactable = false;

            uiSequence.Join(background.DOFade(0, .25f));
            uiSequence.Join(MoveContainer(topSideGroup, topTargetPos + (Vector2.up * moveOffset), animDuration, moveOutEase));
            uiSequence.Join(MoveContainer(rightSideGroup, rightTargetPos + (Vector2.right * moveOffset), animDuration, moveOutEase));
            uiSequence.Join(MoveContainer(bottomSideGroup, bottomTargetPos + (2 * moveOffset * Vector2.down), animDuration, moveOutEase));
        }

        private void SetMenuVisible()
        {
            uiSequence?.Kill();
            canvasGroup.interactable = true;

            background.color = new Color(background.color.r, background.color.g, background.color.b, .25f);
            SetAnchoredPos(topSideGroup, topTargetPos);
            SetAnchoredPos(rightSideGroup, rightTargetPos);
            SetAnchoredPos(bottomSideGroup, bottomTargetPos);
        }

        private void SetMenuHidden()
        {
            uiSequence?.Kill();
            canvasGroup.interactable = false;

            background.color = new Color(background.color.r, background.color.g, background.color.b, 0f);
            SetAnchoredPos(topSideGroup, topTargetPos + (Vector2.up * moveOffset));
            SetAnchoredPos(rightSideGroup, rightTargetPos + (Vector2.right * moveOffset));
            SetAnchoredPos(bottomSideGroup, bottomTargetPos + (Vector2.down * moveOffset));
        }

        private Tween MoveContainer(Transform target, Vector2 targetPos, float duration, Ease ease)
        {
            RectTransform rt = target.GetComponent<RectTransform>();
            return rt.DOAnchorPos(targetPos, duration).SetEase(ease);
        }

        private Vector2 GetAnchoredPos(Transform t) => t.GetComponent<RectTransform>().anchoredPosition;
        private void SetAnchoredPos(Transform t, Vector2 pos) => t.GetComponent<RectTransform>().anchoredPosition = pos;
    }
}
