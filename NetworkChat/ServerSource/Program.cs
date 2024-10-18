using System;

namespace ChatServer
{
    class Program
    {
        private static bool isRunning;

        static void Main(string[] args)
        {
            isRunning = true;

            Console.Title = "Chat Server";

            Thread thread = new Thread(Update);
            thread.Start();
            
            Server.Start();
        }

        private static void Update()
        {
            while (isRunning)
            {
                ActionThread.Execute();

                Thread.Sleep(Constants.MS_PER_TICK);
            }
        }
    }
}