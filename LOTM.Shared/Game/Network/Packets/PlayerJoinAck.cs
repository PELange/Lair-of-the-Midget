using LOTM.Shared.Engine.Network;
using System.IO;
using System.Net;

namespace LOTM.Shared.Game.Network.Packets
{
    public class PlayerJoinAck : NetworkPacket
    {
        public PlayerJoinAck(IPEndPoint sender = default) : base(sender)
        {
        }

        public int WorldSeed { get; set; }
        public int PlayerObjectNetworkId { get; set; }

        public override void ReadBytes(BinaryReader reader)
        {
            base.ReadBytes(reader);

            WorldSeed = reader.ReadInt32();
            PlayerObjectNetworkId = reader.ReadInt32();
        }

        public override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);

            writer.Write(WorldSeed);
            writer.Write(PlayerObjectNetworkId);
        }
    }
}
