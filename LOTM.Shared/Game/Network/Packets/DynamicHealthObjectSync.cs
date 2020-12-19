using LOTM.Shared.Engine.Network.Packets;

namespace LOTM.Shared.Game.Network.Packets
{
    public class DynamicHealthObjectSync : GameObjectSync
    {
        public double? Health { get; set; }
    }
}
