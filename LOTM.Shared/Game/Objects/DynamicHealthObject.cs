using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects.Components;

namespace LOTM.Shared.Game.Objects
{
    public abstract class DynamicHealthObject : GameObject
    {
        public DynamicHealthObject(Vector2 position = null, double rotation = 0, Vector2 scale = null, NetworkInstanceType instanceType = default, double health = default)
            : base(position, rotation, scale, instanceType)
        {
            Components.Add(new Health());
        }

        protected virtual DynamicHealthObjectSync WriteToNetworkPacket(DynamicHealthObjectSync packet)
        {
            base.WriteToNetworkPacket(packet);

            packet.Health = GetComponent<Health>().Value;

            return packet;
        }

        protected virtual bool ApplyNetworkPacket(DynamicHealthObjectSync packet)
        {
            if (!base.ApplyNetworkPacket(packet)) return false;

            var healthComponent = GetComponent<Health>();

            healthComponent.Value = packet.Health ?? healthComponent.Value;

            return true;
        }
    }
}
