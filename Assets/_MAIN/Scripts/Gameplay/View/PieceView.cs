using DG.Tweening;
using Gameplay.Core.Events;
using Gameplay.VFX;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Gameplay.Views
{
    public class PieceView : MonoBehaviour
    {
        [SerializeField] private MeshFilter mainMeshFilter;
        [SerializeField] private MeshRenderer mainMeshRenderer;

        [SerializeField] private MeshFilter glowMeshFilter;
        [SerializeField] private MeshRenderer glowMeshRenderer;
        [SerializeField] private VFXItem vfxItem;

        [SerializeField] private Color emissionBaseColor = Color.white;

        [SerializeField] private float poweredIntensity = 4f;
        [SerializeField] private float unpoweredIntensity = 0f;

        [Title("Animation")]
        [Min(0f)]
        [SerializeField] private float powerFadeDuration = 0.2f;

        [Min(0f)]
        [SerializeField] private float pulseDuration = 1.25f;

        [Range(0f, 0.5f)]
        [SerializeField] private float pulseDepth = 0.08f;


        private static readonly int TopColorId = Shader.PropertyToID("_Top_Color");
        private static readonly int BottomColorId = Shader.PropertyToID("_Bottom_Color");



        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly int EmissionIntensityId = Shader.PropertyToID("_EmissionIntensity");
        private static readonly int IsEmissiveBoolId = Shader.PropertyToID("_IsEmissive");

        private MaterialPropertyBlock glowMpb;
        private MaterialPropertyBlock mainMpb;

        private Tween glowTween;
        private float currentIntensity;



        [Button]
        public void SetupPiece(Mesh mainMesh, Mesh glowMesh)
        {
            mainMeshFilter.mesh = mainMesh;
            glowMeshFilter.mesh = glowMesh;
            gameObject.SetActive(true);

            EnsureMpbs();
            KillGlowTween();

            if (glowMeshRenderer)
            {
                glowMeshRenderer.GetPropertyBlock(glowMpb);
                glowMpb.SetFloat(IsEmissiveBoolId, 1f);
                glowMpb.SetColor(EmissionColorId, emissionBaseColor);
                glowMeshRenderer.SetPropertyBlock(glowMpb);
            }

            SetGlowIntensity(unpoweredIntensity);
        }

        [Button]
        public void PiecePowerRoutine(bool isPowered, bool instant = false)
        {
            EnsureMpbs();
            if (isPowered)
                SetPowered(powered: true, animated: !instant, pulsing: !instant);
            else
                SetPowered(powered: false, animated: !instant, pulsing: false);
        }


        public void RequestVFX()
        {
            vfxItem.Play();
        }

        public void UpdateTheme(Color topColor, Color bottomColor, Color glowColor)
        {
            EnsureMpbs();

            if (mainMeshRenderer)
            {
                mainMeshRenderer.GetPropertyBlock(mainMpb);
                mainMpb.SetColor(TopColorId, topColor);
                mainMpb.SetColor(BottomColorId, bottomColor);
                mainMeshRenderer.SetPropertyBlock(mainMpb);
            }

            if (glowMeshRenderer)
            {
                glowMeshRenderer.GetPropertyBlock(glowMpb);

                glowMpb.SetColor(TopColorId, topColor);
                glowMpb.SetColor(BottomColorId, bottomColor);

                emissionBaseColor = glowColor;

                glowMpb.SetFloat(IsEmissiveBoolId, 1f);
                glowMpb.SetColor(EmissionColorId, emissionBaseColor); // <-- NOVO (antes tava comentado)

                glowMeshRenderer.SetPropertyBlock(glowMpb);

                ApplyGlowIntensity(currentIntensity); // <-- NOVO: reaplica intensidade depois do theme
            }
        }

        [Button]
        private void PowerOnInstant()
        {
            EnsureMpbs();
            SetPowered(powered: true, animated: false, pulsing: false);
        }

        [Button]
        private void PowerOnAnimated()
        {
            EnsureMpbs();
            SetPowered(powered: true, animated: true, pulsing: false);
        }

        [Button]
        private void PowerOnAnimatedPulse()
        {
            EnsureMpbs();
            SetPowered(powered: true, animated: true, pulsing: true);
        }

        [Button]
        private void PowerOffInstant()
        {
            EnsureMpbs();
            SetPowered(powered: false, animated: false, pulsing: false);
        }

        [Button]
        private void PowerOffAnimated()
        {
            EnsureMpbs();
            SetPowered(powered: false, animated: true, pulsing: false);
        }

        private void EnsureMpbs()
        {
            if (glowMpb == null)
                glowMpb = new MaterialPropertyBlock();
            if (mainMpb == null)
                mainMpb = new MaterialPropertyBlock();

            if (glowMpb == null)
                Debug.LogError($"glow mpb on {gameObject.name} is null");
            if (mainMpb == null)
                Debug.LogError($"main mpb on {gameObject.name} is null");
        }

        private void ApplyGlowIntensity(float intensity)
        {
            if (!glowMeshRenderer)
                return;

            glowMeshRenderer.GetPropertyBlock(glowMpb);

            glowMpb.SetFloat(IsEmissiveBoolId, intensity > 0.0001f ? 1f : 0f); // <-- NOVO
            glowMpb.SetFloat(EmissionIntensityId, intensity);

            glowMeshRenderer.SetPropertyBlock(glowMpb);
        }

        private void SetPowered(bool powered, bool animated, bool pulsing)
        {
            KillGlowTween();

            float target = powered ? poweredIntensity : unpoweredIntensity;

            if (!animated)
            {
                SetGlowIntensity(target);
                return;
            }

            glowTween = DOTween
                .To(() => currentIntensity, SetGlowIntensity, target, powerFadeDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    if (!powered || !pulsing)
                        return;

                    float min = poweredIntensity * (1f - pulseDepth);

                    glowTween = DOTween
                        .To(() => currentIntensity, SetGlowIntensity, min, pulseDuration)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo);
                });
        }

        private void SetGlowIntensity(float intensity)
        {
            currentIntensity = intensity;
            ApplyGlowIntensity(intensity);
        }

        private void KillGlowTween()
        {
            if (glowTween != null && glowTween.IsActive())
                glowTween.Kill();

            glowTween = null;
        }

        private void OnDisable()
        {
            KillGlowTween();
        }

        private void Reset()
        {
            if (mainMeshFilter) mainMeshFilter.mesh = null;
            if (glowMeshFilter) glowMeshFilter.mesh = null;
        }

#if UNITY_EDITOR
        // don't want the view to know about the data so this is just for convenience
        [Button]
        private void SetupPiece(Gameplay.Core.Data.PieceSO data)
        {
            mainMeshFilter.mesh = data.MainMesh;
            glowMeshFilter.mesh = data.GlowMesh;
            gameObject.SetActive(true);
            EnsureMpbs();
            KillGlowTween();
            SetGlowIntensity(unpoweredIntensity);
        }
#endif
    }
}
