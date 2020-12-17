using System.Net;

namespace LOTM.Shared.Engine.Network
{
    public interface INetworkPacketSerializationProvider
    {
        public byte[] SerializePacket(NetworkPacket packet);

        public NetworkPacket DeserializePacket(byte[] data, IPEndPoint sender);
    }
}
