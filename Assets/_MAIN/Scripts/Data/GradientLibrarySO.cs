using System;
using System.Collections.Generic;
using UnityEngine;

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