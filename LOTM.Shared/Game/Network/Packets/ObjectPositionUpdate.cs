using System.IO;
using System.Net;

namespace LOTM.Shared.Game.Network.Packets
{
    public class ObjectPositionUpdate : ObjectBoundPacket
    {
        public ObjectPositionUpdate(IPEndPoint sender = default) : base(sender)
        {
        }

        public double PositionX { get; set; }
        public double PositionY { get; set; }

        public override void ReadBytes(BinaryReader reader)
        {
            base.ReadBytes(reader);

            PositionX = reader.ReadDouble();
            PositionY = reader.ReadDouble();
        }

        public override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);

            writer.Write(PositionX);
            writer.Write(PositionY);
        }
    }
}
