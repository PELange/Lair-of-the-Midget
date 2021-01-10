using System.IO;
using System.Net;

namespace LOTM.Shared.Game.Network.Packets
{
    public class PlayerCreation : LivingObjectCreation
    {
        public PlayerCreation(IPEndPoint sender = default) : base(sender)
        {
        }

        public string Name { get; set; }

        public override void ReadBytes(BinaryReader reader)
        {
            base.ReadBytes(reader);

            Name = reader.ReadString();
        }

        public override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);

            writer.Write(Name);
        }
    }
}
