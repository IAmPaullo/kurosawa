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
        [Title("Data Source")]
        //[ReadOnly,InlineEditor]
        public LevelDataSO CurrentLevelData;


        [SerializeField, TabGroup("Grid Configuration")]
        private float CellSize = 1.5f;
        [SerializeField, TabGroup("Grid Configuration")]
        private Transform GridOrigin;
        [SerializeField, TabGroup("Grid Configuration")]
        private int MaxGridSize = 10;

        [Required, AssetsOnly, BoxGroup("References")]
        [SerializeField] private NodeView NodePrefab;
        [Required, SceneObjectsOnly, BoxGroup("References")]
        [SerializeField] private Transform PiecesContainer;


        private NodeModel[,] GridModel;
        private readonly List<NodeView> ActiveViews = new ();
        private readonly List<GameObject> DummyViews = new ();

        private CancellationTokenSource _revealCts;


        private void Start()
        {
            if (CurrentLevelData != null)
            {
                LoadLevel(CurrentLevelData);
            }
        }

        private void OnDestroy()
        {
            _revealCts?.Cancel();
            _revealCts?.Dispose();
        }

        [Button("Load Current Level")]
        public void LoadLevel(LevelDataSO levelData)
        {
            CurrentLevelData = levelData;

            _revealCts?.Cancel();
            _revealCts?.Dispose();
            _revealCts = new CancellationTokenSource();

            ClearCurrentLevel();

            GenerateFullGrid();

            RevealLevelRoutine(_revealCts.Token).Forget();
        }

        private void ClearCurrentLevel()
        {
            foreach (Transform Child in PiecesContainer)
                Destroy(Child.gameObject);
            ActiveViews.Clear();
            DummyViews.Clear();
            GridModel = null;
        }

        private void GenerateFullGrid()
        {

            GridModel = new NodeModel[CurrentLevelData.Width, CurrentLevelData.Height];

            for (int X = 0; X < MaxGridSize; X++)
            {
                for (int Y = 0; Y < MaxGridSize; Y++)
                {
                    float xPosition = X * CellSize;
                    float yPosition = Y * CellSize;
                    Vector3 Position = GridOrigin.position + new Vector3(xPosition, 0, yPosition);

                    
                    if (IsInsideLevelBounds(X, Y) && CurrentLevelData.Layout[X, Y] != null)
                    {
                        SpawnRealNode(X, Y, Position);
                    }
                    else
                    {
                        SpawnDummyNode(Position);
                    }
                }
            }
        }

        private bool IsInsideLevelBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < CurrentLevelData.Width && y < CurrentLevelData.Height;
        }

        private void SpawnRealNode(int x, int y, Vector3 position)
        {
            PieceSO Definition = CurrentLevelData.Layout[x, y];


            GridModel[x, y] = new NodeModel(x, y, Definition, 0);


            NodeView ViewInstance = Instantiate(NodePrefab, position, Quaternion.identity, PiecesContainer);
            ViewInstance.Setup(x, y, Definition.Icon);

            ActiveViews.Add(ViewInstance);
        }

        private void SpawnDummyNode(Vector3 position)
        {
            //TODO: trocar pra um node prefab dummy de vdd ou uma pool?
            NodeView Dummy = Instantiate(NodePrefab, position, Quaternion.identity, PiecesContainer);

            Dummy.UpdateVisuals(0, false, true);
            Dummy.SetAsDummy();

            DummyViews.Add(Dummy.gameObject);
        }

        private async UniTaskVoid RevealLevelRoutine(CancellationToken token)
        {
            
            await UniTask.Delay(2000, cancellationToken: token);

            
            foreach (var Dummy in DummyViews)
            {
                
                Dummy.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).OnComplete(() => Destroy(Dummy));
            }
            DummyViews.Clear();


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
                    if (Node != null && Node.Type == PieceType.Source)
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
            foreach (var Node in GridModel)
            {
                if (Node != null && Node.Type == PieceType.Lamp && !Node.IsPowered)
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
