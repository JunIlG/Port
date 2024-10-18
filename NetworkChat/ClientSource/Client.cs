using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.Burst.CompilerServices;

public class Client : MonoBehaviour
{
    public static Client instance;

    public int clientID = 0;
    public Tcp tcp;
    public Udp udp;
    private bool isConnected = false;

    public delegate void PacketReceiver(Packet packet);
    public static Dictionary<int, PacketReceiver> packetReceivers;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        Initialize();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    private void Initialize()
    {
        tcp = new Tcp();
        udp = new Udp();

        packetReceivers = new Dictionary<int, PacketReceiver>()
        {
            { (int)PacketType.Handshake, ClientReceive.HandshakeReceived },
            { (int)PacketType.Message,  ClientReceive.MessageReceived }
        };

        tcp.Connect();
        isConnected = true;
    }

    public void SendTcpData(Packet packet)
    {
        tcp.Send(packet);
    }

    public void SendUdpData(Packet packet)
    {
        udp.Send(packet);
    }

    public void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;
            tcp.socket?.Close();
            udp.socket?.Close();

            Debug.Log("Disconnect from server");
        }
    }
}

public class Tcp
{
    public TcpClient socket;
    private NetworkStream stream;
    private byte[] buffer;
    private Packet packet;

    #region Connect
    public void Connect()
    {
        socket = new TcpClient();
        socket.ReceiveBufferSize = Constants.MAX_BUFFER_SIZE;
        socket.SendBufferSize = Constants.MAX_BUFFER_SIZE;

        buffer = new byte[Constants.MAX_BUFFER_SIZE];

        socket.BeginConnect(Constants.SERVER_IP, Constants.SERVER_PORT, OnTcpConnect, socket);
    }

    private void OnTcpConnect(IAsyncResult ar)
    {
        socket.EndConnect(ar);

        if(!socket.Connected)
        {
            return;
        }

        stream = socket.GetStream();
        packet = new Packet();

        stream.BeginRead(buffer, 0, Constants.MAX_BUFFER_SIZE, OnReceive, null);
    }

    private void Disconnect()
    {
        Client.instance.Disconnect();

        socket = null;
        stream = null;
        buffer = null;
        packet = null;
    }
    #endregion

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
            Debug.Log($"Data send error: {e}");
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
                Disconnect();
                return;
            }

            byte[] data = new byte[dataBytes];
            Array.Copy(buffer, data, dataBytes);

            packet.Reset(ProcessData(data));

            stream.BeginRead(buffer, 0, Constants.MAX_BUFFER_SIZE, OnReceive, null);
        }
        catch
        {
            Disconnect();
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
                    int packetType = newPacket.ReadInt();
                    Client.packetReceivers[packetType](newPacket);
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
    public UdpClient socket;
    public IPEndPoint endPoint;

    public Udp()
    {
        endPoint = new IPEndPoint(IPAddress.Parse(Constants.SERVER_IP), Constants.SERVER_PORT);
    }

    public void Connect(int localPort)
    {
        Debug.Log(localPort);
        socket = new UdpClient(localPort);

        socket.Connect(endPoint);

        socket.BeginReceive(OnReceive, null);

        // Open a Udp port to listen for incoming messages
        using (Packet packet = new Packet())
        {
            Send(packet);
        }
    }

    public void Disconnect()
    {
        Client.instance.Disconnect();

        socket = null;
        endPoint = null;
    }

    #region Send Data
    public void Send(Packet packet)
    {
        try
        {
            socket?.BeginSend(packet.ToArray(), packet.Length(), null, null);
        }
        catch (Exception e)
        {
            Debug.Log($"Data send error: {e}");
        }
    }
    #endregion

    #region Receive Data
    private void OnReceive(IAsyncResult ar)
    {
        try
        {
            byte[] data = socket.EndReceive(ar, ref endPoint);
            socket.BeginReceive(OnReceive, null);

            if (data.Length < 4)
            {
                Disconnect();
                return;
            }

            ProcessData(data);
        }
        catch
        {
            Disconnect();
        }
    }

    private void ProcessData(byte[] data)
    {
        using (Packet packet = new Packet(data))
        {
            int packetLength = packet.ReadInt();
            data = packet.ReadBytes(packetLength);
        }

        ActionThread.RegisterAction(() => 
        {
            using (Packet newPacket = new Packet(data))
            {
                int packetType = newPacket.ReadInt();
                Client.packetReceivers[packetType](newPacket);
            }
        });
    }
    #endregion
}

