using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A static class that serves as an event bus for events of type T.
/// </summary>
/// <typeparam name="T">The type of event that implements the IEvent interf
public static class EventBus<T> where T : IEvent
{
    static readonly HashSet<IEventBinding<T>> bindings = new();
    /// <summary>
    /// Registers an event binding to the event bus.
    /// </summary>
    /// <param name="binding">The event binding to register.</param>
    public static void Register(EventBinding<T> binding) => bindings.Add(binding);

    /// <summary>
    /// Deregisters an event binding from the event bus.
    /// </summary>
    /// <param name="binding">The event binding to deregister.</param>
    public static void Deregister(EventBinding<T> binding) => bindings.Remove(binding);

    /// <summary>
    /// Raises an event, invoking all registered event bindings.
    /// </summary>
    /// <param name="event">The event to raise.</param>
    public static void Raise(T @event)
    {
        foreach (var binding in bindings)
        {
            binding.OnEvent.Invoke(@event);
            binding.OnEventNoArgs.Invoke();
        }
    }

    static void Clear()
    {
        Debug.Log($"Clearing {typeof(T).Name} bindings");
        bindings.Clear();
    }
}
