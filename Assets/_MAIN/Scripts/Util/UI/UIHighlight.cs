using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image highlightImage;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private Ease easeType = Ease.OutQuad;

    private void Awake()
    {
        if (highlightImage == null)
        {
            Debug.LogError($"{name}: No Image comptnet found.");
            return;
        }

        highlightImage.raycastTarget = false;
        SetAlpha(0f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlightImage.DOFade(1f, fadeDuration).SetEase(easeType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlightImage.DOFade(0f, fadeDuration).SetEase(easeType);
    }

    private void SetAlpha(float alpha)
    {
        Color c = highlightImage.color;
        c.a = alpha;
        highlightImage.color = c;
    }
}