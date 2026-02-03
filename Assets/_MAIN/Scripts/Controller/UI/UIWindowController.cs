using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class UIWindowController : MonoBehaviour
    {
        [Required, SerializeField] private RectTransform window;
        [SerializeField] private Image background;

        [SerializeField] private float fadeInDuration = 0.15f;
        [SerializeField] private float fadeOutDuration = 0.15f;
        [SerializeField] private float scaleDuration = 0.2f;
        [SerializeField] private Ease scaleInEase = Ease.OutBack;
        [SerializeField] private Ease scaleOutEase = Ease.InBack;
        [SerializeField] private float targetAlpha = 0.85f;

        private Sequence seq;

        public void Open()
        {
            seq?.Kill();
            seq = DOTween.Sequence().SetUpdate(true);

            gameObject.SetActive(true);

            if (background != null)
            {
                background.DOKill();
                background.color = new Color(background.color.r, background.color.g, background.color.b, 0f);
                seq.Append(background.DOFade(targetAlpha, fadeInDuration));
            }

            window.DOKill();
            window.gameObject.SetActive(true);
            window.localScale = Vector3.zero;
            seq.Append(window.DOScale(1f, scaleDuration).SetEase(scaleInEase));
        }

        public void Close()
        {
            seq?.Kill();
            seq = DOTween.Sequence().SetUpdate(true);

            window.DOKill();
            seq.Append(window.DOScale(0f, scaleDuration).SetEase(scaleOutEase));

            if (background != null)
            {
                background.DOKill();
                seq.Append(background.DOFade(0f, fadeOutDuration));
            }

            seq.AppendCallback(() => gameObject.SetActive(false));
        }
    }
}
