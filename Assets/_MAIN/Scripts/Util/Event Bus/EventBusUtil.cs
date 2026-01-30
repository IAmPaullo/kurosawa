using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
/// <summary>
/// A utility class for initializing and managing event buses.
/// </summary>
public static class EventBusUtil
{
    /// <summary>
    /// Gets or sets the list of event types that implement the IEvent interface.
    /// </summary>
    public static IReadOnlyList<Type> EventTypes { get; set; }

    /// <summary>
    /// Gets or sets the list of event bus types.
    /// </summary>
    public static IReadOnlyList<Type> EventBusTypes { get; set; }

#if UNITY_EDITOR
    /// <summary>
    /// Gets or sets the current play mode state in the Unity Editor.
    /// </summary>
    public static PlayModeStateChange PlayModeState { get; set; }

    /// <summary>
    /// Initializes the editor by subscribing to the play mode state change event.
    /// </summary>
    [InitializeOnLoadMethod]
    public static void InitializeEditor()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    /// <summary>
    /// Handles the play mode state change event in the Unity Editor.
    /// </summary>
    /// <param name="state">The new play mode state.</param>
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        PlayModeState = state;
        if (state == PlayModeStateChange.ExitingPlayMode)
            ClearAllBuses();
    }
#endif

    /// <summary>
    /// Initializes the event bus utility before the scene loads.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        EventTypes = PredefinedAssemblyUtil.GetTypes(typeof(IEvent));
        EventBusTypes = InitializeAllBuses();
    }

    /// <summary>
    /// Initializes all event buses for the event types.
    /// </summary>
    /// <returns>A list of event bus types.</returns>
    private static List<Type> InitializeAllBuses()
    {
        List<Type> eventBusTypes = new();
        var typedef = typeof(EventBus<>);

        foreach (var type in EventTypes)
        {
            var busType = typedef.MakeGenericType(type);
            eventBusTypes.Add(busType);
        }
        return eventBusTypes;
    }

    /// <summary>
    /// Clears all event buses by invoking their Clear method.
    /// </summary>
    public static void ClearAllBuses()
    {
        for (int i = 0; i < EventBusTypes.Count; i++)
        {
            var busType = EventBusTypes[i];
            var clearMethod = busType.GetMethod("Clear",
                    BindingFlags.Static | BindingFlags.NonPublic);
            clearMethod.Invoke(null, null);
        }
    }
}