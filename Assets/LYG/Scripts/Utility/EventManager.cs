using System;
using System.Collections.Generic;
using UnityEngine;

public enum Event
{
    Start, Pause, Stop, Play
}


public static class EventManager
{
    static Dictionary<Event, Action> bus = new();

    public static void Subscribe(Event name, Action action)
    {
        if (!bus.ContainsKey(name))
        {
            bus[name] = action;
            return;
        }

        bus[name] += action;
    }

    public static void Unsubscribe(Event name, Action action)
    {
        if (!bus.ContainsKey(name))
            return;

        bus[name] -= action;
    }

    public static void Publish(Event name)
    {
        if (!bus.ContainsKey(name))
            return;

        bus[name]?.Invoke();
    }
}
