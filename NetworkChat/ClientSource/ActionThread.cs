using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionThread : MonoBehaviour
{
    private static readonly List<Action> actions = new List<Action>();
    private static readonly List<Action> copiedActions = new List<Action>();
    private static bool haveToExecute = false;

    private void Update()   
    {
        Execute();
    }

    public static void RegisterAction(Action action)
    {
        if (action == null)
        {
            Debug.LogError("Attempt to register null action!");
            return;
        }

        lock (actions)
        {
            actions.Add(action);
            haveToExecute = true;
        }
    }

    private static void Execute()
    {
        if (haveToExecute)
        {
            copiedActions.Clear();

            lock (actions)
            {
                copiedActions.AddRange(actions);
                actions.Clear();
            }

            foreach (Action action in copiedActions)
            {
                action();
            }

            haveToExecute = false;
        }
    }
}