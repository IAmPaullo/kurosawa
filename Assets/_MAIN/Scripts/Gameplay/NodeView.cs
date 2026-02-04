using DG.Tweening;
using Gameplay.Core;
using Gameplay.Core.Events;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Gameplay.Views
{
    public class NodeView : MonoBehaviour
    {
        [SerializeField] private PieceView pieceView;
        [SerializeField] private Ease rotationEase = Ease.Linear;
        [SerializeField] private Ease moveAwayEase = Ease.InOutSine;
        [SerializeField] private MeshRenderer meshRenderer;

        [ShowInInspector, ReadOnly] public int XPosition { get; private set; }
        [ShowInInspector, ReadOnly] public int YPosition { get; private set; }
        [ShowInInspector, ReadOnly] public bool IsDummy { get; private set; }
        [ShowInInspector, ReadOnly] public bool IsPowered { get; private set; }

        private EventBinding<ThemeUpdateEvent> themeUpdateBind;
        private MaterialPropertyBlock mainMpb;
        private static readonly int TopColorId = Shader.PropertyToID("_Top_Color");
        private static readonly int BottomColorId = Shader.PropertyToID("_Bottom_Color");

        public void Setup(int x, int y, Mesh mainMesh, Mesh glowMesh)
        {
            XPosition = x;
            YPosition = y;
            IsDummy = false;

            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;

            if (pieceView != null && mainMesh != null)
            {
                pieceView.SetupPiece(mainMesh, glowMesh);
            }
            EnsureMpbs();
            UpdateVisuals(0, false, true);
        }

        public void UpdateVisuals(int rotationIndex, bool isPowered, bool instant = false)
        {

            Vector3 TargetRotation = new(0, rotationIndex * 90, 0);

            transform.DOKill();

            if (instant)
            {
                transform.localRotation = Quaternion.Euler(TargetRotation);
            }
            else
            {
                transform.DOLocalRotate(TargetRotation, 0.2f)
                    .SetEase(rotationEase)
                    .SetLink(gameObject);
            }
            IsPowered = isPowered;
            pieceView.PiecePowerRoutine(IsPowered, instant);
        }
        public void SetAsDummy()
        {

            IsDummy = true;

            if (pieceView != null)
            {
                pieceView.gameObject.SetActive(false);
            }
            transform.localScale = Vector3.one;
        }

        public void SetAsMisc()
        {

        }
        private void EnsureMpbs()
        {
            if (mainMpb == null)
                mainMpb = new MaterialPropertyBlock();
            if (mainMpb == null)
                Debug.LogError($"main mpb on {gameObject.name} is null");
        }

        private void OnEnable()
        {
            themeUpdateBind = new(OnThemeUpdate);
            EventBus<ThemeUpdateEvent>.Register(themeUpdateBind);
        }

        private void OnDestroy()
        {
            EventBus<ThemeUpdateEvent>.Deregister(themeUpdateBind);
        }

        private void OnThemeUpdate(ThemeUpdateEvent evt)
        {
            ThemeSO theme = evt.Theme;


            Color topColor = theme.TopColor;
            Color bottomColor = theme.BottomColor;
            Color glowColor = theme.GlowColor;

            UpdateTheme(topColor, bottomColor);
            pieceView.UpdateTheme(topColor, bottomColor, glowColor);
        }

        public void UpdateTheme(Color topColor, Color bottomColor)
        {
            EnsureMpbs();

            if (meshRenderer)
            {
                meshRenderer.GetPropertyBlock(mainMpb);
                mainMpb.SetColor(TopColorId, topColor);
                mainMpb.SetColor(BottomColorId, bottomColor);
                meshRenderer.SetPropertyBlock(mainMpb);
            }
        }
        [Button]
        public Tween DummyMoveAwayTween()
        {
            Tween tween;

            tween = transform.DOMoveY(-50f, .25f)
                        .SetEase(moveAwayEase)
                        .OnComplete(() => gameObject.SetActive(false));
            return tween;
        }
    }
}