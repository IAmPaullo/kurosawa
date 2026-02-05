using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StringVariable", menuName = "Scriptable Variables/String Variable")]
public class StringVariable : ScriptableObject
{
    public string Value;
    public event Action<string> OnValueChanged;

    public void SetValue(string value)
    {
        Value = value;
        OnValueChanged?.Invoke(Value);
    }
}

