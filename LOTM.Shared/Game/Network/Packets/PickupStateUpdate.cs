using System.IO;
using System.Net;

namespace LOTM.Shared.Game.Network.Packets
{
    public class PickupStateUpdate : ObjectBoundPacket
    {
        public PickupStateUpdate(IPEndPoint sender = default) : base(sender, true)
        {
        }

        public bool Active { get; set; }

        public override void ReadBytes(BinaryReader reader)
        {
            base.ReadBytes(reader);

            Active = reader.ReadBoolean();
        }

        public override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);

            writer.Write(Active);
        }
    }
}
