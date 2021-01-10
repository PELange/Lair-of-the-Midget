using LOTM.Client.Engine;
using LOTM.Client.Engine.Objects.Components;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Interactable;
using System.Collections.Generic;

namespace LOTM.Client.Game.Objects.Interactable
{
    class PickupRenderable : Pickup
    {
        public PickupRenderable(ObjectType type, Vector2 position)
            : base(type, position)
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
            }

            if (!string.IsNullOrEmpty(pickupName))
            {
                AddComponent(new SpriteRenderer(new List<SpriteRenderer.Segment>
                {
                    new SpriteRenderer.Segment(AssetManager.GetSprite(pickupName))
                }));
            }
        }
    }
}
