using Gameplay.Managers;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

public class AmplitudeBootstrap : MonoBehaviour
{
    [SerializeField] private string apiKey;
    [SerializeField] private bool enableLogging = true;
    [SerializeField, Required] private StringVariable UserIDVariable;
    private bool initialized;

    public void Init()
    {
        if (initialized) return;
        initialized = true;
        DontDestroyOnLoad(gameObject);

        if (UserIDVariable == null)
            throw new System.Exception("User ID String Variable is Null. This needs immediate attention.");

        Amplitude amp = Amplitude.getInstance();
        amp.logging = enableLogging;
        amp.trackSessionEvents(true);
        amp.init(apiKey);
        if (!string.IsNullOrWhiteSpace(UserIDVariable.Value))
            amp.setUserId(UserIDVariable.Value);

        UserIDVariable.OnValueChanged += OnUserIdChanged;
        StartCoroutine(LogStart());
    }

    private IEnumerator LogStart()
    {
        yield return new WaitForEndOfFrame();
        Amplitude amp = Amplitude.getInstance();
        amp.logEvent("Game_Start");
    }

    private void OnDestroy()
    {
        UserIDVariable.OnValueChanged -= OnUserIdChanged;
    }

    private void OnUserIdChanged(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        Amplitude.getInstance().setUserId(id);
    }

}


