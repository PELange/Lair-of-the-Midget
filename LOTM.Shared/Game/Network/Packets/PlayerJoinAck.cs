using LOTM.Shared.Engine.Network;

namespace LOTM.Shared.Game.Network.Packets
{
    public class PlayerJoinAck : NetworkPacket
    {
        public int WorldSeed { get; set; }
        public int PlayerGameObjectId { get; set; }
    }
}
