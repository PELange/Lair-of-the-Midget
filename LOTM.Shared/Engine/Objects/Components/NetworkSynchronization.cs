using LOTM.Shared.Engine.Network;
using System.Collections.Generic;

namespace LOTM.Shared.Engine.Objects.Components
{
    public class NetworkSynchronization : IComponent
    {
        public int NetworkId { get; set; }
        public List<NetworkPacket> PacketsInbound { get; }
        public Queue<NetworkPacket> PacketsOutbound { get; }

        public NetworkSynchronization(int networkId)
        {
            NetworkId = networkId;
            PacketsInbound = new List<NetworkPacket>();
            PacketsOutbound = new Queue<NetworkPacket>();
        }
    }
}
