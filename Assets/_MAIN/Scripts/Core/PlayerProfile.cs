using System;
using System.Collections.Generic;
namespace Gameplay.Core
{
    [Serializable]
    public class PlayerProfile
    {
        public string LastCompletedLevelID; // ID único (GUID ou string fixa)
        public int Coins;
        public Dictionary<string, int> LevelStars; // ID da fase -> Estrelas
        public bool IsSoundMuted;
    }
}