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

        private IEnumerator Start()
        {

            Debug.Log("Initializing Systems");
            saveManager.Init();
            amplitudeBootstrap.Init();

            yield return new WaitForEndOfFrame();

            Debug.Log("Loading Menu");

            if (SceneEvents.Instance != null)
            {
                SceneEvents.Instance.TriggerChangeSceneAsync(nextSceneIndex);

            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneIndex);
            }
            yield break;
        }
    }
}