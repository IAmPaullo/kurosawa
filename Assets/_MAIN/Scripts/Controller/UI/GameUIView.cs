using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Gameplay.UI
{
    public class GameUIView : MonoBehaviour
    {
        [Header("Canvas & Background")]
        [SerializeField] private Canvas overlayUICanvas;
        [SerializeField] private Image background;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Layout Groups")]
        [SerializeField] private RectTransform topSideGroup;
        [SerializeField] private RectTransform rightSideGroup;
        [SerializeField] private RectTransform bottomSideGroup;

        [Header("Windows")]
        [SerializeField] private UIWindowController themesWindow;
        [SerializeField] private UIWindowController meetMeWindow;
        [SerializeField] private UIWindowController settingsWindow;

        [Header("Buttons")]
        [SerializeField] public Button PlayButton;
        [SerializeField] public Button SettingsButton;
        [SerializeField] public Button ReturnToGameButton;
        [SerializeField] public Button PauseButton;
        [SerializeField] public Button RewardsButtons;
        [SerializeField] public Button MeetMeButton;
        [SerializeField] public Button ThemesButton;
        [SerializeField] public Button NextLevelButton;
        [SerializeField] public Button BackToMenuButton;

        [Header("End Game")]
        [SerializeField] private GameObject endGameContainer;

        [Header("Animation Settings")]
        [SerializeField] private float moveOffset = 500f;
        [SerializeField] private float animDuration = 0.5f;
        [SerializeField] private Ease moveInEase = Ease.Linear;
        [SerializeField] private Ease moveOutEase = Ease.Linear;

        private Vector2 topTargetPos;
        private Vector2 rightTargetPos;
        private Vector2 bottomTargetPos;
        private Sequence uiSequence;


        public GameObject PlayButtonObj => PlayButton ? PlayButton.gameObject : null;
        public GameObject PauseButtonObj => PauseButton ? PauseButton.gameObject : null;
        public GameObject ReturnToGameButtonObj => ReturnToGameButton ? ReturnToGameButton.gameObject : null;
        public GameObject EndGameContainerObj => endGameContainer;

        public void Initialize()
        {
            // Salva posições iniciais
            topTargetPos = GetAnchoredPos(topSideGroup);
            rightTargetPos = GetAnchoredPos(rightSideGroup);
            bottomTargetPos = GetAnchoredPos(bottomSideGroup);

            // Define posições iniciais (fora da tela ou onde o layout mandar)
            SetAnchoredPos(topSideGroup, topTargetPos + (Vector2.up * moveOffset));
            SetAnchoredPos(rightSideGroup, rightTargetPos + (Vector2.right * moveOffset));
            SetAnchoredPos(bottomSideGroup, bottomTargetPos + (Vector2.down * moveOffset));
        }

        public void UpdateStateVisuals(GameUIPresenter.UIState state, bool animate)
        {
            bool showPlay = state == GameUIPresenter.UIState.PreMatch;
            bool showPause = state == GameUIPresenter.UIState.Playing;
            bool showResume = state == GameUIPresenter.UIState.Paused;
            bool showEndGame = state == GameUIPresenter.UIState.Ended;

            if (PlayButtonObj) PlayButtonObj.SetActive(showPlay);
            if (PauseButtonObj) PauseButtonObj.SetActive(showPause);
            if (ReturnToGameButtonObj) ReturnToGameButtonObj.SetActive(showResume);
            if (EndGameContainerObj) EndGameContainerObj.SetActive(showEndGame);


            if (state != GameUIPresenter.UIState.Paused)
            {
                CloseAllWindows();
            }

            if (animate)
            {
                if (state == GameUIPresenter.UIState.Playing || state == GameUIPresenter.UIState.PreMatch)
                    AnimateOut();
                else
                    AnimateIn(); 
            }
            else
            {
                if (state == GameUIPresenter.UIState.Playing || state == GameUIPresenter.UIState.PreMatch)
                    SetMenuHidden();
                else
                    SetMenuVisible();
            }
        }

        public void OpenThemesWindow() => themesWindow?.Open();
        public void OpenMeetMeWindow() => meetMeWindow?.Open();
        public void OpenSettingsWindow() => settingsWindow?.Open();

        private void CloseAllWindows()
        {
            themesWindow?.Close();
            meetMeWindow?.Close();
            settingsWindow?.Close();
        }



        private void AnimateIn()
        {
            uiSequence?.Kill();
            uiSequence = DOTween.Sequence().SetUpdate(true);
            canvasGroup.interactable = true;

            uiSequence.Append(background.DOFade(.25f, .25f));
            uiSequence.Append(MoveContainer(topSideGroup, topTargetPos, animDuration, moveInEase));
            uiSequence.Join(MoveContainer(rightSideGroup, rightTargetPos, animDuration, moveInEase)); 
            uiSequence.Join(MoveContainer(bottomSideGroup, bottomTargetPos, animDuration, moveInEase));
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