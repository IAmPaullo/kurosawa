using Cysharp.Threading.Tasks;
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

        [BoxGroup("Debug Config")]
        [SerializeField] private LevelDataSO DebugLevel;
        [SerializeField] private bool AutoStart = true;

        [ShowInInspector, ReadOnly] private int currentSessionLevelIndex;

        private EventBinding<LevelCompletedEvent> levelCompletedBind;
        private EventBinding<RequestNextLevelEvent> nextLevelBind;
        private EventBinding<RequestRestartEvent> restartBind;
        private void Start()
        {
            if (SaveManager == null)
            {
                Debug.LogError("SaveManager  missing");
                return;
            }

            if (AutoStart)
            {
                int savedIndex = SaveManager.GetNextLevelIndex();
                InitializeMatchRoutine(savedIndex).Forget();
            }
        }

        private void OnEnable()
        {
            levelCompletedBind = new(OnLevelCompleted);
            nextLevelBind = new(OnRequestNextLevel);
            restartBind = new(OnRequestRestart);

            EventBus<LevelCompletedEvent>.Register(levelCompletedBind);
            EventBus<RequestNextLevelEvent>.Register(nextLevelBind);
            EventBus<RequestRestartEvent>.Register(restartBind);
        }

        private void OnDisable()
        {
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

            Debug.Log($"Preparing Level {levelIndex}");
            EventBus<MatchPrepareEvent>.Raise(new MatchPrepareEvent { LevelData = data });

            await UniTask.Delay(200);

            Debug.Log("MatchStartEvent Raised. Starting...");
            EventBus<MatchStartEvent>.Raise(new MatchStartEvent());
        }

        private void OnLevelCompleted(LevelCompletedEvent evt)
        {

            Debug.Log($" Level {currentSessionLevelIndex} Complete. Saving progress");

            SaveManager.RegisterLevelCompletion(currentSessionLevelIndex);
            EventBus<MatchEndEvent>.Raise(new MatchEndEvent());
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