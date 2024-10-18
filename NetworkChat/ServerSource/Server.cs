using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    class Server
    {
        private static TcpListener tcpConnectListener;
        public static UdpClient udpListener;
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public delegate void PacketReceiver(int clientID, Packet packet);
        public static Dictionary<int, PacketReceiver> packetReceivers;

        public static void Start()
        {
            Console.WriteLine("Server is starting...");

            Initialize();
        }

        private static void Initialize()
        {
            Console.WriteLine("Initializing server...");

            for (int i = 1; i <= Constants.MAX_CLIENTS; i++)
            {
                clients.Add(i, new Client(i));
            }

            // Tcp Connect Listener
            tcpConnectListener = new TcpListener(IPAddress.Any, Constants.SERVER_PORT);
            tcpConnectListener.Start();
            tcpConnectListener.BeginAcceptTcpClient(OnTcpConnect, null);

            // Udp Listener
            udpListener = new UdpClient(Constants.SERVER_PORT);
            udpListener.BeginReceive(OnUdpReceive, null);

            // Packet Receivers
            packetReceivers = new Dictionary<int, PacketReceiver>()
            {
                { (int)PacketType.Handshake, ServerReceive.HandshakeReceived },
                { (int)PacketType.Message, ServerReceive.MessageReceived }
            };

            Console.WriteLine("Server initialized.");
        }

        private static void OnTcpConnect(IAsyncResult ar)
        {
            TcpClient client = tcpConnectListener.EndAcceptTcpClient(ar);
            tcpConnectListener.BeginAcceptTcpClient(OnTcpConnect, null);

            Console.WriteLine("Attempt to connect...");

            for (int i = 1; i <= Constants.MAX_CLIENTS; i++)
            {
                if(clients[i].isTcpConnected == false)
                {
                    clients[i].TcpConnect(client);
                    Console.WriteLine($"Client {i} connected.");
                    return;
                }
            }

            Console.WriteLine("Connect failed: Server is full.");
        }

        private static void OnUdpReceive(IAsyncResult ar)
        {
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpListener.EndReceive(ar, ref clientEndPoint);
                udpListener.BeginReceive(OnUdpReceive, null);

                if (data.Length < 4)
                {
                    return;
                }

                using (Packet packet = new Packet(data))
                {
                    int clientID = packet.ReadInt();

                    if (clients[clientID].isUdpConnected == false)
                    {
                        clients[clientID].UdpConnect(clientEndPoint);
                    }

                    clients[clientID].udp.ProcessData(packet);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}