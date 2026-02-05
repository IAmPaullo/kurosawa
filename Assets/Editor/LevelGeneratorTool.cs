using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Gameplay.Core.Data;

namespace Gameplay.Core.Editor
{
    public class LevelGeneratorTool : OdinEditorWindow
    {
        [MenuItem("Tools/Level Generator")]
        private static void OpenWindow()
        {
            GetWindow<LevelGeneratorTool>().Show();
        }

        [Title("Piece References")]
        [Required] public PieceSO Source;
        [Required] public PieceSO Lamp;
        [Required] public PieceSO Straight;
        [Required] public PieceSO Curve;
        [Required] public PieceSO FourConnect;
        [Required] public PieceSO Scenery;

        [Title("Generation Settings")]
        [FolderPath]
        public string SavePath = "Assets/Data/Levels";

        [Button(ButtonSizes.Large), GUIColor(0, 1, 0)]
        public void GenerateAllLevels()
        {
            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
                AssetDatabase.Refresh();
            }

            // Tutorial style levels (1-10)

            CreateLevel("Level_01", 4, 1, new string[] {
                "S-L"
            });

            CreateLevel("Level_02", 3, 3, new string[] {
                "S-C",
                "__-",
                "__L"
            });

            CreateLevel("Level_03", 3, 3, new string[] {
                "_L_",
                "S+L",
                "_L_"
            });

            CreateLevel("Level_04", 4, 4, new string[] {
                "SC_L",
                "_C-C",
                "____",
                "____"
            });

            CreateLevel("Level_05", 4, 4, new string[] {
                "S-CL",
                "__-_",
                "LC+S",
                "____"
            });

            CreateLevel("Level_06", 4, 4, new string[] {
                "S-C_",
                "-.-_",
                "C-+L",
                "__L_"
            });

            CreateLevel("Level_07", 5, 5, new string[] {
                "SC_CL",
                "_-.-_",
                "_C-C_",
                "_____",
                "_____"
            });

            CreateLevel("Level_08", 5, 5, new string[] {
                "S-+L",
                "__-__",
                "L-+S",
                "__-__",
                "__L__"
            });

            CreateLevel("Level_09", 6, 6, new string[] {
                "S-C___",
                "__-_CL",
                "LC+SC_",
                "_-____",
                "SC____",
                "______"
            });

            CreateLevel("Level_10", 6, 6, new string[] {
                "S-CLCS",
                "__-_-_",
                "LC+S+L",
                "_-__-_",
                "SCLCS",
                "_____L"
            });

            // Intermediate and advanced levels (11-20)

            // Level 11: Vertical zigzag
            // Requires rotating straight pieces to form the staircase
            CreateLevel("Level_11", 5, 5, new string[] {
                "S-C__",
                "__C-C",
                "__L_-",
                "S-C_L",
                "__C-L"
            });

            // Level 12: The fork (splitter)
            // One source feeding multiple parallel paths
            CreateLevel("Level_12", 5, 5, new string[] {
                "__L__",
                "__-__",
                "L-+-L",
                "__-__",
                "__S__"
            });

            // Level 13: High density
            // Small grid packed with connections
            CreateLevel("Level_13", 4, 4, new string[] {
                "S+L_",
                "-+-C",
                "+-+S",
                "L-C_"
            });

            // Level 14: Perimeter
            // The path runs along the outer edges
            CreateLevel("Level_14", 6, 6, new string[] {
                "S-C___",
                "L_C-C_",
                "C___C_",
                "C___L_",
                "C-C___",
                "__L___"
            });

            // Level 15: Twin systems
            // Two independent circuits on the same grid
            CreateLevel("Level_15", 6, 6, new string[] {
                "S-C_S-",
                "__C-C_",
                "L_____",
                "____L_",
                "_C-C__",
                "_L_C-L"
            });

            // Level 16: Crossfire
            // Heavy use of 4-connect pieces
            CreateLevel("Level_16", 6, 6, new string[] {
                "S-+-C_",
                "__L_C-",
                "_-+-_L",
                "L-+-__",
                "__S___",
                "______"
            });

            // Level 17: Spiral
            // Path that curls toward the center
            CreateLevel("Level_17", 7, 7, new string[] {
                "S-C____",
                "__C-C__",
                "__L_C__",
                "__C-C__",
                "__C-C__",
                "__C-C__",
                "__L____"
            });

            // Level 18: Islands
            // Groups separated by scenery (empty space)
            CreateLevel("Level_18", 7, 7, new string[] {
                "S-L_S-L",
                "_______",
                "S-C_C-S",
                "__C-C__",
                "__L_L__",
                "_______",
                "L-C_C-L"
            });

            // Level 19: Gridlock
            // Very dense with little room for error
            CreateLevel("Level_19", 6, 6, new string[] {
                "S-C-C-",
                "L_+_C_",
                "C-S-C_",
                "C_+_L_",
                "C-L___",
                "______"
            });

            // Level 20: Grandmaster
            // Wide and complex
            CreateLevel("Level_20", 8, 8, new string[] {
                "S-C_____",
                "__C-C___",
                "L-+-C_S-",
                "__C-+-C_",
                "__L_L_C_",
                "______L_",
                "S-C_____",
                "__L_____"
            });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"<color=green>Success:</color> 20 levels generated or updated at {SavePath}");
        }

        private void CreateLevel(string levelName, int width, int height, string[] rows)
        {
            LevelDataSO level = ScriptableObject.CreateInstance<LevelDataSO>();

            level.Width = width;
            level.Height = height;
            level.Layout = new PieceSO[width, height];

            // Time formula tuned for larger levels
            level.TargetTimeS = 10f + (width * height * 0.75f);
            level.TargetTimeA = level.TargetTimeS * 1.4f;
            level.TargetTimeB = level.TargetTimeS * 2.0f;

            for (int y = 0; y < height; y++)
            {
                if (y >= rows.Length) continue;

                // Flip Y so the parser draws top to bottom visually
                string row = rows[rows.Length - 1 - y];

                char[] chars = row.ToCharArray();

                for (int x = 0; x < width; x++)
                {
                    if (x >= chars.Length) continue;
                    level.Layout[x, y] = GetPieceFromChar(chars[x]);
                }
            }

            string path = $"{SavePath}/{levelName}.asset";
            AssetDatabase.CreateAsset(level, path);
        }

        private PieceSO GetPieceFromChar(char c)
        {
            switch (char.ToUpper(c))
            {
                case 'S': return Source;
                case 'L': return Lamp;
                case '-': return Straight; // Remember the game must rotate this if it needs to be vertical
                case '.': return Straight; // Visual alias for vertical in the layout strings
                case 'C': return Curve;
                case '+': return FourConnect;
                case '_': return Scenery;
                default: return null;
            }
        }
    }
}
