using LOTM.Shared.Engine.Network;
using System.Text.Json.Serialization;

namespace LOTM.Shared.Game.Network.Packets
{
    public class PlayerJoin : NetworkPacket
    {
        public string PlayerName { get; set; }
    }
}
