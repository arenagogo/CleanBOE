using System;
using System.Collections.Concurrent;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();

    public static void RunOnMainThread(Action action)
    {
        _actions.Enqueue(action);
    }


    void Update()
    {
        while (_actions.TryDequeue(out var action))
        {
            action?.Invoke();
        }
    }
}
