using Gameplay.Core.Events;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThemeUIController : MonoBehaviour
{
    [BoxGroup("References")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField, Required] private Transform contentContainer;

    [BoxGroup("References")]
    [SerializeField, Required] private ThemeButton_UI themeButtonPrefab;

    [SerializeField] private int maxThemes = 20;
    [SerializeField] private GradientLibrarySO themeLibrary;

    private List<ThemeButton_UI> buttonPool = new();
    private EventBinding<MatchPrepareEvent> prepareMatchBinding;

    private void Awake()
    {
        contentContainer.GetComponentsInChildren(true, buttonPool);
    }

    private void OnEnable()
    {
        prepareMatchBinding = new(OnPrepareMatch);
        EventBus<MatchPrepareEvent>.Register(prepareMatchBinding);
    }
    private void OnDestroy()
    {
        EventBus<MatchPrepareEvent>.Deregister(prepareMatchBinding);
    }

    private void OnPrepareMatch(MatchPrepareEvent _)
    {
        //scrollRect.onValueChanged.AddListener((_) => NormalizeScrollRectPosition());
        RefreshLevelList();
    }
    [Button("Force Refresh")]
    public void RefreshLevelList()
    {
        if (themeLibrary == null) return;

        int totalThemes = themeLibrary.Entries.Count;



        EnsurePoolSize(totalThemes);


        for (int i = 0; i < buttonPool.Count; i++)
        {
            ThemeButton_UI buttonView = buttonPool[i];


            if (i < totalThemes)
            {
                buttonView.gameObject.SetActive(true);

                int levelIndex = i;
                GradientSO theme = themeLibrary.Entries[levelIndex];
                Gradient gradient = theme.MainGradient;

                buttonView.Setup(
                    gradient,
                    () => EventBus<ThemeUpdateEvent>.Raise(new() { Theme = theme })
                );
            }
            else
            {
                buttonView.gameObject.SetActive(false);
            }
        }
    }


    private void EnsurePoolSize(int requiredSize)
    {
        int currentSize = buttonPool.Count;

        if (currentSize < requiredSize)
        {
            int missingCount = requiredSize - currentSize;
            for (int k = 0; k < missingCount; k++)
            {
                ThemeButton_UI newBtn = Instantiate(themeButtonPrefab, contentContainer);
                buttonPool.Add(newBtn);
            }
        }
    }
}