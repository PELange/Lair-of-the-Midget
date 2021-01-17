using System.IO;
using System.Net;

namespace LOTM.Shared.Game.Network.Packets
{
    public class ObjectPositionUpdate : ObjectBoundPacket
    {
        public ObjectPositionUpdate(IPEndPoint sender = default) : base(sender)
        {
        }

        public float PositionX { get; set; }
        public float PositionY { get; set; }

        public override void ReadBytes(BinaryReader reader)
        {
            base.ReadBytes(reader);

            PositionX = reader.ReadSingle();
            PositionY = reader.ReadSingle();
        }

        public override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);

            writer.Write(PositionX);
            writer.Write(PositionY);
        }
    }
}
