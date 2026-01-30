using Sirenix.OdinInspector;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "IntVariable", menuName = "Scriptable Variables/Int Variable")]
public class IntVariable : SerializedScriptableObject
{
    public int Value;
    public event Action<int> OnValueChanged;

    public void SetValue(int value)
    {
        Value = value;
        OnValueChanged?.Invoke(Value);
    }
    public void SetValueClamped(int value)
    {
        if(value < 0)
        {
            value = 0;
        }
        Value = value;
        OnValueChanged?.Invoke(Value);
    }
    [Button]
    public void ApplyChange(int changeAmount = 10)
    {
        Value += changeAmount;
        OnValueChanged?.Invoke(Value);
    }
}
