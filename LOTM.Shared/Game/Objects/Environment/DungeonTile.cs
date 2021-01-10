using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using System.Collections.Generic;

namespace LOTM.Shared.Game.Objects.Environment
{
    public class DungeonTile : TypedObject
    {
        public DungeonTile(ObjectType type, Vector2 position, Vector2 scale)
            : base(type, position, 0, scale)
        {
            switch (Type)
            {
                case ObjectType.Tile_TopWall:
                {
                    Components.Add(new Collider(this, new BoundingBox(0, 0.5, 1, 0.5)));
                    break;
                }

                case ObjectType.Tile_BottomWall:
                {
                    Components.Add(new Collider(this, new BoundingBox(0, 0.8, 1, 0.2)));
                    break;
                }

                case ObjectType.Tile_LeftWall:
                {
                    Components.Add(new Collider(this, new BoundingBox(0, 0, 0.25, 1)));
                    break;
                }

                case ObjectType.Tile_RightWall:
                {
                    Components.Add(new Collider(this, new BoundingBox(0.75, 0, 0.25, 1)));
                    break;
                }

                case ObjectType.Tile_TopLeftCorner:
                {
                    Components.Add(new Collider(this, new BoundingBox(0, 0.5, 1, 0.5)));
                    break;
                }

                case ObjectType.Tile_TopRightCorner:
                {
                    Components.Add(new Collider(this, new BoundingBox(0, 0.5, 1, 0.5)));
                    break;
                }

                case ObjectType.Tile_BottomLeftCorner:
                {
                    Components.Add(new Collider(this, new List<BoundingBox> { new BoundingBox(0, 0.8, 1, 0.2), new BoundingBox(0, 0.5, 0.25, 0.3) }));
                    break;
                }

                case ObjectType.Tile_BottomRightCorner:
                {
                    Components.Add(new Collider(this, new List<BoundingBox> { new BoundingBox(0, 0.8, 1, 0.2), new BoundingBox(0.75, 0.5, 0.25, 0.3) }));
                    break;
                }

                case ObjectType.Tile_DoorFrameBottom:
                {
                    Components.Add(new Collider(this, new List<BoundingBox>
                    {
                        new BoundingBox(-0.25, 0.535, 0.25, 0.13),
                        new BoundingBox(0, 0.3, 0.1, 0.37),
                        new BoundingBox(0.45, 0.3, 0.05, 0.37),
                        new BoundingBox(0.5, 0.535, 0.25, 0.13)
                    }));
                    break;
                }


                case ObjectType.Tile_DoorFrameTop:
                {
                    Components.Add(new Collider(this, new List<BoundingBox>
                    {
                        new BoundingBox(-0.25, 0.337, 0.25, 0.33),
                        new BoundingBox(0, 0, 0.1, 0.67),
                        new BoundingBox(0.45, 0, 0.05, 0.67),
                        new BoundingBox(0.5, 0.337, 0.25, 0.33)
                    }));
                    break;
                }

                case ObjectType.Tile_DoorClosed:
                {
                    Components.Add(new Collider(this, new BoundingBox(0, 0, 1, 1)));
                    break;
                }

                case ObjectType.Tile_Pillar:
                {
                    Components.Add(new Collider(this, new BoundingBox(0, 0.5, 1, 0.5)));
                    break;
                }
            }
        }
    }
}
