using DG.Tweening;
using Gameplay.Boot.Events;
using Gameplay.Managers;
using System.Collections;
using UnityEngine;

namespace Gameplay.Boot
{

    public class BootloaderController : MonoBehaviour
    {
        [SerializeField] private int nextSceneIndex = 1;
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private AmplitudeBootstrap amplitudeBootstrap;
        [SerializeField] private SplashScreenSystem splashScreen;

        bool splashScreenSkipped;
        private IEnumerator Start()
        {

            Debug.Log("Initializing Systems");
            SetupSplashScreen();
            InitSaveManager();
            InitAmplitudeSDK();

            bool canProceed = false;
            splashScreen.onSplashFinished.AddListener(() => canProceed = true);
            splashScreen.Init();

            yield return new WaitUntil(() => canProceed);

            Debug.Log("Loading Scene Index: " + nextSceneIndex);

            if (SceneEvents.Instance != null)
            {
                SceneEvents.Instance.TriggerChangeSceneAsync(nextSceneIndex);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneIndex);
            }
        }

        private void InitAmplitudeSDK()
        {
            amplitudeBootstrap.Init();
        }

        private void InitSaveManager()
        {
            saveManager.Init();
        }

        private void SetupSplashScreen()
        {
            splashScreen.Init();
        }
    }
}
