using LOTM.Shared.Engine.Network;
using System.IO;
using System.Net;

namespace LOTM.Shared.Game.Network.Packets
{
    public class ObjectPositionUpdate : NetworkPacket
    {
        public ObjectPositionUpdate(IPEndPoint sender = default) : base(sender)
        {
        }

        public int ObjectId { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }

        public override void ReadBytes(BinaryReader reader)
        {
            base.ReadBytes(reader);

            ObjectId = reader.ReadInt32();
            PositionX = reader.ReadDouble();
            PositionY = reader.ReadDouble();
        }

        public override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);

            writer.Write(ObjectId);
            writer.Write(PositionX);
            writer.Write(PositionY);
        }
    }
}
