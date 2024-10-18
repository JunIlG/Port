using System;

namespace ChatServer
{
    class ServerReceive
    {
        public static void HandshakeReceived(int fromClientID, Packet packet)
        {
            string message = $"{fromClientID} has connected to the server";
            ServerSend.BroadcastMessage(message);
        }

        public static void MessageReceived(int fromClientID, Packet packet)
        {
            // Unboxing the data from the packet
            string message = packet.ReadString();

            // Broadcast the message to all clients except the sender
            message = $"{fromClientID}: {message}";
            ServerSend.BroadcastMessage(message);

            Console.WriteLine("Message received from client " + fromClientID + ": " + message);
        }
    }
}