

namespace Gameplay.UI
{
    using Sirenix.OdinInspector;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    public class MainMenuView : MonoBehaviour
    {
        [Title("Buttons")]
        [SerializeField, Required] private Button playButton;
        [SerializeField, Required] private Button chooseLevelButton;
        [SerializeField, Required] private Button infoButton;

        [Title("Windows")]
        [SerializeField] private UIWindowController levelSelectWindow;
        [SerializeField] private UIWindowController infoWindow;

        [Title("Text")]
        [SerializeField] private TextMeshProUGUI playButtonText;

        public void Initialize()
        {
            if (levelSelectWindow) levelSelectWindow.gameObject.SetActive(false);
            if (infoWindow) infoWindow.gameObject.SetActive(false);
        }

        public void SetPlayButtonLevel(int level)
        {
            if (!playButtonText)
                return;

            playButtonText.text = $"PLAY (Level {level})";
        }

        public void BindPlay(UnityAction action) => BindButton(playButton, action);
        public void BindOpenLevelSelect(UnityAction action) => BindButton(chooseLevelButton, action);
        public void BindCloseLevelSelect(UnityAction action) => BindWindowClose(levelSelectWindow, action);

        public void BindOpenInfo(UnityAction action) => BindButton(infoButton, action);
        public void BindCloseInfo(UnityAction action) => BindWindowClose(infoWindow, action);

        public void OpenLevelSelect() => levelSelectWindow?.Open();
        public void CloseLevelSelect() => levelSelectWindow?.Close();

        public void OpenInfo() => infoWindow?.Open();
        public void CloseInfo() => infoWindow?.Close();

        private void BindButton(Button button, UnityAction action)
        {
            if (!button)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private void BindWindowClose(UIWindowController window, UnityAction action)
        {
            if (!window)
                return;

            Button closeButton = window.GetComponentInChildren<Button>(true);
            if (!closeButton)
                return;

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(action);
        }
    }
}