using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UI/Theme/New Theme Library", fileName = "GradientLibrary")]
public class ThemeLibrarySO : ScriptableObject
{

    public List<ThemeSO> Entries = new();
}