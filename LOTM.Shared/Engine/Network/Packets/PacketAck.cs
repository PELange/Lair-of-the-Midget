using System.IO;
using System.Net;

namespace LOTM.Shared.Engine.Network.Packets
{
    public class PacketAck : NetworkPacket
    {
        public PacketAck(IPEndPoint sender = default) : base(sender)
        {
        }

        public int AckPacketId { get; set; }

        public override void ReadBytes(BinaryReader reader)
        {
            base.ReadBytes(reader);

            AckPacketId = reader.ReadInt32();
        }

        public override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);

            writer.Write(AckPacketId);
        }
    }
}
