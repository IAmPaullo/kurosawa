using Cysharp.Threading.Tasks;
using Gameplay.Core.Controllers;
using Gameplay.Core.Data;
using Gameplay.Core.Events;
using Sirenix.OdinInspector;
using UnityEngine;
namespace Gameplay.Managers
{
    public class MatchSetupSystem : MonoBehaviour
    {
        [BoxGroup("Config")]
        [SerializeField] private CampaignSO Campaign;

        [BoxGroup("Dependencies")]
        [SerializeField] private SaveManager SaveManager;
        [SerializeField] private CameraController CameraController;

        [BoxGroup("Debug Config")]
        [SerializeField] private LevelDataSO DebugLevel;
        [SerializeField] private bool AutoStart = true;

        [Title("Settings")]
        [Range(0f, .99f), SerializeField]
        private float pauseTimeScale = .75f;


        [ShowInInspector, ReadOnly] public bool IsPaused { get; private set; }
        [ShowInInspector, ReadOnly] private int currentSessionLevelIndex;

        [ShowInInspector, ReadOnly] private float levelStartTime;
        [ShowInInspector, ReadOnly] private float totalPausedDuration;
        [ShowInInspector, ReadOnly] private float lastPauseStartTime;
        [ShowInInspector, ReadOnly] private float finalElapsedTime;

        private EventBinding<RequestPauseEvent> pauseRequestBind;
        private EventBinding<RequestResumeEvent> resumeRequestBind;
        private EventBinding<LevelCompletedEvent> levelCompletedBind;
        private EventBinding<RequestNextLevelEvent> nextLevelBind;
        private EventBinding<RequestRestartEvent> restartBind;

        private void Start()
        {
            if (SaveManager == null)
            {
                Debug.LogError("SaveManager missing");
                return;
            }

            if (AutoStart)
            {
                int levelToLoad;

                if (SaveManager.LevelLoadOverride != -1)
                {
                    levelToLoad = SaveManager.LevelLoadOverride;
                    Debug.Log($"Loading Selected level: {levelToLoad}");
                }
                else
                {
                    levelToLoad = SaveManager.GetNextLevelIndex();
                    Debug.Log($"loading progress level: {levelToLoad}");
                }

                SaveManager.LevelLoadOverride = -1;

                InitializeMatchRoutine(levelToLoad).Forget();
            }
        }

        private void OnEnable()
        {
            pauseRequestBind = new(OnPauseRequest);
            resumeRequestBind = new(OnResumeRequest);
            levelCompletedBind = new(OnLevelCompleted);
            nextLevelBind = new(OnRequestNextLevel);
            restartBind = new(OnRequestRestart);

            EventBus<RequestPauseEvent>.Register(pauseRequestBind);
            EventBus<LevelCompletedEvent>.Register(levelCompletedBind);
            EventBus<RequestNextLevelEvent>.Register(nextLevelBind);
            EventBus<RequestRestartEvent>.Register(restartBind);
            EventBus<RequestResumeEvent>.Register(resumeRequestBind);
        }

        private void OnDisable()
        {
            EventBus<RequestPauseEvent>.Deregister(pauseRequestBind);
            EventBus<RequestResumeEvent>.Deregister(resumeRequestBind);
            EventBus<LevelCompletedEvent>.Deregister(levelCompletedBind);
            EventBus<RequestNextLevelEvent>.Deregister(nextLevelBind);
            EventBus<RequestRestartEvent>.Deregister(restartBind);
        }
#if UNITY_EDITOR
        [Button("Force Start Match")]
        public void ForceStart()
        {
            if (DebugLevel != null)
                InitializeMatchRoutine(currentSessionLevelIndex).Forget();
        }
#endif
        private async UniTaskVoid InitializeMatchRoutine(int levelIndex)
        {

            if (Campaign == null) return;


            if (levelIndex >= Campaign.Levels.Count)
            {
                Debug.Log("Campaign Finished! Looping");
                levelIndex = 0;
            }

            LevelDataSO data = Campaign.GetLevel(levelIndex);

            if (data == null)
            {
                Debug.LogError($"Data not found for level {levelIndex}");
                return;
            }


            currentSessionLevelIndex = levelIndex;

            totalPausedDuration = 0f;
            finalElapsedTime = 0f;

            Debug.Log($"Preparing Level {levelIndex}");
            EventBus<MatchPrepareEvent>.Raise(new MatchPrepareEvent { LevelData = data });

            await UniTask.Delay(200);

            levelStartTime = Time.time;

            Debug.Log("MatchStartEvent Raised. Starting...");
            EventBus<MatchStartEvent>.Raise(new MatchStartEvent());
        }

        private void OnPauseRequest(RequestPauseEvent evt)
        {
            if (IsPaused) return;
            IsPaused = true;

            lastPauseStartTime = Time.time;
            if (CameraController != null)
                CameraController.SetPaused(true);

            Time.timeScale = pauseTimeScale;

            Debug.Log("Game Paused");
        }
        private void OnResumeRequest(RequestResumeEvent evt)
        {
            if (!IsPaused) return;
            IsPaused = false;

            float duration = Time.time - lastPauseStartTime;
            totalPausedDuration += duration;

            if (CameraController != null)
                CameraController.SetPaused(false);

            Time.timeScale = 1;

            Debug.Log("Game Resumed");
        }
        private void OnLevelCompleted(LevelCompletedEvent evt)
        {
            Debug.Log($" Level {currentSessionLevelIndex} Complete. Saving progress");

            float endTime = Time.time;
            finalElapsedTime = endTime - levelStartTime - totalPausedDuration;

            string grade = CalculatePlayerGrade();
            SaveManager.RegisterLevelCompletion(currentSessionLevelIndex);
            SaveManager.RegisterLevelGrade(currentSessionLevelIndex, grade);
            EventBus<MatchEndEvent>.Raise(new MatchEndEvent());
        }

        private string CalculatePlayerGrade()
        {
            LevelDataSO data = Campaign.GetLevel(currentSessionLevelIndex);
            string grade = data.CalculateGrade(finalElapsedTime);
            Debug.Log($"Grade Achieved: {grade}");
            return grade;
        }

        private void OnRequestNextLevel(RequestNextLevelEvent evt)
        {
            int nextIndex = currentSessionLevelIndex + 1;
            InitializeMatchRoutine(nextIndex).Forget();
        }

        private void OnRequestRestart(RequestRestartEvent evt)
        {
            InitializeMatchRoutine(currentSessionLevelIndex).Forget();
        }
    }
}