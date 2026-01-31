using DG.Tweening;
using Gameplay.Core.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Core.Controllers
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField, BoxGroup("References")]
        private Camera MainCamera;
        private Transform cameraTransform;

        [SerializeField, BoxGroup("Animation")] private float padding = 2.0f;
        [SerializeField, BoxGroup("Animation")] private float zoomDuration = 1.5f;
        [SerializeField, BoxGroup("Animation")] private Ease zoomEase = Ease.InOutCubic;
        [SerializeField, BoxGroup("Animation")] private Vector3 offset = new(-20, 20, -20);

        private void Awake()
        {
            if (MainCamera == null)
                MainCamera = Camera.main;
            MainCamera.orthographic = true;
        }

        public void Setup(int MaxGridSize, float CellSize, Vector3 GridOrigin)
        {
            cameraTransform = MainCamera.transform;

            GetCameraTarget(MaxGridSize, MaxGridSize, CellSize, GridOrigin, out Vector3 TargetPos, out float TargetSize);

            cameraTransform.position = TargetPos;
            cameraTransform.LookAt(TargetPos - offset);
            MainCamera.orthographicSize = TargetSize;
        }

        public void FocusOnLevel(LevelDataSO LevelData, float CellSize, Vector3 GridOrigin)
        {
            if (LevelData == null) return;

            GetCameraTarget(LevelData.Width, LevelData.Height, CellSize, GridOrigin, out Vector3 TargetPos, out float TargetSize);


            cameraTransform.DOMove(TargetPos, zoomDuration).SetEase(zoomEase);
            MainCamera.DOOrthoSize(TargetSize, zoomDuration).SetEase(zoomEase);
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