using Gameplay.Core.Events;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(ParticleSystem))]
public class VFXItem : MonoBehaviour
{

    private ParticleSystem SystemParticle;

    private void Awake()
    {
        SystemParticle = GetComponent<ParticleSystem>();
        var main = SystemParticle.main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    public void Play()
    {
        gameObject.SetActive(true);
        SystemParticle.Play();
    }

    private void OnParticleSystemStopped()
    {

        gameObject.SetActive(false);

    }
    private void OnThemeUpdate(ThemeUpdateEvent evt)
    {
        var main = SystemParticle.main;
        main.startColor = evt.Theme.GlowColor;
    }
}