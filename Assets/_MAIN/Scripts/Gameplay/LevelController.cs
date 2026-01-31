using Gameplay.Core;
using Gameplay.Core.Data;
using Gameplay.Views;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Controllers
{
    public class LevelController : MonoBehaviour
    {
        [Title("Data Source")]
        //[ReadOnly]
        public LevelDataSO CurrentLevelData;


        [SerializeField, TabGroup("Grid Configuration")]
        private float CellSize = 1.5f;
        [SerializeField, TabGroup("Grid Configuration")]
        private Transform GridOrigin;

        [Required, AssetsOnly, BoxGroup("References")]
        public NodeView NodePrefab;
        [Required, SceneObjectsOnly, BoxGroup("References")]
        public Transform PiecesContainer;



        private NodeModel[,] GridModel;
        private NodeView[,] GridView;

        private void Start()
        {
            if (CurrentLevelData != null)
            {
                LoadLevel(CurrentLevelData);
            }
            else
            {
                Debug.LogError("level data is null");
            }
        }


        [Button("Load Current Level")]
        public void LoadLevel(LevelDataSO levelData)
        {
            CurrentLevelData = levelData;
            ClearCurrentLevel();
            GenerateGrid();
            RecalculateFlow();
            UpdateAllViews();
        }

        private void ClearCurrentLevel()
        {
            foreach (Transform Child in PiecesContainer) Destroy(Child.gameObject);
        }

        private void GenerateGrid()
        {
            int Width = CurrentLevelData.Width;
            int Height = CurrentLevelData.Height;

            GridModel = new NodeModel[Width, Height];
            GridView = new NodeView[Width, Height];

            for (int X = 0; X < Width; X++)
            {
                for (int Y = 0; Y < Height; Y++)
                {

                    PieceSO Definition = CurrentLevelData.Layout[X, Y];

                    if (Definition == null) continue;


                    GridModel[X, Y] = new NodeModel(X, Y, Definition, 0);

                    //view create
                    Vector3 Position = GridOrigin.position + new Vector3(X * CellSize, Y * CellSize, 0);
                    NodeView ViewInstance = Instantiate(NodePrefab, Position, Quaternion.identity, PiecesContainer);

                    ViewInstance.Setup(X, Y, Definition.Icon);
                    ViewInstance.OnNodeClicked += HandleNodeInput;

                    GridView[X, Y] = ViewInstance;
                }
            }
        }

        private void HandleNodeInput(int x, int y)
        {
            GridModel[x, y].Rotate();
            RecalculateFlow();
            UpdateAllViews();
            CheckWinCondition();
        }

        private void RecalculateFlow()
        {

            foreach (var Node in GridModel)
                if (Node != null) Node.IsPowered = false;

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

            // Flood Fill
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


            if (TargetX >= 0 && TargetX < CurrentLevelData.Width && TargetY >= 0 && TargetY < CurrentLevelData.Height)
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
            for (int X = 0; X < CurrentLevelData.Width; X++)
            {
                for (int Y = 0; Y < CurrentLevelData.Height; Y++)
                {
                    if (GridModel[X, Y] != null)
                    {
                        GridView[X, Y].UpdateVisuals(GridModel[X, Y].RotationIndex, GridModel[X, Y].IsPowered);
                    }
                }
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
                Debug.Log("Level Complete");
            }
        }
    }
}
