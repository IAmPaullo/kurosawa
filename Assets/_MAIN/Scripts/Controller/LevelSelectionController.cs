using Gameplay.Core.Data;
using Gameplay.Managers;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Gameplay.Boot.Events;
using System.Collections;

namespace Gameplay.UI
{
    public class LevelSelectionController : MonoBehaviour
    {
        [BoxGroup("References")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField, Required] private Transform contentContainer;

        [BoxGroup("References")]
        [SerializeField, Required] private LevelButton_UI levelButtonPrefab;

        [BoxGroup("Data")]
        [SerializeField, Required] private CampaignSO campaign;

        [BoxGroup("Dependencies")]
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private MainMenuController mainMenuController;

        private List<LevelButton_UI> buttonPool = new();

        private EventBinding<MainMenuStartEvent> setupMenuBinding;

        private void Awake()
        {
            contentContainer.GetComponentsInChildren(true, buttonPool);
        }
        private void OnEnable()
        {
            setupMenuBinding = new(OnSetupEvent);
            EventBus<MainMenuStartEvent>.Register(setupMenuBinding);
        }
        private void OnDestroy()
        {
            EventBus<MainMenuStartEvent>.Deregister(setupMenuBinding);
        }
        private void OnSetupEvent(MainMenuStartEvent _)
        {
            if (saveManager == null) saveManager = FindFirstObjectByType<SaveManager>();
            scrollRect.onValueChanged.AddListener((_) => NormalizeScrollRectPosition());
            RefreshLevelList();
        }

        [Button("Force Refresh")]
        public void RefreshLevelList()
        {
            if (campaign == null || saveManager == null) return;

            int totalLevels = campaign.Levels.Count;
            int unlockedIndexLimit = saveManager.CurrentProfile.LastCompletedLevelIndex + 1;


            EnsurePoolSize(totalLevels);


            for (int i = 0; i < buttonPool.Count; i++)
            {
                LevelButton_UI buttonView = buttonPool[i];


                if (i < totalLevels)
                {
                    buttonView.gameObject.SetActive(true);

                    int levelIndex = i;
                    bool isUnlocked = levelIndex <= unlockedIndexLimit;
                    string grade = "";

                    if (isUnlocked)
                    {
                        grade = TryFindGradeFromLevelIndex(levelIndex);
                    }



                    buttonView.Setup(
                        levelIndex,
                        isUnlocked,
                        grade,
                        () => mainMenuController.SelectLevelAndPlay(levelIndex)
                    );
                }
                else
                {
                    buttonView.gameObject.SetActive(false);
                }
            }
        }

        private void NormalizeScrollRectPosition()
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }

        private string TryFindGradeFromLevelIndex(int i)
        {
            string grade = "<3";
            saveManager.CurrentProfile.LevelGrades.TryGetValue(i, out grade);
            return grade;
        }

        private void EnsurePoolSize(int requiredSize)
        {
            int currentSize = buttonPool.Count;

            if (currentSize < requiredSize)
            {
                int missingCount = requiredSize - currentSize;
                for (int k = 0; k < missingCount; k++)
                {
                    LevelButton_UI newBtn = Instantiate(levelButtonPrefab, contentContainer);
                    buttonPool.Add(newBtn);
                }
            }
        }
    }
}