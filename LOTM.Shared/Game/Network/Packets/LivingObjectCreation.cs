using System.IO;
using System.Net;

namespace LOTM.Shared.Game.Network.Packets
{
    public class LivingObjectCreation : ObjectCreation
    {
        public LivingObjectCreation(IPEndPoint sender = default) : base(sender)
        {
        }

        public float Health { get; set; }

        public override void ReadBytes(BinaryReader reader)
        {
            base.ReadBytes(reader);

            Health = reader.ReadSingle();
        }

        public override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);

            writer.Write(Health);
        }
    }
}
