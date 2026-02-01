using DG.Tweening;
using Gameplay.Core.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Core.Controllers
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField, BoxGroup("References")]
        private Camera mainCamera;
        private Transform cameraTransform;

        [SerializeField, BoxGroup("Animation"), OnValueChanged(nameof(RefreshCameraPosition))]
        private float padding = 2.0f;

        [SerializeField, BoxGroup("Animation"), OnValueChanged(nameof(RefreshCameraPosition))]
        private Vector3 offset = new(-20, 20, -20);

        [SerializeField, BoxGroup("Animation")]
        private float zoomDuration = 1.5f;

        [SerializeField, BoxGroup("Animation")]
        private Ease zoomEase = Ease.InOutCubic;


        [SerializeField, BoxGroup("Animation"), Range(1f, 2f), OnValueChanged(nameof(RefreshCameraPosition))]
        private float pauseZoomMultiplier = 1.2f;


        private LevelDataSO currentLevelData; //TODO: trocar por uma bool msm?
        private float currentCellSize;
        private Vector3 currentGridOrigin;
        private bool isPaused = false;

        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            mainCamera.orthographic = true;
            cameraTransform = mainCamera.transform;
        }


        public void Setup(int MaxGridSize, float CellSize, Vector3 GridOrigin)
        {
            cameraTransform = mainCamera.transform;

            GetCameraTarget(MaxGridSize, MaxGridSize, CellSize, GridOrigin, out Vector3 TargetPos, out float TargetSize);

            cameraTransform.position = TargetPos;
            cameraTransform.LookAt(TargetPos - offset);
            mainCamera.orthographicSize = TargetSize;
        }

        public void FocusOnLevel(LevelDataSO LevelData, float CellSize, Vector3 GridOrigin)
        {
            if (LevelData == null) return;

            currentLevelData = LevelData;
            currentCellSize = CellSize;
            currentGridOrigin = GridOrigin;
            isPaused = false;

            UpdateCameraState(true);
        }
        [Button]
        public void SetPaused(bool paused)
        {
            if (currentLevelData == null) return;

            if (isPaused != paused)
            {
                isPaused = paused;
                UpdateCameraState(true);
            }
        }


        private void UpdateCameraState(bool animated)
        {
            if (currentLevelData == null) return;

            GetCameraTarget(currentLevelData.Width, currentLevelData.Height, currentCellSize, currentGridOrigin, out Vector3 TargetPos, out float TargetSize);


            if (isPaused)
            {
                TargetSize *= pauseZoomMultiplier;
            }


            cameraTransform.DOKill();
            mainCamera.DOKill();

            if (animated && Application.isPlaying)
            {
                cameraTransform.DOMove(TargetPos, zoomDuration).SetEase(zoomEase).SetUpdate(true); // SetUpdate(true) ignora Time.timeScale = 0
                mainCamera.DOOrthoSize(TargetSize, zoomDuration).SetEase(zoomEase).SetUpdate(true);
            }
            else
            {
                cameraTransform.position = TargetPos;
                cameraTransform.LookAt(TargetPos - offset);
                mainCamera.orthographicSize = TargetSize;
            }
        }

        private void RefreshCameraPosition()
        {
            if (Application.isPlaying && currentLevelData != null)
            {
                UpdateCameraState(false);
            }
        }

        private void GetCameraTarget(int Width, int Height, float CellSize, Vector3 GridOrigin, out Vector3 Position, out float Size)
        {
            float WorldWidth = Width * CellSize;
            float WorldHeight = Height * CellSize;

            float CenterX = (WorldWidth - CellSize) / 2f;
            float CenterZ = (WorldHeight - CellSize) / 2f;

            Vector3 Center = GridOrigin + new Vector3(CenterX, 0, CenterZ);
            Position = Center + offset;

            float BoundsHeight = WorldHeight + padding;
            float BoundsWidth = WorldWidth + padding;

            float ScreenRatio = (float)Screen.width / Screen.height;
            float TargetRatio = BoundsWidth / BoundsHeight;

            if (ScreenRatio >= TargetRatio)
            {
                Size = BoundsHeight / 2f;
            }
            else
            {
                Size = (BoundsWidth / ScreenRatio) / 2f;
            }
        }
    }
}