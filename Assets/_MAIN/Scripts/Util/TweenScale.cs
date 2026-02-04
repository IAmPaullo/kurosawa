using DG.Tweening;
using UnityEngine;

public static class TweenScale
{
    public static Tween SquashY(Transform target, float targetY, float duration, Ease ease = Ease.OutQuad)
    {
        var start = target.localScale;
        var end = new Vector3(start.x, targetY, start.z);
        return target.DOScale(end, duration).SetEase(ease);
    }
}