namespace Gameplay.UI
{
    using Gameplay.Core.Events;
    using Gameplay.UI;
    using Sirenix.OdinInspector;
    using System.Collections.Generic;
    using UnityEngine;

    public class GameUIPresenter : MonoBehaviour
    {
        public enum UIState
        {
            PreMatch,
            Playing,
            Paused,
            Ended
        }

        [SerializeField, Required] private GameUIView view;

        [SerializeField] private List<ThemeSO> availableThemes;
        public ThemeSO CurrentTheme { get; private set; }

        private EventBinding<MatchStartEvent> matchStartBind;
        private EventBinding<RequestPauseEvent> pauseRequestBind;
        private EventBinding<MatchEndEvent> matchEndBind;

        private UIState currentState;

        private void Awake()
        {
            view.Initialize();
            SetupButtonListeners();
            SetState(UIState.PreMatch, false);
        }

        private void OnEnable()
        {
            pauseRequestBind = new EventBinding<RequestPauseEvent>(OnPauseRequest);
            matchEndBind = new EventBinding<MatchEndEvent>(OnMatchEnd);
            matchStartBind = new EventBinding<MatchStartEvent>(OnMatchStart);

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

        private void SetupButtonListeners()
        {
            if (view.ReturnToGameButton)
            {
                view.ReturnToGameButton.onClick.RemoveAllListeners();
                view.ReturnToGameButton.onClick.AddListener(OnResumeClicked);
            }

            if (view.PauseButton)
            {
                view.PauseButton.onClick.RemoveAllListeners();
                view.PauseButton.onClick.AddListener(() => EventBus<RequestPauseEvent>.Raise(new RequestPauseEvent()));
            }

            if (view.PlayButton)
            {
                view.PlayButton.onClick.RemoveAllListeners();
                view.PlayButton.onClick.AddListener(OnPlayClicked);
            }

            if (view.NextLevelButton)
            {
                view.NextLevelButton.onClick.RemoveAllListeners();
                view.NextLevelButton.onClick.AddListener(() => EventBus<RequestNextLevelEvent>.Raise(new RequestNextLevelEvent()));
            }

            if (view.BackToMenuButton)
            {
                view.BackToMenuButton.onClick.RemoveAllListeners();
                view.BackToMenuButton.onClick.AddListener(() =>
                {
                    if (SceneEvents.Instance != null)
                        SceneEvents.Instance.LoadMainMenuAsync();
                });
            }

            if (view.MeetMeButton)
            {
                view.MeetMeButton.onClick.RemoveAllListeners();
                view.MeetMeButton.onClick.AddListener(() => view.OpenMeetMeWindow());
            }

            if (view.ThemesButton)
            {
                view.ThemesButton.onClick.RemoveAllListeners();
                view.ThemesButton.onClick.AddListener(() => view.OpenThemesWindow());
            }
        }

        private void OnPlayClicked()
        {
            EventBus<RequestMatchStartEvent>.Raise(new RequestMatchStartEvent());
            SetState(UIState.Playing, false);
        }

        private void OnResumeClicked()
        {
            EventBus<RequestResumeEvent>.Raise(new RequestResumeEvent());
            SetState(UIState.Playing, true);
        }

        private void OnMatchStart(MatchStartEvent evt)
        {
            SetState(UIState.Playing, false);
        }

        private void OnPauseRequest(RequestPauseEvent evt)
        {
            SetState(UIState.Paused, true);
        }

        private void OnMatchEnd(MatchEndEvent evt)
        {
            SetState(UIState.Ended, true);
        }


        private void SetState(UIState state, bool animate)
        {
            currentState = state;
            view.UpdateStateVisuals(state, animate);
        }
    }
}
