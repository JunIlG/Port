using System;
using System.Collections.Generic;

namespace ChatServer
{
    class ServerSend
    {
        private static void SendTcpData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server.clients[toClient].SendTcpData(packet);
        }

        private static void SendUdpData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server.clients[toClient].SendUdpData(packet);
        }

        private static void SendTcpDataToAll(Packet packet, int except = -1)
        {
            packet.WriteLength();

            for (int i = 1; i <= Constants.MAX_CLIENTS; i++)
            {
                if (i == except || Server.clients[i].isTcpConnected == false) 
                {
                    continue;
                }

                Server.clients[i].SendTcpData(packet);
            }
        }

        private static void SendUdpDataToAll(Packet packet, int except = -1)
        {
            packet.WriteLength();

            for (int i = 1; i <= Constants.MAX_CLIENTS; i++)
            {
                if (i == except || Server.clients[i].isUdpConnected == false) 
                {
                    continue;
                }

                Server.clients[i].SendUdpData(packet);
            }
        }

        public static void Handshake(int toClient)
        {
            using (Packet packet = new Packet(PacketType.Handshake))
            {
                // Boxing the data to the packet
                packet.Write(toClient);

                SendTcpData(toClient, packet);
            }
        }

        public static void BroadcastMessage(string msg)
        {
            using (Packet packet = new Packet(PacketType.Message))
            {
                // Boxing the data to the packet
                packet.Write(msg);

                SendTcpDataToAll(packet);
            }
        }
    }
}