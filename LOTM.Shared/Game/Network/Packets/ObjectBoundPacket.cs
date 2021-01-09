using LOTM.Shared.Engine.Network;
using System.IO;
using System.Net;

namespace LOTM.Shared.Game.Network.Packets
{
    public class ObjectBoundPacket : NetworkPacket
    {
        public ObjectBoundPacket(IPEndPoint sender = default) : base(sender)
        {
        }

        public int ObjectId { get; set; }

        public override void ReadBytes(BinaryReader reader)
        {
            base.ReadBytes(reader);

            ObjectId = reader.ReadInt32();
        }

        public override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);

            writer.Write(ObjectId);
        }
    }
}
