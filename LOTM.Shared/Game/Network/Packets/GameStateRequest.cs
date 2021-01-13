using LOTM.Shared.Engine.Network;
using System.Net;

namespace LOTM.Shared.Game.Network.Packets
{
    public class GameStateRequest : NetworkPacket
    {
        public GameStateRequest(IPEndPoint sender = default) : base(sender)
        {
        }
    }
}
