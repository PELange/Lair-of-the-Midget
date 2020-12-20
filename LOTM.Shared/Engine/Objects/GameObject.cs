using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Network;
using LOTM.Shared.Engine.Network.Packets;
using LOTM.Shared.Engine.Objects.Components;
using System.Collections.Generic;
using System.Linq;

namespace LOTM.Shared.Engine.Objects
{
    public class GameObject
    {
        protected ICollection<IComponent> Components { get; } = new LinkedList<IComponent>();

        public int NetworkId { get; set; }
        public Queue<NetworkPacket> PacketsInbound { get; }
        public Queue<NetworkPacket> PacketsOutbound { get; }
        public bool NetworkSyncFlag { get; set; }
        public enum NetworkInstanceType
        {
            Unknown,
            Local,
            Remote,
            Server
        }
        public NetworkInstanceType InstanceType { get; }

        public GameObject(Vector2 position = null, double rotation = 0, Vector2 scale = null, NetworkInstanceType instanceType = default)
        {
            NetworkId = -1;
            PacketsInbound = new Queue<NetworkPacket>();
            PacketsOutbound = new Queue<NetworkPacket>();
            InstanceType = instanceType;

            if (InstanceType == NetworkInstanceType.Server)
            {
                NetworkSyncFlag = true;
            }

            var transform = new Transformation2D
            {
                Position = position ?? Vector2.ZERO,
                Rotation = rotation,
                Scale = scale ?? new Vector2(1, 1)
            };

            Components.Add(transform);
        }

        public T GetComponent<T>() where T : IComponent
        {
            return (T)Components.Where(x => x is T).FirstOrDefault();
        }

        public virtual void OnBeforeUpdate()
        {
        }

        public virtual void OnFixedUpdate(double deltaTime)
        {
        }

        public virtual void OnUpdate(double deltaTime)
        {
        }

        protected virtual GameObjectSync WriteToNetworkPacket(GameObjectSync packet)
        {
            var transform = GetComponent<Transformation2D>();

            packet.Type = GetType().Name;
            packet.NetworkId = NetworkId;

            if (transform.Position.X != default) packet.PositionX = transform.Position.X;
            if (transform.Position.Y != default) packet.PositionY = transform.Position.Y;
            if (transform.Rotation != default) packet.Rotation = transform.Rotation;
            if (transform.Scale.X != default) packet.ScaleX = transform.Scale.X;
            if (transform.Scale.Y != default) packet.ScaleY = transform.Scale.Y;

            return packet;
        }

        protected virtual void ApplyNetworkPacket(GameObjectSync packet)
        {
            var transform = GetComponent<Transformation2D>();

            NetworkId = packet.NetworkId;
            transform.Position.X = packet.PositionX ?? transform.Position.X;
            transform.Position.Y = packet.PositionY ?? transform.Position.Y;
            transform.Rotation = packet.Rotation ?? transform.Rotation;
            transform.Scale.X = packet.ScaleX ?? transform.Scale.X;
            transform.Scale.Y = packet.ScaleY ?? transform.Scale.Y;
        }
    }
}
