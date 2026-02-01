using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.Core;
using Gameplay.Core.Data;
using Gameplay.Core.Events;
using Gameplay.Views;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;

using System.Threading;
using UnityEngine;

namespace Gameplay.Core.Controllers
{
    public class LevelController : MonoBehaviour
    {
        //[ReadOnly,InlineEditor]
        [BoxGroup("References")]
        [SerializeField] private LevelDataSO CurrentLevelData;
        [Required, SceneObjectsOnly, BoxGroup("References")]
        [SerializeField] private CameraController CameraController;


        [SerializeField, TabGroup("Grid Configuration")]
        private float CellSize = 1.5f;
        [SerializeField, TabGroup("Grid Configuration")]
        private Transform GridOrigin;
        //[SerializeField, TabGroup("Grid Configuration")]
        //private int MaxGridSize = 10;

        [SerializeField, TabGroup("Pool")]
        private int InitialPoolSize = 50;

        [Required, AssetsOnly, BoxGroup("References")]
        [SerializeField] private NodeView NodePrefab;
        [Required, SceneObjectsOnly, BoxGroup("References")]
        [SerializeField] private Transform PiecesContainer;

        // O GridModel só conterá peças lógicas (Wire, Source, Lamp). 
        // Misc e Vazios serão nulos aqui.
        private NodeModel[,] GridModel;

        [ShowInInspector, ReadOnly, FoldoutGroup("Views List")]
        private readonly List<NodeView> ActiveViews = new();

        [ShowInInspector, ReadOnly, FoldoutGroup("Views List")]
        private readonly List<NodeView> MiscViews = new();

        [ShowInInspector, ReadOnly, FoldoutGroup("Views List")]
        private readonly List<NodeView> DummyViews = new();

        [ShowInInspector, ReadOnly, FoldoutGroup("Views List")]
        private List<NodeView> NodePool = new();

        private CancellationTokenSource revealCts;
        private System.Diagnostics.Stopwatch stopwatch = new();

        private void Awake()
        {
            InitializePool();
        }
        [Button]
        private void StartLevel()
        {
            if (CurrentLevelData != null)
            {
                LoadLevel(CurrentLevelData);
            }
        }

        private void OnDestroy()
        {
            revealCts?.Cancel();
            revealCts?.Dispose();
        }


        private void InitializePool()
        {
            stopwatch.Start();

            foreach (Transform child in PiecesContainer)
            {
                if (child.TryGetComponent(out NodeView node))
                {
                    node.gameObject.SetActive(false);
                    NodePool.Add(node);
                }
            }
            if (NodePool.Count < InitialPoolSize)
            {
                ExpandPool(InitialPoolSize - NodePool.Count);
            }
            stopwatch.Stop();
            Debug.Log($"Pool init duration: {stopwatch.Elapsed.TotalSeconds}");

        }

        [Button("Load Current Level")]
        public void LoadLevel(LevelDataSO levelData)
        {
            stopwatch.Restart();
            stopwatch.Start();
            CurrentLevelData = levelData;

            // Define o tamanho máximo baseado no VisualGridSize (para incluir o cenário de fundo)
            int MaxGridSize = levelData.VisualGridSize;

            revealCts?.Cancel();
            revealCts?.Dispose();
            revealCts = new CancellationTokenSource();

            ResetLevelState();
            GenerateFullGrid();

            if (CameraController != null)
            {
                CameraController.Setup(MaxGridSize, CellSize, GridOrigin.position);
            }

            RevealLevelRoutine(revealCts.Token).Forget();
            stopwatch.Stop();
            Debug.Log($"Load Level duration: {stopwatch.Elapsed.TotalSeconds}");

        }

        private void ResetLevelState()
        {
            foreach (var Node in NodePool)
            {
                Node.gameObject.SetActive(false);
                Node.transform.DOKill();
            }
            ActiveViews.Clear();
            DummyViews.Clear();
            MiscViews.Clear();
            GridModel = null;
        }

        private void GenerateFullGrid()
        {
            GridModel = new NodeModel[CurrentLevelData.Width, CurrentLevelData.Height];

            int MaxGridSize = Mathf.Max(CurrentLevelData.VisualGridSize, CurrentLevelData.Width, CurrentLevelData.Height);
            int TotalRequired = MaxGridSize * MaxGridSize;

            if (NodePool.Count < TotalRequired)
                ExpandPool(TotalRequired - NodePool.Count);

            int PoolIndex = 0;

            for (int X = 0; X < MaxGridSize; X++)
            {
                for (int Y = 0; Y < MaxGridSize; Y++)
                {
                    float xPos = X * CellSize;
                    float yPos = Y * CellSize;
                    Vector3 Position = GridOrigin.position + new Vector3(xPos, 0, yPos);

                    NodeView ViewInstance = NodePool[PoolIndex];
                    ViewInstance.gameObject.SetActive(true);
                    ViewInstance.transform.position = Position;
                    ViewInstance.gameObject.name = $"X:{X},Y:{Y}";
                    PoolIndex++;

                    bool isInside = IsInsideLevelBounds(X, Y);

                    if (isInside && CurrentLevelData.Layout[X, Y] != null)
                    {

                        PieceSO pieceData = CurrentLevelData.Layout[X, Y];

                        if (pieceData.PieceType == PieceType.Misc)
                        {
                            SetupMiscNode(ViewInstance);
                        }
                        else
                        {
                            SetupRealNode(X, Y, ViewInstance, pieceData);
                        }
                    }
                    else
                    {
                        SetupDummyNode(ViewInstance);
                    }
                }
            }
        }

