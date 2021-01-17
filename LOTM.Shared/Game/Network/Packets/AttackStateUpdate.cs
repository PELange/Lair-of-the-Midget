using System.IO;
using System.Net;

namespace LOTM.Shared.Game.Network.Packets
{
    public class AttackStateUpdate : ObjectBoundPacket
    {
        public AttackStateUpdate(IPEndPoint sender = default) : base(sender, true)
        {
        }

        public bool Attacking { get; set; }

        public override void ReadBytes(BinaryReader reader)
        {
            base.ReadBytes(reader);

            Attacking = reader.ReadBoolean();
        }

        public override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);

            writer.Write(Attacking);
        }
    }
}
