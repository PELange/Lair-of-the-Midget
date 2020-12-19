using LOTM.Shared.Engine.Network;

namespace LOTM.Shared.Game.Network.Packets
{
    public class PlayerJoin : NetworkPacket
    {
        public string PlayerName { get; set; }
    }
}
