using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ThemeButton_UI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image image;
    [SerializeField] private Ease ease;
    public void Setup(Gradient gradient, UnityAction onClick)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);
        image.color = gradient.Evaluate(.5f);
    }

    public Tween EnableIcon()
    {
        button.enabled = true;
        Tween tween = transform.GetComponent<RectTransform>().DOScale(1, .15f).SetEase(ease);
        return tween;
    }

}