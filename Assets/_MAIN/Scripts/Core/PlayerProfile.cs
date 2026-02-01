using System;
using System.Collections.Generic;
namespace Gameplay.Core
{
    [Serializable]
    public class PlayerProfile
    {

        public int LastCompletedLevelIndex = -1;


        // Key: Level ID (string), Value: Stars (int)
        public Dictionary<string, int> LevelProgress = new Dictionary<string, int>();

        public int Coins = 0;
        public bool IsSoundMuted = false;
    }
}


