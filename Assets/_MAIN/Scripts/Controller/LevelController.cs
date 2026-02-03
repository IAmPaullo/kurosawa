using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.Core;
using Gameplay.Core.Data;
using Gameplay.Core.Events;
using Gameplay.Views;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Gameplay.Core.Controllers
{
    public class LevelController : MonoBehaviour
    {
        [BoxGroup("References")]
        [SerializeField] private LevelDataSO CurrentLevelData;
        [Required, SceneObjectsOnly, BoxGroup("References")]
        [SerializeField] private CameraController CameraController;

        [SerializeField, TabGroup("Grid Configuration")]
        private float CellSize = 1.5f;
        [SerializeField, TabGroup("Grid Configuration")]
        private Transform GridOrigin;
        [SerializeField, TabGroup("Grid Configuration")]
        private Vector3 PiecesContainerOffset = new(-1.2f, 0, 0);

        [SerializeField, TabGroup("Pool")]
        private int InitialPoolSize = 50;

        [Required, AssetsOnly, BoxGroup("References")]
        [SerializeField] private NodeView NodePrefab;
        [Required, SceneObjectsOnly, BoxGroup("References")]
        [SerializeField] private Transform PiecesContainer;

        private NodeModel[,] GridModel;
        private bool IsInputActive = false;

        [ShowInInspector, ReadOnly, FoldoutGroup("Views List")]
        private readonly List<NodeView> ActiveViews = new();

        [ShowInInspector, ReadOnly, FoldoutGroup("Views List")]
        private readonly List<NodeView> MiscViews = new();

        [ShowInInspector, ReadOnly, FoldoutGroup("Views List")]
        private readonly List<NodeView> DummyViews = new();

        [ShowInInspector, ReadOnly, FoldoutGroup("Views List")]
        private List<NodeView> NodePool = new();

        private CancellationTokenSource revealCts;
        private Queue<NodeModel> flowQueue;

        private int currentVisualGridSize;
        private int levelStartX;
        private int levelStartY;
        private Vector3 levelWorldOrigin;

        private EventBinding<MatchPrepareEvent> matchPrepareBind;
        private EventBinding<MatchStartEvent> matchStartBind;
        private EventBinding<MatchEndEvent> matchEndBind;

        private void Awake()
        {
            InitializePool();
        }

        private void OnEnable()
        {
            matchPrepareBind = new(OnMatchPrepare);
            matchStartBind = new(OnMatchStart);
            matchEndBind = new(OnMatchEnd);

            EventBus<MatchPrepareEvent>.Register(matchPrepareBind);
            EventBus<MatchStartEvent>.Register(matchStartBind);
            EventBus<MatchEndEvent>.Register(matchEndBind);
        }

        private void OnDisable()
        {
            EventBus<MatchPrepareEvent>.Deregister(matchPrepareBind);
            EventBus<MatchStartEvent>.Deregister(matchStartBind);
            EventBus<MatchEndEvent>.Deregister(matchEndBind);

            revealCts?.Cancel();
            revealCts?.Dispose();
        }

        private void InitializePool()
        {
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
        }

        private void OnMatchPrepare(MatchPrepareEvent evt)
        {
            IsInputActive = false;
            CurrentLevelData = evt.LevelData;

            flowQueue = new Queue<NodeModel>(CurrentLevelData.Width * CurrentLevelData.Height);

            revealCts?.Cancel();
            revealCts?.Dispose();
            revealCts = new CancellationTokenSource();

            ResetLevelState();

            // Mantém sua intenção original: grid visual é baseado no VisualGridSize
            int maxGridSize = CurrentLevelData.VisualGridSize;

            // ADICIONADO: garante que cabe e centraliza o level dentro desse grid visual
            currentVisualGridSize = Mathf.Max(maxGridSize, CurrentLevelData.Width, CurrentLevelData.Height);

            levelStartX = Mathf.Max(0, (currentVisualGridSize - CurrentLevelData.Width) / 2);
            levelStartY = Mathf.Max(0, (currentVisualGridSize - CurrentLevelData.Height) / 2);

            levelWorldOrigin = GridOrigin.position + new Vector3(levelStartX * CellSize, 0f, levelStartY * CellSize);

            GenerateFullGrid();
            PiecesContainer.position = PiecesContainerOffset;

            if (CameraController != null)
            {
                // Mantém sua lógica: no prepare, enquadra o grid visual inteiro
                CameraController.Setup(currentVisualGridSize, CellSize, GridOrigin.position);
            }
        }

        private void OnMatchStart(MatchStartEvent evt)
        {
            RevealLevelRoutine(revealCts.Token).Forget();
        }

        private void OnMatchEnd(MatchEndEvent evt)
        {
            IsInputActive = false;
        }

#if UNITY_EDITOR
        [Button("Debug Manual Load")]
        public void LoadLevel(LevelDataSO levelData)
        {
            OnMatchPrepare(new MatchPrepareEvent { LevelData = levelData });
            OnMatchStart(new MatchStartEvent());
        }
#endif
        private void ResetLevelState()
        {
            foreach (var Node in NodePool)
            {
                Node.gameObject.SetActive(false);
                Node.transform.DOKill();
                Node.transform.localScale = Vector3.one;
            }
            ActiveViews.Clear();
            DummyViews.Clear();
            MiscViews.Clear();
            GridModel = null;
        }

        private void GenerateFullGrid()
        {
            GridModel = new NodeModel[CurrentLevelData.Width, CurrentLevelData.Height];

            int maxGridSize = currentVisualGridSize;
            int totalRequired = maxGridSize * maxGridSize;

            if (NodePool.Count < totalRequired)
                ExpandPool(totalRequired - NodePool.Count);

            int PoolIndex = 0;

            for (int X = 0; X < maxGridSize; X++)
            {
                for (int Y = 0; Y < maxGridSize; Y++)
                {
                    float xPos = X * CellSize;
                    float yPos = Y * CellSize;
                    Vector3 Position = GridOrigin.position + new Vector3(xPos, 0, yPos);

                    NodeView ViewInstance = NodePool[PoolIndex];
                    ViewInstance.gameObject.SetActive(true);
                    ViewInstance.transform.position = Position;


                    int levelX = X - levelStartX;
                    int levelY = Y - levelStartY;

                    ViewInstance.gameObject.name = $"X:{X},Y:{Y}";

                    PoolIndex++;

                    bool isInsideLevel = IsInsideLevelBounds(levelX, levelY);

                    if (isInsideLevel && CurrentLevelData.Layout[levelX, levelY] != null)
                    {
                        PieceSO pieceData = CurrentLevelData.Layout[levelX, levelY];

                        if (pieceData.PieceType == PieceType.Misc)
                        {
                            SetupMiscNode(ViewInstance);
                        }
                        else
                        {
                            SetupRealNode(levelX, levelY, ViewInstance, pieceData);
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
            view.Setup(x, y, data.MainMesh, data.GlowMesh);
            ActiveViews.Add(view);
        }

        private void SetupMiscNode(NodeView view)
        {
            view.Setup(-1, -1, null, null);
            view.SetAsMisc();
            MiscViews.Add(view);
        }

        private void SetupDummyNode(NodeView view)
        {
            view.Setup(-1, -1, null, null);
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
                CameraController.FocusOnLevel(CurrentLevelData, CellSize, levelWorldOrigin);
            }

            foreach (NodeView Dummy in DummyViews)
            {
                Dummy.transform.DOScale(Vector3.zero, 0.5f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => Dummy.gameObject.SetActive(false));
            }

            RecalculateFlow();
            UpdateAllViews();

            IsInputActive = true;
        }

        public void OnNodeInteraction(int x, int y)
        {
            if (!IsInputActive) return;
            if (GridModel == null || !IsInsideLevelBounds(x, y) || GridModel[x, y] == null) return;

            GridModel[x, y].Rotate();
            RecalculateFlow();
            UpdateAllViews();
            CheckWinCondition();
        }

        private void RecalculateFlow()
        {
            int width = CurrentLevelData.Width;
            int height = CurrentLevelData.Height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var node = GridModel[x, y];
                    if (node != null) node.IsPowered = false;
                }
            }

            flowQueue.Clear();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var node = GridModel[x, y];
                    if (node != null && node.PieceType == PieceType.Source)
                    {
                        node.IsPowered = true;
                        flowQueue.Enqueue(node);
                    }
                }
            }

            while (flowQueue.Count > 0)
            {
                var current = flowQueue.Dequeue();
                CheckNeighbor(current, 0, 1, Direction.Up, Direction.Down, flowQueue);
                CheckNeighbor(current, 1, 0, Direction.Right, Direction.Left, flowQueue);
                CheckNeighbor(current, 0, -1, Direction.Down, Direction.Up, flowQueue);
                CheckNeighbor(current, -1, 0, Direction.Left, Direction.Right, flowQueue);
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
            int width = CurrentLevelData.Width;
            int height = CurrentLevelData.Height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var node = GridModel[x, y];
                    if (node != null && node.PieceType == PieceType.Lamp && !node.IsPowered)
                        return;
                }
            }

            Debug.Log("LevelCompletedEvent Raised. Finishing match...");
            EventBus<LevelCompletedEvent>.Raise(new LevelCompletedEvent());
        }
    }
}