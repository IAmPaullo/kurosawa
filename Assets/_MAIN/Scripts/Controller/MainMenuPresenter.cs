namespace Gameplay.UI
{
    using Gameplay.Boot.Events;
    using Gameplay.Core.Events;
    using Gameplay.Managers;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class MainMenuPresenter : MonoBehaviour
    {
        [Title("References")]
        [SerializeField, Required] private MainMenuView view;

        [SerializeField] private SaveManager saveManager;

        [Title("Config")]
        [SerializeField] private string gameplaySceneName = "Game";

        private void Awake()
        {
            //if (!saveManager)
            //    saveManager = FindFirstObjectByType<SaveManager>();

            //view.Initialize();
            //SetupButtonListeners();
            //RefreshVisuals();
        }

        private void Start()
        {
            if (!saveManager)
                saveManager = FindFirstObjectByType<SaveManager>();

            view.Initialize();
            SetupButtonListeners();
            RefreshVisuals();
            EventBus<MainMenuStartEvent>.Raise(new MainMenuStartEvent());
        }

        private void SetupButtonListeners()
        {
            view.BindPlay(OnPlayClicked);
            view.BindOpenLevelSelect(OnOpenLevelSelectClicked);
            view.BindCloseLevelSelect(OnCloseLevelSelectClicked);
            view.BindOpenInfo(OnOpenInfoClicked);
            view.BindCloseInfo(OnCloseInfoClicked);
        }

        private void RefreshVisuals()
        {
            if (!saveManager)
                return;

            int nextLevel = saveManager.GetNextLevelIndex() + 1;
            view.SetPlayButtonLevel(nextLevel);
            EventBus<RequestThemeEvent>.Raise(new() { });
        }

        private void OnPlayClicked()
        {
            if (saveManager)
                saveManager.LevelLoadOverride = -1;

            if (SceneEvents.Instance != null)
                SceneEvents.Instance.TriggerChangeSceneAsync(gameplaySceneName);
        }

        private void OnOpenLevelSelectClicked() => view.OpenLevelSelect();
        private void OnCloseLevelSelectClicked() => view.CloseLevelSelect();

        private void OnOpenInfoClicked() => view.OpenInfo();
        private void OnCloseInfoClicked() => view.CloseInfo();

        public void SelectLevelAndPlay(int levelIndex)
        {
            if (saveManager)
                saveManager.LevelLoadOverride = levelIndex;

            if (SceneEvents.Instance != null)
                SceneEvents.Instance.TriggerChangeSceneAsync(gameplaySceneName);
        }
    }
}