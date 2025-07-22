using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NSW_Utilities
{
    public class NSW_CommandDispatcher : ICommandDispatcher
    {
        private readonly Queue<Action> deferredActions = new();
        private readonly Dictionary<string, Action> commandMap = new();

        public void QueueAction(Action action)
        {
            if (action != null)
                deferredActions.Enqueue(action);
        }

        public void RegisterCommand(string name, Action action)
        {
            if (!commandMap.ContainsKey(name))
                commandMap[name] = action;
        }

        public void Call(string name)
        {
            if (commandMap.TryGetValue(name, out var action))
                QueueAction(action);
            else
                Debug.LogWarning($"[CommandDispatcher] Command '{name}' not found.");
        }

        public void ExecutePending()
        {
            while (deferredActions.Count > 0)
            {
                var action = deferredActions.Dequeue();
                action?.Invoke();
            }
        }
    }

    public interface ICommandDispatcher
    {
        void QueueAction(Action action);
        void RegisterCommand(string name, Action action);
        void Call(string name);
        void ExecutePending();
    }
}

