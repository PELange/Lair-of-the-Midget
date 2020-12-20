using LOTM.Shared.Engine.Controls;
using LOTM.Shared.Engine.Network;

namespace LOTM.Shared.Game.Network.Packets
{
    public class PlayerInput : NetworkPacket
    {
        public InputType Inputs { get; set; }
    }
}
