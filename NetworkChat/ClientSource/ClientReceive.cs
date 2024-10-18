using System.Net;
using UnityEngine;

public class ClientReceive
{
    public static void HandshakeReceived(Packet packet)
    {
        // Unboxing the data from the packet
        int myID = packet.ReadInt();
        
        Client.instance.clientID = myID;

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);

        ClientSend.Handshake(myID);
    }

    public static void MessageReceived(Packet packet)
    {
        // Unboxing the data from the packet
        string message = packet.ReadString();

        GameManager.instance.AddChatMessage(message);

        Debug.Log(message);
    }
}