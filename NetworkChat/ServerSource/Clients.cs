using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    class Client
    {
        public int ID;
        public bool isTcpConnected;
        public bool isUdpConnected;

        // Tcp socket
        public Tcp tcp;

        // Udp socket
        public Udp udp;

        public Client(int clientID)
        {
            ID = clientID;
            isTcpConnected = false;
            isUdpConnected = false;
        }

        public void TcpConnect(TcpClient socket)
        {
            tcp = new Tcp(ID);
            tcp.Connect(socket);

            isTcpConnected = true;

            ServerSend.Handshake(ID);
        }

        public void UdpConnect(IPEndPoint endPoint)
        {
            udp = new Udp(ID);
            udp.Connect(endPoint);

            isUdpConnected = true;
        }

        public void Disconnect()
        {
            if (isTcpConnected)
            {
                tcp.Disconnect();
                isTcpConnected = false;
            }
            if (isUdpConnected)
            {
                udp.Disconnect();
                isUdpConnected = false;
            }

            Console.WriteLine($"{ID} has disconnected");
        }

        public void SendTcpData(Packet packet)
        {
            tcp.Send(packet);
        }

        public void SendUdpData(Packet packet)
        {
            udp.Send(packet);
        }
    }

    public class Tcp
    {
        public int clientID;
        public TcpClient socket;
        private NetworkStream stream;
        private byte[] buffer;
        private Packet packet;

        public Tcp(int inID)
        {
            clientID = inID;
        }

        public void Connect(TcpClient connectedSocket)
        {
            socket = connectedSocket;
            socket.ReceiveBufferSize = Constants.MAX_BUFFER_SIZE;
            socket.SendBufferSize = Constants.MAX_BUFFER_SIZE;

            buffer = new byte[Constants.MAX_BUFFER_SIZE];

            packet = new Packet();

            stream = socket.GetStream();
            stream.BeginRead(buffer, 0, Constants.MAX_BUFFER_SIZE, OnReceive, null);
        }

        public void Disconnect()
        {
            socket.Close();
            socket = null;
            stream = null;
            buffer = null;
            packet = null;
        }

        #region Send Data
        public void Send(Packet sendPacket)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(sendPacket.ToArray(), 0, sendPacket.Length(), null, null);
                }
            }
            catch (Exception e) 
            { 
                Console.WriteLine(e.Message);
            }
        }
        #endregion

        #region Receive Data
        private void OnReceive(IAsyncResult ar)
        {
            try 
            {
                int dataBytes = stream.EndRead(ar);
                if (dataBytes == 0)
                {
                    Server.clients[clientID].Disconnect();
                    return;
                }

                byte[] data = new byte[dataBytes];
                Array.Copy(buffer, data, dataBytes);

                packet.Reset(ProcessData(data));

                stream.BeginRead(buffer, 0, Constants.MAX_BUFFER_SIZE, OnReceive, null);
            }
            catch (Exception e) 
            {
                Console.WriteLine(e.Message);
                Server.clients[clientID].Disconnect();
            }
        }

        private bool ProcessData(byte[] data)
        {
            int packetLength = 0;

            packet.SetBytes(data);

            if (packet.UnreadLength() >= 4)
            {
                packetLength = packet.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }

            while (packetLength > 0 && packetLength <= packet.UnreadLength())
            {
                byte[] packetBytes = packet.ReadBytes(packetLength);
                
                ActionThread.RegisterAction(() => 
                {
                    using (Packet newPacket = new Packet(packetBytes))
                    {
                        int evtID = newPacket.ReadInt();
                        Server.packetReceivers[evtID](clientID, newPacket);
                    }
                });

                packetLength = 0;

                if (packet.UnreadLength() >= 4)
                {
                    packetLength = packet.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (packetLength <= 1)
            {
                return true;
            }

            return false;
        }
        #endregion
    }
    
    public class Udp
    {
        private int clientID;
        public IPEndPoint endPoint;

        public Udp(int inID)
        {
            clientID = inID;
        }

        public void Connect(IPEndPoint inEndPoint)
        {
            endPoint = inEndPoint;
        }

        public void Disconnect()
        {
            clientID = 0;
            endPoint = null;
        }

        #region Send Data
        public void Send(Packet packet)
        {
            Server.udpListener.BeginSend(packet.ToArray(), packet.Length(), endPoint, null, null);
        }
        #endregion

        #region Receive Data
        public void ProcessData(Packet packet)
        {
            int packetLength = packet.ReadInt();
            byte[] data = packet.ReadBytes(packetLength);

            ActionThread.RegisterAction(() =>
            {
                using (Packet newPacket = new Packet(data))
                {
                    int evtID = packet.ReadInt();
                    Server.packetReceivers[evtID](clientID, newPacket);
                }
            });
        }
        #endregion

    }
}