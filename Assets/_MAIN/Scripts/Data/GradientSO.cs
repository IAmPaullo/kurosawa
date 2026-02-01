using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New UI Theme", menuName = "UI/Theme Data")]
public class GradientSO : ScriptableObject
{
    [Title("Colors")]
    public Gradient MainGradient;
    public Color TextColor = Color.white;
    public Color ButtonColor = Color.white;

    [Title("Settings")]
    public bool UseDarkText = false;
}


[CreateAssetMenu(menuName = "UI/Theme/New Theme Library", fileName = "GradientLibrary")]
public class GradientLibrarySO : ScriptableObject
{
    [Serializable]
    public class GradientEntry
    {
        public string Name;
        public Gradient Gradient;
        public Texture2D Preview;
    }

    public List<GradientEntry> Entries = new();
}