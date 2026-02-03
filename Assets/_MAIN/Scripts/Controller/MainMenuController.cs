using DG.Tweening;
using Gameplay.Boot.Events;
using Gameplay.Managers;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Title("References")]
    [SerializeField] private SaveManager saveManager;

    [Title("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button chooseLevelButton;
    [SerializeField] private Button infoButton;

    [Title("Windows")]
    [SerializeField] private RectTransform levelSelectWindow;
    [SerializeField] private RectTransform infoWindow;
    [SerializeField] private Image levelSelectBackground;
    [SerializeField] private Image infoBackground;
    [SerializeField] private Button closeLevelSelectButton;
    [SerializeField] private Button closeInfoButton;

    [Title("Text")]
    [SerializeField] private TextMeshProUGUI playButtonText;

    [Title("Config")]
    [SerializeField] private string gameplaySceneName = "GameplayScene";

    private void Start()
    {
        EventBus<BootSetupEvent>.Raise(new() { });
        if (saveManager == null)
            saveManager = FindFirstObjectByType<SaveManager>();

        SetupVisuals();
        SetupButtons();

        levelSelectWindow.gameObject.SetActive(false);
        infoWindow.gameObject.SetActive(false);
    }

    private void SetupVisuals()
    {
        if (saveManager != null)
        {
            int nextLevel = saveManager.GetNextLevelIndex() + 1;
            //playButtonText.text = $"PLAY (Level {nextLevel})";
            Debug.Log($"PLAY (Level {nextLevel})");
        }
    }

    private void SetupButtons()
    {

        playButton.onClick.AddListener(() =>
        {
            if (saveManager)
                saveManager.LevelLoadOverride = -1;
            SceneEvents.Instance.TriggerChangeSceneAsync(gameplaySceneName);
        });


        chooseLevelButton.onClick.AddListener(() => OpenWindow(levelSelectWindow, levelSelectBackground));
        closeLevelSelectButton.onClick.AddListener(() => CloseWindow(levelSelectWindow, levelSelectBackground));


        infoButton.onClick.AddListener(() => OpenWindow(infoWindow, infoBackground));
        closeInfoButton.onClick.AddListener(() => CloseWindow(infoWindow, infoBackground));
    }

    public void SelectLevelAndPlay(int levelIndex)
    {
        if (saveManager) saveManager.LevelLoadOverride = levelIndex;
        SceneEvents.Instance.TriggerChangeSceneAsync(gameplaySceneName);
    }

    private void OpenWindow(RectTransform window, Image background)
    {
        background.DOFade(0, 0);
        background.gameObject.SetActive(true);
        background.DOFade(.9f, .25f).SetEase(Ease.InExpo);
        window.gameObject.SetActive(true);
        window.localScale = Vector3.zero;
        window.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
    }

    private void CloseWindow(RectTransform window, Image background)
    {
        background.DOFade(0f, .20f).SetEase(Ease.InExpo)
            .OnComplete(() => background.gameObject.SetActive(false));
        window.DOScale(0f, 0.2f).SetEase(Ease.InBack)
            .OnComplete(() => window.gameObject.SetActive(false));
    }
}