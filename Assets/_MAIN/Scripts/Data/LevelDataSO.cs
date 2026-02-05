using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gameplay.Core.Data
{
    [CreateAssetMenu(fileName = "New Level", menuName = "Level/New Level Data")]
    public class LevelDataSO : SerializedScriptableObject
    {
        [Title("Scoring")]
        [SuffixLabel("seconds")]
        public float TargetTimeS = 30f;
        [SuffixLabel("seconds")]
        public float TargetTimeA = 60f;
        [SuffixLabel("seconds")]
        public float TargetTimeB = 120f;


        [Title("Level Settings")]
        [OnValueChanged("ResizeMatrix")]
        [Min(1)]
        public int Width = 4;

        [OnValueChanged("ResizeMatrix")]
        [Min(1)]
        public int Height = 4;

        [Min(2)]
        public int VisualGridSize = 10;


        [Title("Layout")]
        [TableMatrix(SquareCells = true, HorizontalTitle = "Grid Layout", DrawElementMethod = "DrawPieceElement")]
        public PieceSO[,] Layout = new PieceSO[4, 4];

        public string CalculateGrade(float timeElapsed)
        {
            if (timeElapsed <= TargetTimeS) return "S";
            if (timeElapsed <= TargetTimeA) return "A";
            if (timeElapsed <= TargetTimeB) return "B";
            return "C";
        }

#if UNITY_EDITOR
        private PieceSO DrawPieceElement(Rect rect, PieceSO value)
        {

            PieceSO Result = (PieceSO)SirenixEditorFields.UnityObjectField(rect, value, typeof(PieceSO), true);

            if (Result != null && Result.Icon != null)
            {
                Rect TextureRect = rect.Padding(5);


                Texture2D PreviewTexture = AssetPreview.GetAssetPreview(Result.Icon);

                if (PreviewTexture != null)
                {
                    GUI.DrawTexture(TextureRect, PreviewTexture, ScaleMode.ScaleToFit);
                }
            }

            return Result;
        }

        [Button]
        private void ResizeMatrix()
        {
            if (Layout == null)
            {
                Layout = new PieceSO[Width, Height];
                return;
            }

            if (Layout.GetLength(0) == Width && Layout.GetLength(1) == Height) return;

            var NewMatrix = new PieceSO[Width, Height];

            int MinX = Mathf.Min(Width, Layout.GetLength(0));
            int MinY = Mathf.Min(Height, Layout.GetLength(1));

            for (int x = 0; x < MinX; x++)
            {
                for (int y = 0; y < MinY; y++)
                {
                    NewMatrix[x, y] = Layout[x, y];
                }
            }
            Layout = NewMatrix;
        }
#endif
    }
}