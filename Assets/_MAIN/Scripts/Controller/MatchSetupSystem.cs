using Cysharp.Threading.Tasks;
using Gameplay.Core.Data;
using Gameplay.Core.Events;
using Sirenix.OdinInspector;
using UnityEngine;
namespace Gameplay.Managers
{
    public class MatchSetupSystem : MonoBehaviour
    {
        [BoxGroup("Debug Config")]
        [SerializeField] private LevelDataSO DebugLevel;
        [SerializeField] private bool AutoStart = true;

        private EventBinding<LevelCompletedEvent> levelCompletedBind;
        private void Start()
        {
            if (AutoStart && DebugLevel != null)
            {
                InitializeMatchRoutine().Forget();
            }
        }

        private void OnEnable()
        {
            levelCompletedBind = new(OnLevelCompleted);
            EventBus<LevelCompletedEvent>.Register(levelCompletedBind);
        }

        private void OnDisable()
        {
            EventBus<LevelCompletedEvent>.Deregister(levelCompletedBind);
        }

        [Button("Force Start Match")]
        public void ForceStart()
        {
            if (DebugLevel != null) InitializeMatchRoutine().Forget();
        }

        private async UniTaskVoid InitializeMatchRoutine()
        {

            EventBus<MatchPrepareEvent>.Raise(new MatchPrepareEvent { LevelData = DebugLevel });


            await UniTask.Delay(500);

            Debug.Log("MatchStartEvent Raised. Starting...");
            EventBus<MatchStartEvent>.Raise(new MatchStartEvent());
        }

        private void OnLevelCompleted(LevelCompletedEvent evt)
        {
            Debug.Log("MatchEndEvent Raised. Calling Match End...");
            EventBus<MatchEndEvent>.Raise(new MatchEndEvent());
        }
    }
}