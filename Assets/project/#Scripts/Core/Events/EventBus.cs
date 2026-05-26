// Scripts/Core/Events/EventBus.cs
using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<Type, List<Delegate>> _listeners = new();

    public static void Initialise()
    {
        _listeners.Clear();
    }

    public static void Subscribe<T>(Action<T> listener) where T : struct
    {
        var type = typeof(T);
        if (!_listeners.ContainsKey(type))
            _listeners[type] = new List<Delegate>();

        _listeners[type].Add(listener);
    }

    public static void Unsubscribe<T>(Action<T> listener) where T : struct
    {
        var type = typeof(T);
        if (_listeners.ContainsKey(type))
            _listeners[type].Remove(listener);
    }

    public static void Publish<T>(T eventData) where T : struct
    {
        var type = typeof(T);
        if (!_listeners.ContainsKey(type)) return;

        foreach (var listener in _listeners[type])
            ((Action<T>)listener)?.Invoke(eventData);
    }
}