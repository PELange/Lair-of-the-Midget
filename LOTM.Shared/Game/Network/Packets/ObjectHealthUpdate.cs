using System.IO;
using System.Net;

namespace LOTM.Shared.Game.Network.Packets
{
    public class ObjectHealthUpdate : ObjectBoundPacket
    {
        public ObjectHealthUpdate(IPEndPoint sender = default) : base(sender, true)
        {
        }

        public double Health { get; set; }

        public override void ReadBytes(BinaryReader reader)
        {
            base.ReadBytes(reader);

            Health = reader.ReadDouble();
        }

        public override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);

            writer.Write(Health);
        }
    }
}
