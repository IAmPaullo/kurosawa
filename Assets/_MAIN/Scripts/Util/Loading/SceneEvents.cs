using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneEvents : MonoBehaviour
{
    public static SceneEvents Instance { get; private set; }

    public event Action<string> OnChangeScene;
    public event Action OnReloadScene;

    public float SceneLoadProgress { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Chamadas públicas já com LoadingScreen integrado
    public void TriggerChangeSceneAsync(string sceneName)
    {
        OnChangeScene?.Invoke(sceneName);
        StartCoroutine(LoadingScreen.Show(LoadSceneAsync(sceneName)));
    }

    public void TriggerChangeSceneAsync(int sceneIndex)
    {
        var name = SceneManager.GetSceneByBuildIndex(sceneIndex).name;
        OnChangeScene?.Invoke(name);
        StartCoroutine(LoadingScreen.Show(LoadSceneAsync(sceneIndex)));
    }

    public void TriggerChangeNextSceneAsync()
    {
        int index = SceneManager.GetActiveScene().buildIndex + 1;
        StartCoroutine(LoadingScreen.Show(LoadSceneAsync(index)));
    }

    public void TriggerReloadSceneAsync()
    {
        OnReloadScene?.Invoke();
        string currentScene = SceneManager.GetActiveScene().name;
        StartCoroutine(LoadingScreen.Show(LoadSceneAsync(currentScene)));
    }

    public void LoadIntroScene() => TriggerChangeSceneAsync(0);

    public void LoadMainMenuAsync() => TriggerChangeSceneAsync(1);

    // --- Núcleo de carregamento ---
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        SceneLoadProgress = 0f;
        var op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            SceneLoadProgress = op.progress;
            if (op.progress >= 0.9f)
                op.allowSceneActivation = true;

            yield return null;
        }

        SceneLoadProgress = 1f;
    }

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        SceneLoadProgress = 0f;
        var op = SceneManager.LoadSceneAsync(sceneIndex);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            SceneLoadProgress = op.progress;
            if (op.progress >= 0.9f)
                op.allowSceneActivation = true;

            yield return null;
        }

        SceneLoadProgress = 1f;
    }

    // Helpers
    public string GetSceneName() => SceneManager.GetActiveScene().name;
    public bool IsIntro() => string.Equals(GetSceneName(), "Intro", StringComparison.OrdinalIgnoreCase);
}