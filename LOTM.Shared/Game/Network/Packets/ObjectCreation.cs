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
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float ScaleX { get; set; }
        public float ScaleY { get; set; }

        public override void ReadBytes(BinaryReader reader)
        {
            base.ReadBytes(reader);

            Type = (ObjectType)reader.ReadInt16();
            PositionX = reader.ReadSingle();
            PositionY = reader.ReadSingle();
            ScaleX = reader.ReadSingle();
            ScaleY = reader.ReadSingle();
        }

        public override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);

            writer.Write((short)Type);
            writer.Write(PositionX);
            writer.Write(PositionY);
            writer.Write(ScaleX);
            writer.Write(ScaleY);
        }
    }
}
