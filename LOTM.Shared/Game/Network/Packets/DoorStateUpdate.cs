using System.IO;
using System.Net;

namespace LOTM.Shared.Game.Network.Packets
{
    public class DoorStateUpdate : ObjectBoundPacket
    {
        public DoorStateUpdate(IPEndPoint sender = default) : base(sender, true)
        {
        }

        public bool Open { get; set; }

        public override void ReadBytes(BinaryReader reader)
        {
            base.ReadBytes(reader);

            Open = reader.ReadBoolean();
        }

        public override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);

            writer.Write(Open);
        }
    }
}