        private void ExpandPool(int Amount)
        {
            for (int i = 0; i < Amount; i++)
            {
                NodeView NewNode = Instantiate(NodePrefab, PiecesContainer);
                NewNode.gameObject.SetActive(false);
                NodePool.Add(NewNode);
            }
        }

        private void SetupRealNode(int x, int y, NodeView view, PieceSO data)
        {
            GridModel[x, y] = new NodeModel(x, y, data, 0);
            view.Setup(x, y, data.Icon);
            ActiveViews.Add(view);
        }
        private void SetupMiscNode(NodeView view)
        {
            view.Setup(-1, -1, null);
            view.SetAsMisc();
            MiscViews.Add(view);
        }
        private void SetupDummyNode(NodeView view)
        {
            view.Setup(-1, -1, null);
            view.SetAsDummy();
            DummyViews.Add(view);
        }

        private bool IsInsideLevelBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < CurrentLevelData.Width && y < CurrentLevelData.Height;
        }

        private async UniTaskVoid RevealLevelRoutine(CancellationToken token)
        {
            await UniTask.Delay(500, cancellationToken: token);

            if (CameraController != null)
            {
                CameraController.FocusOnLevel(CurrentLevelData, CellSize, GridOrigin.position);
            }

            foreach (NodeView Dummy in DummyViews)
            {
                Dummy.transform.DOScale(Vector3.zero, 0.5f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => Dummy.gameObject.SetActive(false));
            }

            RecalculateFlow();
            UpdateAllViews();
        }

        public void OnNodeInteraction(int x, int y)
        {
            if (GridModel == null || !IsInsideLevelBounds(x, y) || GridModel[x, y] == null) return;

            GridModel[x, y].Rotate();
            RecalculateFlow();
            UpdateAllViews();
            CheckWinCondition();
        }

        private void RecalculateFlow()
        {
            foreach (var Node in GridModel) if (Node != null) Node.IsPowered = false;

            Queue<NodeModel> Queue = new();

            for (int X = 0; X < CurrentLevelData.Width; X++)
            {
                for (int Y = 0; Y < CurrentLevelData.Height; Y++)
                {
                    var Node = GridModel[X, Y];
                    if (Node != null && Node.PieceType == PieceType.Source)
                    {
                        Node.IsPowered = true;
                        Queue.Enqueue(Node);
                    }
                }
            }

            while (Queue.Count > 0)
            {
                NodeModel Current = Queue.Dequeue();
                CheckNeighbor(Current, 0, 1, Direction.Up, Direction.Down, Queue);
                CheckNeighbor(Current, 1, 0, Direction.Right, Direction.Left, Queue);
                CheckNeighbor(Current, 0, -1, Direction.Down, Direction.Up, Queue);
                CheckNeighbor(Current, -1, 0, Direction.Left, Direction.Right, Queue);
            }
        }

        private void CheckNeighbor(NodeModel current, int offsetX, int offsetY, Direction outDir, Direction inDir, Queue<NodeModel> queue)
        {
            if ((current.GetCurrentConnections() & outDir) == 0) return;

            int TargetX = current.X + offsetX;
            int TargetY = current.Y + offsetY;

            if (IsInsideLevelBounds(TargetX, TargetY))
            {
                NodeModel Neighbor = GridModel[TargetX, TargetY];
                if (Neighbor == null || Neighbor.IsPowered) return;

                if ((Neighbor.GetCurrentConnections() & inDir) != 0)
                {
                    Neighbor.IsPowered = true;
                    queue.Enqueue(Neighbor);
                }
            }
        }

        private void UpdateAllViews()
        {
            foreach (var View in ActiveViews)
            {
                NodeModel Model = GridModel[View.XPosition, View.YPosition];
                View.UpdateVisuals(Model.RotationIndex, Model.IsPowered);
            }
        }

        private void CheckWinCondition()
        {
            bool Win = true;
            foreach (NodeModel Node in GridModel)
            {
                if (Node != null && Node.PieceType == PieceType.Lamp && !Node.IsPowered)
                {
                    Win = false;
                    break;
                }
            }

            if (Win)
            {
                Debug.LogWarning("yipieee");
                EventBus<LevelCompletedEvent>.Raise(new LevelCompletedEvent());
            }
        }
    }
}