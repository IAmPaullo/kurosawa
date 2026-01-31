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

        public event Action<int, int> OnNodeClicked;

        [ShowInInspector, ReadOnly] private int xPosition;
        [ShowInInspector, ReadOnly] private int yPosition;

        public void Setup(int x, int y, Sprite icon)
        {
            xPosition = x;
            yPosition = y;

            if (spriteRenderer != null)
                spriteRenderer.sprite = icon;

            UpdateVisuals(0, false);

        }

        public void UpdateVisuals(int rotationIndex = 0, bool isPowered = false)
        {

            transform.DORotate(new Vector3(0, 0, -90 * rotationIndex), 0.2f)
                .SetEase(rotationEase);

            spriteRenderer.color = isPowered ? colorOn : colorOff;
        }

        private void OnMouseDown()
        {
            OnNodeClicked?.Invoke(xPosition, yPosition);
        }
    }
}