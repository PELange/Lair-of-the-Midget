using LOTM.Shared.Engine.Network;
using System.IO;
using System.Net;

namespace LOTM.Shared.Game.Network.Packets
{
    public class PlayerInputAck : NetworkPacket
    {
        public PlayerInputAck(IPEndPoint sender = default) : base(sender)
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
