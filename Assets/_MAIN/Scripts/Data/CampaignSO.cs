using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;


namespace Gameplay.Core.Data
{
    [CreateAssetMenu(fileName = "Campaign", menuName = "Game/Campaign Data")]
    public class CampaignSO : ScriptableObject
    {
        [Title("Campaign Config")]
        [ListDrawerSettings(ShowIndexLabels = true)]
        public List<LevelDataSO> Levels = new();

        public LevelDataSO GetLevel(int index)
        {
            if (Levels == null || Levels.Count == 0) return null;
            if (index < 0 || index >= Levels.Count) return null;

            return Levels[index];
        }
    }
}