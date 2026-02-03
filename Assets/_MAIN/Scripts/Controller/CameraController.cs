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
        [SerializeField, BoxGroup("References")]
        private Renderer groundRenderer;
        private Transform cameraTransform;

        [SerializeField, BoxGroup("Animation")]
        private float nearClearance = 0.5f;

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

            GetCameraTarget(MaxGridSize, MaxGridSize, CellSize, GridOrigin, out Vector3 targetPos, out float targetSize, out Quaternion targetRot);

            cameraTransform.SetPositionAndRotation(targetPos, targetRot);
            mainCamera.orthographicSize = targetSize;
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

            GetCameraTarget(currentLevelData.Width, currentLevelData.Height, currentCellSize, currentGridOrigin,
                out Vector3 targetPos, out float targetSize, out Quaternion targetRot);

            if (isPaused)
            {
                targetSize *= pauseZoomMultiplier;
            }


            cameraTransform.DOKill();
            mainCamera.DOKill();

            if (animated && Application.isPlaying)
            {
                cameraTransform.DOMove(targetPos, zoomDuration).SetEase(zoomEase).SetUpdate(true); // SetUpdate(true) ignora Time.timeScale = 0
                mainCamera.DOOrthoSize(targetSize, zoomDuration).SetEase(zoomEase).SetUpdate(true);
            }
            else
            {
                cameraTransform.position = targetPos;
                cameraTransform.LookAt(targetPos - offset);
                mainCamera.orthographicSize = targetSize;
            }
        }

        private void RefreshCameraPosition()
        {
            if (Application.isPlaying && currentLevelData != null)
            {
                UpdateCameraState(false);
            }
        }

        /// <summary>Fits the board in ortho and pushes the camera back to avoid near-plane clipping with no framing change(or minimal).</summary>
        private void GetCameraTarget(int width, int height, float cellSize, Vector3 gridOrigin, out Vector3 position, out float size, out Quaternion rotation)
        {
            float halfCell = cellSize * 0.5f;

            float centerX = (width - 1) * cellSize * 0.5f;
            float centerZ = (height - 1) * cellSize * 0.5f;

            Vector3 center = gridOrigin + new Vector3(centerX, 0f, centerZ);

            position = center + offset;
            rotation = Quaternion.LookRotation((center - position).normalized, Vector3.up);

            Vector3 c0 = gridOrigin + new Vector3(-halfCell, 0f, -halfCell);
            Vector3 c1 = gridOrigin + new Vector3((width - 1) * cellSize + halfCell, 0f, -halfCell);
            Vector3 c2 = gridOrigin + new Vector3(-halfCell, 0f, (height - 1) * cellSize + halfCell);
            Vector3 c3 = gridOrigin + new Vector3((width - 1) * cellSize + halfCell, 0f, (height - 1) * cellSize + halfCell);

            Matrix4x4 view = Matrix4x4.TRS(position, rotation, Vector3.one).inverse;

            Vector3 p0 = view.MultiplyPoint3x4(c0);
            Vector3 p1 = view.MultiplyPoint3x4(c1);
            Vector3 p2 = view.MultiplyPoint3x4(c2);
            Vector3 p3 = view.MultiplyPoint3x4(c3);

            float minX = Mathf.Min(p0.x, p1.x, p2.x, p3.x);
            float maxX = Mathf.Max(p0.x, p1.x, p2.x, p3.x);
            float minY = Mathf.Min(p0.y, p1.y, p2.y, p3.y);
            float maxY = Mathf.Max(p0.y, p1.y, p2.y, p3.y);

            float halfWidth = (maxX - minX) * 0.5f;
            float halfHeight = (maxY - minY) * 0.5f;

            float aspect = mainCamera.aspect;

            size = Mathf.Max(halfHeight, halfWidth / aspect) + padding;


            // GridOrigin is not the rectangle corner in this project
            // It is the center of the first cell at (0,0)
            // Because of that the true center is based on (width - 1) and (height - 1) not width and height
            // We also build the 4 world corners using a half cell margin so the whole board fits
            // Then we convert those corners into camera view space and measure min max bounds
            // Finally we pick the larger half extent between height and width divided by aspect and add padding

            // Thanks ChatGPT ♪

            float nearTarget = mainCamera.nearClipPlane + nearClearance;

            Matrix4x4 viewDepth = Matrix4x4.TRS(position, rotation, Vector3.one).inverse;

            float minZ = float.PositiveInfinity;

            void ConsiderPoint(Vector3 worldPoint)
            {
                float z = viewDepth.MultiplyPoint3x4(worldPoint).z;
                if (z < minZ) minZ = z;
            }

            // gets grid all 4 corners
            ConsiderPoint(c0);
            ConsiderPoint(c1);
            ConsiderPoint(c2);
            ConsiderPoint(c3);

            // all 8 corners from the floor cube (TODO:Change this to a plane check)
            if (groundRenderer != null)
            {
                Bounds b = groundRenderer.bounds;
                Vector3 e = b.extents;
                Vector3 o = b.center;

                ConsiderPoint(o + new Vector3(e.x, e.y, e.z));
                ConsiderPoint(o + new Vector3(e.x, e.y, -e.z));
                ConsiderPoint(o + new Vector3(e.x, -e.y, e.z));
                ConsiderPoint(o + new Vector3(e.x, -e.y, -e.z));
                ConsiderPoint(o + new Vector3(-e.x, e.y, e.z));
                ConsiderPoint(o + new Vector3(-e.x, e.y, -e.z));
                ConsiderPoint(o + new Vector3(-e.x, -e.y, e.z));
                ConsiderPoint(o + new Vector3(-e.x, -e.y, -e.z));
            }
            //if something is before the near clip, pushes the camera away
            float pushBack = nearTarget - minZ;
            if (pushBack > 0f)
            {
                Vector3 forward = rotation * Vector3.forward;
                position -= forward * pushBack;
            }

        }
    }
}