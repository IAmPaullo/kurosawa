using System.Collections;
using UnityEngine;

public class BootloaderController : MonoBehaviour
{
    [SerializeField] private int nextSceneIndex = 1;

    private IEnumerator Start()
    {

        Debug.Log("Initializing Systems");


        yield return new WaitForSeconds(1f);

        Debug.Log("Loading Menu");

        if (SceneEvents.Instance != null)
        {
            SceneEvents.Instance.TriggerChangeSceneAsync(nextSceneIndex);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneIndex);
        }
    }
}
