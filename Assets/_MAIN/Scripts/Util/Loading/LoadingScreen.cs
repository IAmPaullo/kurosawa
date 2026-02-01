using Sirenix.OdinInspector;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreenUI;
    [SerializeField] private Slider progressBar;

    [SerializeField] private TextMeshProUGUI loadingLabel;
    [SerializeField] private Image screenOverlay;

    private static LoadingScreen instance;

    [Title("Transitions")]
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private float transitionDuration = 1f;
    [ShowInInspector, ReadOnly]
    private static readonly int _fadeOutTriggerHash = Animator.StringToHash("FadeOut");
    [ShowInInspector, ReadOnly]
    private static readonly int _fadeInTriggerHash = Animator.StringToHash("FadeIn");
    [ShowInInspector, ReadOnly] public static bool HasProgressBar { get; private set; }
    [ShowInInspector, ReadOnly] public static bool HasLabel { get; private set; }
    [ShowInInspector, ReadOnly] public static bool HasOverlay { get; private set; }



    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeFlags();
    }

    [Button]
    private static IEnumerator FadeOut(Action OnComplete = null)
    {
        if (!instance.transitionAnimator.gameObject.activeSelf)
        {
            instance.transitionAnimator.gameObject.SetActive(true);
        }

        instance.transitionAnimator.SetTrigger(_fadeOutTriggerHash);
        yield return new WaitForSeconds(instance.transitionDuration);
        OnComplete?.Invoke();
        yield return new WaitForSeconds(instance.transitionDuration / 2);
        Debug.LogWarning("terminou fade out");
    }

    [Button]
    private static IEnumerator FadeIn(Action OnComplete = null)
    {
        instance.transitionAnimator.SetTrigger(_fadeInTriggerHash);
        yield return new WaitForSeconds(instance.transitionDuration);
        OnComplete?.Invoke();
        Debug.LogWarning("terminou fade in");
    }
    private void InitializeFlags()
    {
        HasProgressBar = progressBar != null;
        HasLabel = loadingLabel != null;
        HasOverlay = screenOverlay != null;
    }
    public static IEnumerator Show(IEnumerator sceneLoadTask)
    {
        if (instance == null)
        {
            Debug.LogError("LoadingScreen instance not found!");
            yield break;
        }

        // 1. Ativa a UI


        // 2. Executa o FADE OUT e espera
        yield return FadeOut(() => PrepareUI());

        // 3. Inicia o load E espera o progresso
        instance.StartCoroutine(sceneLoadTask);
        yield return TrackLoadProgress();

        
        HideUI();

        yield return FadeIn();

        // 5. Desativa a UI
    }

    private static IEnumerator TrackLoadProgress()
    {
        while (SceneEvents.Instance.SceneLoadProgress < 1f)
        {
            float progress = Mathf.Clamp01(SceneEvents.Instance.SceneLoadProgress);

            if (HasProgressBar)
                instance.progressBar.value = progress;

            if (HasLabel)
                instance.loadingLabel.text = $"Loading {(progress * 100):F0}%";

            yield return null;
        }
        Debug.LogWarning("terminou tracking");

        if (HasProgressBar)
            instance.progressBar.value = 1f;

        if (HasLabel)
            instance.loadingLabel.text = "Loading 100%";
    }

    private static void ShowOverlay()
    {
        if (HasOverlay)
            instance.screenOverlay.gameObject.SetActive(true);
    }
    private static void PrepareUI()
    {
        if (instance.loadingScreenUI != null)
            instance.loadingScreenUI.SetActive(true);

        if (HasProgressBar)
            instance.progressBar.value = 0;
    }
    private static void HideUI()
    {
        if (HasOverlay)
            instance.screenOverlay.gameObject.SetActive(false);

        if (instance.loadingScreenUI != null)
            instance.loadingScreenUI.SetActive(false);
    }
    private void Reset()
    {
        InitializeFlags();
    }
}