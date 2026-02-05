using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;


namespace Gameplay.Settings
{

    using System;
    using Gameplay.Boot.Events;
    using Gameplay.Core.Events;
    using Sirenix.OdinInspector;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    public sealed class SettingsManager : MonoBehaviour
    {
        private const string MasterVolumeKey = "Settings.MasterVolume";
        private const string MusicVolumeKey = "Settings.MusicVolume";
        private const string HapticsEnabledKey = "Settings.HapticsEnabled";

        public enum SettingsScreenMode
        {
            Menu,
            Gameplay
        }

        [Title("Mode")]
        [SerializeField] private SettingsScreenMode ScreenMode = SettingsScreenMode.Gameplay;

        [Title("Smart Variables")]
        [SerializeField, Required] private FloatVariable MasterVolume;
        [SerializeField, Required] private FloatVariable MusicVolume;
        [SerializeField, Required] private BoolVariable HapticsEnabled;

        [Title("View")]
        [SerializeField, Required] private SettingsView View;

        [Title("Actions")]
        [SerializeField] private UnityEvent RestartLevelRequested;
        [SerializeField] private UnityEvent BackToMenuRequested;

        private bool IsInitialized;
        private bool IsActive;
        private bool IsUpdatingView;

        private EventBinding<MainMenuStartEvent> bootSetupEventBind;
        private EventBinding<MatchPrepareEvent> matchPrepareBind;

        private void OnEnable()
        {
            bootSetupEventBind = new(OnBootSetup);
            matchPrepareBind = new(OnMatchPrepare);
            EventBus<MainMenuStartEvent>.Register(bootSetupEventBind);
            EventBus<MatchPrepareEvent>.Register(matchPrepareBind);
        }

        private void OnMatchPrepare(MatchPrepareEvent _)
        {
            Initialize(SettingsScreenMode.Gameplay);
            Activate();
        }

        private void OnBootSetup(MainMenuStartEvent _)
        {
            Initialize(SettingsScreenMode.Menu);
            Activate();
        }
        private void OnDisable()
        {
            Deactivate();
        }
        private void OnDestroy()
        {
            EventBus<MainMenuStartEvent>.Deregister(bootSetupEventBind);
            EventBus<MatchPrepareEvent>.Deregister(matchPrepareBind);
        }
        public void Initialize(SettingsScreenMode screenMode)
        {
            ScreenMode = screenMode;

            LoadFromPrefs();

            IsInitialized = true;
        }

        public void Activate()
        {
            if (!IsInitialized)
                LoadFromPrefs();

            if (IsActive)
                return;

            BindView();
            BindVariables();

            ApplyVisibility();
            ApplyToView();

            IsActive = true;
        }

        public void Deactivate()
        {
            if (!IsActive)
                return;

            UnbindView();
            UnbindVariables();

            SaveToPrefs();

            IsActive = false;
        }

        public void SetScreenMode(SettingsScreenMode mode)
        {
            ScreenMode = mode;
            if (IsActive)
                ApplyVisibility();
        }

        private void ApplyVisibility()
        {
            bool ShowRestart = ScreenMode == SettingsScreenMode.Gameplay;

            if (View.BackToMenuButton != null)
                View.BackToMenuButton.gameObject.SetActive(true);
        }

        private void BindView()
        {
            if (View.MasterVolumeSlider != null)
                View.MasterVolumeSlider.onValueChanged.AddListener(OnMasterSliderChanged);

            if (View.MusicVolumeSlider != null)
                View.MusicVolumeSlider.onValueChanged.AddListener(OnMusicSliderChanged);

            if (View.HapticsButton != null)
                View.HapticsButton.onClick.AddListener(OnHapticsButtonClicked);

            if (View.BackToMenuButton != null)
                View.BackToMenuButton.onClick.AddListener(OnBackToMenuClicked);
        }

        private void UnbindView()
        {
            if (View.MasterVolumeSlider != null)
                View.MasterVolumeSlider.onValueChanged.RemoveListener(OnMasterSliderChanged);

            if (View.MusicVolumeSlider != null)
                View.MusicVolumeSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);

            if (View.HapticsButton != null)
                View.HapticsButton.onClick.RemoveListener(OnHapticsButtonClicked);

            if (View.BackToMenuButton != null)
                View.BackToMenuButton.onClick.RemoveListener(OnBackToMenuClicked);
        }

        private void BindVariables()
        {
            MasterVolume.OnValueChanged += OnMasterVariableChanged;
            MusicVolume.OnValueChanged += OnMusicVariableChanged;
            HapticsEnabled.OnValueChanged += OnHapticsVariableChanged;
        }

        private void UnbindVariables()
        {
            MasterVolume.OnValueChanged -= OnMasterVariableChanged;
            MusicVolume.OnValueChanged -= OnMusicVariableChanged;
            HapticsEnabled.OnValueChanged -= OnHapticsVariableChanged;
        }

        private void OnMasterSliderChanged(float value)
        {
            if (IsUpdatingView) return;

            MasterVolume.SetValue(Mathf.Clamp01(value));
            SaveToPrefs();
        }

        private void OnMusicSliderChanged(float value)
        {
            if (IsUpdatingView) return;

            MusicVolume.SetValue(Mathf.Clamp01(value));
            SaveToPrefs();
        }

        private void OnHapticsButtonClicked()
        {
            HapticsEnabled.SetValue(!HapticsEnabled.Value);
            SaveToPrefs();
        }

        private void OnBackToMenuClicked()
        {
            SaveToPrefs();
            if (SceneEvents.Instance != null)
            {
                SceneEvents.Instance.TriggerChangeSceneAsync(1);
            }
        }

        private void OnMasterVariableChanged(float value) => ApplyToView();
        private void OnMusicVariableChanged(float value) => ApplyToView();
        private void OnHapticsVariableChanged(bool value) => ApplyToView();

        private void ApplyToView()
        {
            if (!IsActive && IsInitialized == false)
                return;

            IsUpdatingView = true;

            if (View.MasterVolumeSlider != null)
                View.MasterVolumeSlider.SetValueWithoutNotify(Mathf.Clamp01(MasterVolume.Value));

            if (View.MusicVolumeSlider != null)
                View.MusicVolumeSlider.SetValueWithoutNotify(Mathf.Clamp01(MusicVolume.Value));

            if (View.HapticsLabel != null)
                View.HapticsLabel.text = HapticsEnabled.Value ? "Haptics: On" : "Haptics: Off";

            IsUpdatingView = false;
        }

        private void LoadFromPrefs()
        {
            float Master = PlayerPrefs.GetFloat(MasterVolumeKey, 1);
            float Music = PlayerPrefs.GetFloat(MusicVolumeKey, .25f);
            bool Haptics = PlayerPrefs.GetInt(HapticsEnabledKey, HapticsEnabled.Value ? 1 : 0) == 1;

            MasterVolume.SetValue(Mathf.Clamp01(Master));
            MusicVolume.SetValue(Mathf.Clamp01(Music));
            HapticsEnabled.SetValue(Haptics);
        }

        private void SaveToPrefs()
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, Mathf.Clamp01(MasterVolume.Value));
            PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(MusicVolume.Value));
            PlayerPrefs.SetInt(HapticsEnabledKey, HapticsEnabled.Value ? 1 : 0);
            PlayerPrefs.Save();
        }

        [Serializable]
        public sealed class SettingsView
        {
            public Slider MasterVolumeSlider;
            public Slider MusicVolumeSlider;

            public Button HapticsButton;
            public TextMeshProUGUI HapticsLabel;

            public Button BackToMenuButton;
        }
    }
}