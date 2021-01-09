using LOTM.Shared.Engine.Network;
using LOTM.Shared.Game.Objects;
using System.IO;
using System.Net;

namespace LOTM.Shared.Game.Network.Packets
{
    public class ObjectCreation : ObjectBoundPacket
    {
        public ObjectCreation(IPEndPoint sender = default) : base(sender)
        {
        }

        public ObjectType Type { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double ScaleX { get; set; }
        public double ScaleY { get; set; }

        public override void ReadBytes(BinaryReader reader)
        {
            base.ReadBytes(reader);

            Type = (ObjectType)reader.ReadByte();
            PositionX = reader.ReadDouble();
            PositionY = reader.ReadDouble();
            ScaleX = reader.ReadDouble();
            ScaleY = reader.ReadDouble();
        }

        public override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);

            writer.Write((byte)Type);
            writer.Write(PositionX);
            writer.Write(PositionY);
            writer.Write(ScaleX);
            writer.Write(ScaleY);
        }
    }
}
