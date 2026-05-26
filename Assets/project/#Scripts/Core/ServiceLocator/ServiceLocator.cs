// Scripts/Core/ServiceLocator/ServiceLocator.cs
using System;
using System.Collections.Generic;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new();

    public static void Initialise()
    {
        _services.Clear();
    }

    public static void Register<T>(T service)
    {
        _services[typeof(T)] = service;
    }

    public static T Get<T>()
    {
        if (_services.TryGetValue(typeof(T), out var service))
            return (T)service;

        throw new Exception($"Service {typeof(T)} not registered");
    }

    public static bool TryGet<T>(out T service)
    {
        if (_services.TryGetValue(typeof(T), out var s))
        {
            service = (T)s;
            return true;
        }
        service = default;
        return false;
    }
}