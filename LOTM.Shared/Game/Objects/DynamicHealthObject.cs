using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using LOTM.Shared.Game.Network.Packets;

namespace LOTM.Shared.Game.Objects
{
    public abstract class DynamicHealthObject : GameObject
    {
        public DynamicHealthObject(Vector2 position = null, double rotation = 0, Vector2 scale = null, NetworkInstanceType instanceType = default, double health = default)
            : base(position, rotation, scale, instanceType)
        {
            Health = health;
        }

        public double Health { get; set; }

        protected virtual DynamicHealthObjectSync WriteToNetworkPacket(DynamicHealthObjectSync packet)
        {
            base.WriteToNetworkPacket(packet);

            packet.Health = Health;

            return packet;
        }

        protected virtual void ApplyNetworkPacket(DynamicHealthObjectSync packet)
        {
            base.ApplyNetworkPacket(packet);

            Health = packet.Health ?? Health;
        }
    }
}
