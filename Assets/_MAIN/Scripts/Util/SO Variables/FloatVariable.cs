using Sirenix.OdinInspector;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "FloatVariable", menuName = "Scriptable Variables/Float Variable")]
public class FloatVariable : SerializedScriptableObject
{
    public float Value;
    public event Action<float> OnValueChanged;

    public void SetValue(float value)
    {
        Value = value;
        OnValueChanged?.Invoke(Value);
    }
    [Button]
    public void ApplyChange(float changeAmount = 10)
    {
        Value += changeAmount;
        OnValueChanged?.Invoke(Value);
    }

    public int ToInt() => (int)Value;

    public void ResetFloat()
    {
        //OnValueChanged = null;
        Value = 0;
    }

}
