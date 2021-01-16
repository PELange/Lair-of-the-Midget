using LOTM.Shared.Engine.Network.Packets;
using System.IO;
using System.Net;

namespace LOTM.Shared.Engine.Network
{
    public class NetworkPacketSerializationProvider
    {
        public virtual bool SerializePacket(NetworkPacket packet, out byte[] data)
        {
            data = default;

            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);

            switch (packet)
            {
                case PacketAck _:
                    writer.Write(0);
                    break;

                default:
                    return false; //Unknown packet type
            }

            packet.WriteBytes(writer);

            data = memoryStream.ToArray();

            return true;
        }

        public virtual NetworkPacket DeserializePacket(byte[] data, IPEndPoint sender)
        {
            if (data == null || sender == null || data.Length < 1) return null;

            using MemoryStream memoryStream = new MemoryStream(data);
            using BinaryReader reader = new BinaryReader(memoryStream);

            var type = reader.ReadInt32();
            NetworkPacket networkPacket = null;

            switch (type)
            {
                case 0:
                    networkPacket = new PacketAck(sender);
                    break;
            }

            if (networkPacket != null)
            {
                networkPacket.ReadBytes(reader);
            }

            return networkPacket;
        }
    }
}
