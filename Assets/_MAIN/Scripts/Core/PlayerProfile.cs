using System;
using System.Collections.Generic;
namespace Gameplay.Core
{
    [Serializable]
    public class PlayerProfile
    {

        public string AnalyticsUserId;
        public int LastCompletedLevelIndex = -1;
        public Dictionary<string, int> LevelProgress = new();
        public Dictionary<int, string> LevelGrades = new();
        public bool IsSoundMuted = false;
    }
}


