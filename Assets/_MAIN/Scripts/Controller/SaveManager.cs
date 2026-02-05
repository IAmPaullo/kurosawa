using UnityEngine;
using System.IO;
using Sirenix.OdinInspector;
using Gameplay.Core;
using Newtonsoft.Json;


namespace Gameplay.Managers
{
    public class SaveManager : MonoBehaviour
    {
        [ShowInInspector, ReadOnly]
        public PlayerProfile CurrentProfile { get; private set; }

        [ShowInInspector, ReadOnly]
        public int LevelLoadOverride { get; set; } = -1;
        // -1 Use saved progress. any other value force that level value

        private string SavePath => Path.Combine(Application.persistentDataPath, "player_save.json");
        [SerializeField] private StringVariable UserIDVariable;




        public void Init()
        {
            LoadProfile();
        }

        [Button]
        public void LoadProfile()
        {
            if (File.Exists(SavePath))
            {
                try
                {
                    string json = File.ReadAllText(SavePath);
                    CurrentProfile = JsonConvert.DeserializeObject<PlayerProfile>(json);
                    EnsureAnalyticsUserId();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"<b>Save Manager</b?> failed to load save: {e.Message}");
                    CreateNewProfile();
                }

            }
            else
            {
                CreateNewProfile();
            }
        }

        [Button]
        public void SaveProfile()
        {
            if (CurrentProfile == null) return;

            string json = JsonConvert.SerializeObject(CurrentProfile, Formatting.Indented);
            File.WriteAllText(SavePath, json);
            Debug.Log(" Game Saved");
        }

        public void RegisterLevelGrade(int levelIndex, string grade)
        {
            bool improved = false;

            if (!CurrentProfile.LevelGrades.ContainsKey(levelIndex))
            {
                CurrentProfile.LevelGrades[levelIndex] = grade;
                improved = true;
            }
            else
            {
                // TODO: helper method for comparison
                string currentGrade = CurrentProfile.LevelGrades[levelIndex];
                if (IsGradeBetter(grade, currentGrade))
                {
                    CurrentProfile.LevelGrades[levelIndex] = grade;
                    improved = true;
                }
            }

            if (improved)
                SaveProfile();
        }

        private bool IsGradeBetter(string newGrade, string oldGrade)
        {
            return newGrade[0] < oldGrade[0];
        }


        [Button]
        public void ResetProgress()
        {
            CreateNewProfile();
            SaveProfile();
        }

        private void CreateNewProfile()
        {
            CurrentProfile = new PlayerProfile();
            EnsureAnalyticsUserId();
        }
        private void EnsureAnalyticsUserId()
        {
            if (CurrentProfile == null) return;

            if (string.IsNullOrWhiteSpace(CurrentProfile.AnalyticsUserId))
            {
                CurrentProfile.AnalyticsUserId = System.Guid.NewGuid().ToString("N");
                SaveProfile();
            }
            if (UserIDVariable == null)
                throw new System.Exception("User ID String Variable is Null. This needs immediate attention.");
            UserIDVariable.SetValue(CurrentProfile.AnalyticsUserId);
        }

        public void RegisterLevelCompletion(int levelIndex)
        {
            if (levelIndex > CurrentProfile.LastCompletedLevelIndex)
            {
                CurrentProfile.LastCompletedLevelIndex = levelIndex;
                SaveProfile();
            }
        }

        public int GetNextLevelIndex()
        {
            return CurrentProfile.LastCompletedLevelIndex + 1;
        }


#if UNITY_EDITOR
        [Title("Debug Tools")]
        [Button("DELETE SAVE FILE", ButtonSizes.Large), GUIColor(1, 0, 0)]
        public void DeleteSaveFile()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.LogWarning($"Save file deleted at: {SavePath}");
            }


            CreateNewProfile();

            Debug.LogWarning("Memory reset. Restart Play mode to ensure fresh state");
        }

        [Button("Open Save Folder")]
        public void OpenSaveFolder()
        {
            Application.OpenURL(Application.persistentDataPath);
        }
#endif
    }
}