using UnityEngine;

class ClientSend
{
    public static void SendTcpData(Packet packet)
    {
        packet.WriteLength();
        Client.instance.SendTcpData(packet);
    }

    public static void SendUdpData(Packet packet)
    {
        packet.WriteLength();
        packet.InsertInt(Client.instance.clientID);
        Client.instance.SendUdpData(packet);
    }

    public static void Handshake(int clientID)
    {
        using (Packet packet = new Packet(PacketType.Handshake))
        {
            packet.Write(clientID);
            SendTcpData(packet);
        }
    }
}