using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BoolVariable", menuName = "Scriptable Variables/Bool Variable")]
public class BoolVariable : ScriptableObject
{
    public bool Value;
    public event Action<bool> OnValueChanged;

    public void SetValue(bool value)
    {
        Value = value;
        OnValueChanged?.Invoke(Value);
    }
}