using LOTM.Shared.Engine.Controls;
using LOTM.Shared.Engine.Network;
using System.IO;
using System.Net;

namespace LOTM.Shared.Game.Network.Packets
{
    public class PlayerInput : NetworkPacket
    {
        public PlayerInput(IPEndPoint sender = default) : base(sender)
        {
        }

        public InputType Inputs { get; set; }

        public override void ReadBytes(BinaryReader reader)
        {
            base.ReadBytes(reader);

            Inputs = (InputType)reader.ReadByte();
        }

        public override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);

            writer.Write((byte)Inputs); //Only one byte for now. If we have more controls change it to short
        }
    }
}
