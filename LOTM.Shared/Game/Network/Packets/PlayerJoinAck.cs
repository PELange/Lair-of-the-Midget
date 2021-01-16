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

        public bool GameActive { get; set; }
        public int LobbySize { get; set; }
        public int WorldSeed { get; set; }
        public int PlayerObjectId { get; set; }

        public override void ReadBytes(BinaryReader reader)
        {
            base.ReadBytes(reader);

            GameActive = reader.ReadBoolean();
            LobbySize = reader.ReadInt32();
            WorldSeed = reader.ReadInt32();
            PlayerObjectId = reader.ReadInt32();
        }

        public override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);

            writer.Write(GameActive);
            writer.Write(LobbySize);
            writer.Write(WorldSeed);
            writer.Write(PlayerObjectId);
        }
    }
}
