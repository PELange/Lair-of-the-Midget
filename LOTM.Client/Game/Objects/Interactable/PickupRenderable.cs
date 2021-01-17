using LOTM.Client.Engine;
using LOTM.Client.Engine.Objects.Components;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Engine.World;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Interactable;
using System.Collections.Generic;
using System.Linq;

namespace LOTM.Client.Game.Objects.Interactable
{
    class PickupRenderable : Pickup
    {
        public int LastStatePacketId { get; set; }

        public PickupRenderable(int id, ObjectType type, Vector2 position)
            : base(id, type, position)
        {
            string pickupName = "";

            switch (Type)
            {
                case ObjectType.Pickup_Health_Minor:
                    pickupName = "pickup_pot_orange_small";
                    break;
                case ObjectType.Pickup_Health_Major:
                    pickupName = "pickup_pot_orange_big";
                    break;
                case ObjectType.Pickup_Revive_Major:
                    pickupName = "pickup_pot_green_big";
                    break;
            }

            if (!string.IsNullOrEmpty(pickupName))
            {
                AddComponent(new SpriteRenderer(new List<SpriteRenderer.Segment>
                {
                    new SpriteRenderer.Segment(AssetManager.GetSprite(pickupName))
                }));
            }
        }

        public override void OnFixedUpdate(double deltaTime, GameWorld world)
        {
            //Process inbound packets
            var networkSynchronization = GetComponent<NetworkSynchronization>();

            //1. Check for state changes and only apply the latest one
            if (networkSynchronization.PacketsInbound.Where(x => x is PickupStateUpdate).OrderByDescending(x => x.Id).FirstOrDefault() is PickupStateUpdate pickupStateUpdate)
            {
                //Only accept the pickup state update, if the packet id is larger than the last known update about it. This avoids retransmission issues.
                if (pickupStateUpdate.Id > LastStatePacketId)
                {
                    Active = pickupStateUpdate.Active;

                    GetComponent<Collider>().Active = Active;
                    GetComponent<SpriteRenderer>().Segments[0].Active = Active;
                }
            }

            networkSynchronization.PacketsInbound.Clear();
        }
    }
}
