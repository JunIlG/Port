using System;
using System.Collections.Generic;

namespace ChatServer
{
    class ActionThread
    {
        private static readonly List<Action> actions = new List<Action>();
        private static readonly List<Action> copiedActions = new List<Action>();
        private static bool haveToExecute = false;

        public static void RegisterAction(Action action)
        {
            if (action == null)
            {
                Console.WriteLine("Attempt to register null action!");
                return;
            }

            lock (actions)
            {
                actions.Add(action);
                haveToExecute = true;
            }
        }

        public static void Execute()
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
}