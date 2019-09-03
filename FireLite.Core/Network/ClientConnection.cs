using System;
using System.Net.Sockets;
using System.Threading;
using FireLite.Core.Exceptions;
using FireLite.Core.Extensions;
using FireLite.Core.Interfaces;

namespace FireLite.Core.Network
{
    public delegate void ClientConnectionEventHandler(ClientConnection sender);

    public delegate void ClientPacketReceivedEventHandler(ClientConnection sender, byte[] bytes);

    public class ClientConnection : IClientConnection
    {
        public event ClientConnectionEventHandler Disconnected;

        public event ClientPacketReceivedEventHandler PacketReceived;

        public Guid Id { get; private set; }

        private readonly TcpClient tcpClient;
        private readonly NetworkStream networkStream;
        private readonly Thread listenThread;

        public ClientConnection(TcpClient tcpClient)
        {
            Id = Guid.NewGuid();

            this.tcpClient = tcpClient;

            networkStream = tcpClient.GetStream();

            listenThread = new Thread(ListenToClient);
            listenThread.Start();
        }

        public void Disconnect()
        {
            Disconnected?.Invoke(this);

            tcpClient.Close();
            listenThread.Abort();
        }

        public void SendPacket(byte[] packetBytes)
        {
            networkStream.SendPacket(packetBytes);
        }

        private void ListenToClient()
        {
            while (true)
            {
                try
                {
                    var packetBytes = networkStream.ReadPacket();

                    PacketReceived?.Invoke(this, packetBytes);
                }
                catch (ConnectionException)
                {
                    Disconnect();
                    break;
                }
            }
        }
    }
}