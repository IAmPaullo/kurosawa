using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace Gameplay.UI
{
    public class LevelButton_UI : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI levelNumberText;
        [SerializeField] private TextMeshProUGUI gradeText;

        [SerializeField] private GameObject gradeContainer;

        public void Setup(int levelIndex, bool isUnlocked, string grade, UnityAction onClick)
        {
            levelNumberText.text = (levelIndex + 1).ToString();


            button.interactable = isUnlocked;

            //lockIcon.SetActive(!isUnlocked);


            button.onClick.RemoveAllListeners();

            if (isUnlocked)
            {

                button.onClick.AddListener(onClick);


                gradeContainer.SetActive(true);

                if (gradeText)
                {
                    if (!string.IsNullOrEmpty(grade))
                    {
                        gradeText.text = grade;
                        gradeText.color = GetGradeColor(grade[0]);
                    }
                    else
                    {
                        gradeText.text = "-"; // sem nota 
                        gradeText.color = Color.white;
                    }
                }
            }
            else
            {

                if (gradeContainer) gradeContainer.SetActive(false);
                if (gradeText) gradeText.text = "";
            }
        }

        private Color GetGradeColor(char grade)
        {
            return grade switch
            {
                'S' => Color.yellow,
                'A' => Color.cyan,
                'B' => Color.green,
                _ => Color.white
            };
        }
    }
}
