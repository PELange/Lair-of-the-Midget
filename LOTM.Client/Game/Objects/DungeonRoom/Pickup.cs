﻿using LOTM.Client.Engine;
using LOTM.Client.Engine.Objects.Components;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using System;
using System.Collections.Generic;

namespace LOTM.Client.Game.Objects.DungeonRoom
{
    class Pickup : GameObject
    {
        public Pickup(Random random, Vector2 position = null, double rotation = 0, Vector2 scale = null) : base(position, rotation, scale)
        {
            Random rnd = random;
            int colorIndex = rnd.Next(0, 4);
            int sizeIndex = rnd.Next(0, 2);

            string color;
            string size = sizeIndex == 0 ? "big" : "small";

            switch (colorIndex)
            {
                case 0:
                    color = "orange";
                    break;

                case 1:
                    color = "blue";
                    break;

                case 2:
                    color = "green";
                    break;

                case 3:
                    color = "yellow";
                    break;

                default:
                    color = "";
                    break;
            }

            string pickupName = "pickup_pot_" + color + "_" + size;

            Components.Add(new SpriteRenderer(new List<SpriteRenderer.Segment>
            {
                new SpriteRenderer.Segment(AssetManager.GetSprite(pickupName))
            }));
        }
    }
}
