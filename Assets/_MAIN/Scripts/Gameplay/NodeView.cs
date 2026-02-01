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
        [ShowInInspector, ReadOnly] public bool IsDummy { get; private set; }
        [ShowInInspector, ReadOnly] public bool IsPowered { get; private set; }


        public void Setup(int x, int y, Sprite icon)
        {
            XPosition = x;
            YPosition = y;
            IsDummy = false;

            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;

            if (spriteRenderer != null)
            {

                spriteRenderer.gameObject.SetActive(true);

                spriteRenderer.sprite = icon;
                spriteRenderer.transform.localRotation = Quaternion.Euler(90, 0, 0);
            }

            UpdateVisuals(0, false, true);
        }

        public void UpdateVisuals(int rotationIndex, bool isPowered, bool instant = false)
        {

            Vector3 TargetRotation = new(0, rotationIndex * 90, 0);

            if (instant)
            {
                transform.localRotation = Quaternion.Euler(TargetRotation);
            }
            else
            {
                transform.DOLocalRotate(TargetRotation, 0.2f)
                .SetEase(rotationEase);
            }
            IsPowered = isPowered;
            spriteRenderer.color = IsPowered ? colorOn : colorOff;
        }
        public void SetAsDummy()
        {

            IsDummy = true;

            if (spriteRenderer != null)
            {
                spriteRenderer.gameObject.SetActive(false);
            }
            transform.localScale = Vector3.one;
        }

        public void SetAsMisc()
        {

        }
    }
}