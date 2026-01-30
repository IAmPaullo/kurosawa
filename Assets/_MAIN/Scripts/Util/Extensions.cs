using System;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    #region Collections
    public static T GetRandom<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            Debug.LogWarning("A lista está vazia ou não foi inicializada.");
            return default;
        }
        int randomIndex = UnityEngine.Random.Range(0, list.Count);
        return list[randomIndex];
    }
    public static T GetRandom<T>(this T[] array)
    {
        if (array == null || array.Length == 0)
        {
            Debug.LogWarning("O array está vazio ou não foi inicializado.");
            return default;
        }
        int randomIndex = UnityEngine.Random.Range(0, array.Length);
        return array[randomIndex];
    }
    public static void Shuffle<T>(this IList<T> list)
    {
        if (list == null || list.Count <= 1)
            return;

        var rng = new System.Random(); // já usa seed baseada no tempo
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
    #endregion
    public static T GetRandomEnumValue<T>() where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        int randomIndex = UnityEngine.Random.Range(0, values.Length);
        return values[randomIndex];
    }
}