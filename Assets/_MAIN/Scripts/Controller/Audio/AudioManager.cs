using Cysharp.Threading.Tasks;
using Gameplay.Boot.Events;
using Gameplay.Core.Events;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Gameplay.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [BoxGroup("References")]
        [SerializeField, Required] private AudioLibrarySO audioLibrary;


        [SerializeField, BoxGroup("Scriptable Variables")]
        private FloatVariable masterVolume;
        [SerializeField, BoxGroup("Scriptable Variables")]
        private FloatVariable musicVolume;
        [SerializeField, BoxGroup("Scriptable Variables")]
        private FloatVariable sfxVolume;

        [SerializeField, BoxGroup("Audio Sources")]
        private AudioSource musicSourceA;
        [SerializeField, BoxGroup("Audio Sources")]
        private AudioSource musicSourceB;

        [SerializeField, BoxGroup("Settings")]
        private float crossFadeDuration = 2.0f;


        private ObjectPool<AudioSource> sfxPool;
        private bool isPlayingSourceA = true;

        private EventBinding<MainMenuStartEvent> mainMenuStartBind;
        private EventBinding<MatchStartEvent> matchStartBind;
        private EventBinding<MatchEndEvent> matchEndBind;
        private EventBinding<RequestMatchStartEvent> requestStartBind;

        #region event boilerplate
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePool();
            SetupMusicSources();
        }

        private void OnEnable()
        {
            matchStartBind = new(OnMatchStart);
            matchEndBind = new(OnMatchEnd);
            mainMenuStartBind = new(OnMainMenuStart);
            requestStartBind = new(OnRequestMatchStart);

            EventBus<MatchStartEvent>.Register(matchStartBind);
            EventBus<MatchEndEvent>.Register(matchEndBind);
            EventBus<MainMenuStartEvent>.Register(mainMenuStartBind);
            EventBus<RequestMatchStartEvent>.Register(requestStartBind);


            if (musicVolume) musicVolume.OnValueChanged += UpdateMusicVolume;
            if (masterVolume) masterVolume.OnValueChanged += UpdateMusicVolume;
        }

        private void OnMainMenuStart(MainMenuStartEvent evt)
        {
            PlayMusic(MusicType.Menu);
        }
        private void OnMatchStart(MatchStartEvent evt)
        {
            PlayMusic(MusicType.Gameplay);
        }
        private void OnMatchEnd(MatchEndEvent evt)
        {
            PlayMusic(MusicType.Menu);
        }
        private void OnRequestMatchStart(RequestMatchStartEvent evt)
        {
            PlaySFX(SFXType.Level_Start);
        }
        private void OnDisable()
        {
            EventBus<MatchStartEvent>.Deregister(matchStartBind);
            EventBus<MatchEndEvent>.Deregister(matchEndBind);
            EventBus<MainMenuStartEvent>.Deregister(mainMenuStartBind);
            EventBus<RequestMatchStartEvent>.Deregister(requestStartBind);

            if (musicVolume) musicVolume.OnValueChanged -= UpdateMusicVolume;
            if (masterVolume) masterVolume.OnValueChanged -= UpdateMusicVolume;
        }
        #endregion

        private void Start()
        {
            //PlayMusic(MusicType.Menu);
        }

        private void InitializePool()
        {
            sfxPool = new ObjectPool<AudioSource>(
                createFunc: () =>
                {
                    var go = new GameObject("SFX_Pooled");
                    go.transform.SetParent(transform);
                    return go.AddComponent<AudioSource>();
                },
                actionOnGet: (source) => source.gameObject.SetActive(true),
                actionOnRelease: (source) => source.gameObject.SetActive(false),
                actionOnDestroy: (source) => Destroy(source.gameObject),
                defaultCapacity: 10,
                maxSize: 20
            );
        }

        public void PlaySFX(SFXType type, float pitchVar = 0f)
        {
            AudioClip clip = audioLibrary.GetSFX(type);
            if (clip == null) return;

            AudioSource source = sfxPool.Get();
            source.clip = clip;
            source.volume = (sfxVolume ? sfxVolume.Value : 1f) * (masterVolume ? masterVolume.Value : 1f);
            source.pitch = 1f + UnityEngine.Random.Range(-pitchVar, pitchVar);
            source.Play();


            ReleaseToPoolAfterPlayAsync(source, clip.length).Forget();
        }

        private async UniTaskVoid ReleaseToPoolAfterPlayAsync(AudioSource source, float duration)
        {
            // destroyCancellationToken for safe keeping in case Audiomanager gets destroyed for whatver reason
            await UniTask.Delay(TimeSpan.FromSeconds(duration + 0.1f), cancellationToken: destroyCancellationToken);


            if (sfxPool != null && source != null)
            {
                sfxPool.Release(source);
            }
        }


        private void SetupMusicSources()
        {
            if (musicSourceA == null)
                musicSourceA = gameObject.AddComponent<AudioSource>();
            if (musicSourceB == null)
                musicSourceB = gameObject.AddComponent<AudioSource>();

            musicSourceA.loop = true;
            musicSourceB.loop = true;
            musicSourceA.volume = 0;
            musicSourceB.volume = 0;
        }

        public void PlayMusic(MusicType type)
        {
            AudioClip newClip = audioLibrary.GetRandomMusic(type);
            if (newClip == null) return;


            AudioSource activeSource = isPlayingSourceA ? musicSourceA : musicSourceB;
            AudioSource nextSource = isPlayingSourceA ? musicSourceB : musicSourceA;

            if (activeSource.clip == newClip && activeSource.isPlaying) return;

            CrossfadeMusicAsync(activeSource, nextSource, newClip).Forget();
            isPlayingSourceA = !isPlayingSourceA;
        }

        private async UniTaskVoid CrossfadeMusicAsync(AudioSource fadingOut, AudioSource fadingIn, AudioClip newClip)
        {
            //float duration = crossFadeDuration;
            float timer = 0f;
            float targetVol = (musicVolume ? musicVolume.Value : 1f) * (masterVolume ? masterVolume.Value : 1f);

            fadingIn.clip = newClip;
            fadingIn.Play();

            while (timer < crossFadeDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / crossFadeDuration;

                fadingOut.volume = Mathf.Lerp(targetVol, 0, progress);
                fadingIn.volume = Mathf.Lerp(0, targetVol, progress);

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: destroyCancellationToken);
            }

            fadingOut.Stop();
            fadingOut.volume = 0;
            fadingIn.volume = targetVol;
        }

        private void UpdateMusicVolume(float _)
        {
            // updates the volume of the music that is playing now
            float finalVol = (musicVolume ? musicVolume.Value : 1f) * (masterVolume ? masterVolume.Value : 1f);
            if (isPlayingSourceA) musicSourceA.volume = finalVol;
            else musicSourceB.volume = finalVol;
        }
    }
}