using Gameplay.Audio;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Audio
{
    [RequireComponent(typeof(Button))]
    public class PlaySoundOnClick : MonoBehaviour
    {
        [SerializeField] private SFXType soundType = SFXType.UI_Click;
        [SerializeField, Required] private Button button;


        void Reset()
        {
            TryGetComponent(out button);
        }

        void Awake()
        {
            if (button == null)
                TryGetComponent(out button);
        }
        private void Start()
        {
            if (button == null)
                Debug.LogError("Button component is missing");

            button.onClick.AddListener(() =>
             {
                 if (AudioManager.Instance != null)
                 {
                     AudioManager.Instance.PlaySFX(soundType, 0);
                 }
             });
        }
    }
}