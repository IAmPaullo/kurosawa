using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using System.Collections;

public class SplashScreenSystem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float minDuration = 3f;
    [SerializeField] private Button skipButton;

    public UnityEvent onSplashFinished = new();

    public void Init()
    {
        Application.targetFrameRate = 60;
        skipButton.interactable = false;
        StartCoroutine(RunSplashRoutine());
    }

    private IEnumerator RunSplashRoutine()
    {




        // Allow skipping after a brief moment
        yield return new WaitForSeconds(0.5f);
        skipButton.interactable = true;

        bool skipped = false;
        skipButton.onClick.AddListener(() => skipped = true);

        float elapsed = 0f;
        while (elapsed < minDuration && !skipped)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        FinishSplash();
    }

    private void FinishSplash()
    {
        skipButton.interactable = false;
        onSplashFinished?.Invoke();
    }
}