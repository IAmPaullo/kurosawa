using DG.Tweening;
using Gameplay.Core;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Gameplay.Views
{
    public class NodeView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color colorOn = Color.cyan;
        [SerializeField] private Color colorOff = Color.gray;
        [SerializeField] private Ease rotationEase = Ease.Linear;


        [ShowInInspector, ReadOnly] public int XPosition { get; private set; }
        [ShowInInspector, ReadOnly] public int YPosition { get; private set; }

        public void Setup(int x, int y, Sprite icon)
        {
            XPosition = x;
            YPosition = y;

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = icon;
                spriteRenderer.transform.localRotation = Quaternion.Euler(90, 0, 0);
            }

            UpdateVisuals(0, false, true);
        }

        public void UpdateVisuals(int rotationIndex, bool isPowered, bool instant = false)
        {

            Vector3 TargetRotation = new Vector3(0, rotationIndex * 90, 0);

            if (instant)
            {
                transform.localRotation = Quaternion.Euler(TargetRotation);
            }
            else
            {
                transform.DOLocalRotate(TargetRotation, 0.2f)
                .SetEase(rotationEase);
            }

            spriteRenderer.color = isPowered ? colorOn : colorOff;
        }
        public void SetAsDummy()
        {
            spriteRenderer.gameObject.SetActive(false);
        }
    }
}