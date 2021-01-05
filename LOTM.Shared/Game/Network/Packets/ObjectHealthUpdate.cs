using LOTM.Shared.Engine.Network;
using System.IO;
using System.Net;

namespace LOTM.Shared.Game.Network.Packets
{
    public class ObjectHealthUpdate : NetworkPacket
    {
        public ObjectHealthUpdate(IPEndPoint sender = default) : base(sender)
        {
        }

        public int ObjectId { get; set; }
        public double Health { get; set; }

        public override void ReadBytes(BinaryReader reader)
        {
            base.ReadBytes(reader);

            ObjectId = reader.ReadInt32();
            Health = reader.ReadDouble();
        }

        public override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);

            writer.Write(ObjectId);
            writer.Write(Health);
        }
    }
}
