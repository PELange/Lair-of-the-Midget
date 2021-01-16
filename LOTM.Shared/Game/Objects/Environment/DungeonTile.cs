using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using System.Collections.Generic;

namespace LOTM.Shared.Game.Objects.Environment
{
    public class DungeonTile : TypedObject
    {
        public DungeonTile(int id, ObjectType type, Vector2 position, Vector2 scale)
            : base(id, type, position, 0, scale)
        {
            switch (Type)
            {
                case ObjectType.Tile_TopWall:
                {
                    Components.Add(new Collider(this, new Rectangle(0, 0.5, 1, 0.5)));
                    break;
                }

                case ObjectType.Tile_BottomWall:
                {
                    Components.Add(new Collider(this, new Rectangle(0, 0.8, 1, 0.2)));
                    break;
                }

                case ObjectType.Tile_LeftWall:
                {
                    Components.Add(new Collider(this, new Rectangle(0, 0, 0.25, 1)));
                    break;
                }

                case ObjectType.Tile_RightWall:
                {
                    Components.Add(new Collider(this, new Rectangle(0.75, 0, 0.25, 1)));
                    break;
                }

                case ObjectType.Tile_TopLeftCorner:
                {
                    Components.Add(new Collider(this, new Rectangle(0, 0.5, 1, 0.5)));
                    break;
                }

                case ObjectType.Tile_TopRightCorner:
                {
                    Components.Add(new Collider(this, new Rectangle(0, 0.5, 1, 0.5)));
                    break;
                }

                case ObjectType.Tile_BottomLeftCorner:
                {
                    Components.Add(new Collider(this, new List<Rectangle> { new Rectangle(0, 0.8, 1, 0.2), new Rectangle(0, 0.5, 0.25, 0.3) }));
                    break;
                }

                case ObjectType.Tile_BottomRightCorner:
                {
                    Components.Add(new Collider(this, new List<Rectangle> { new Rectangle(0, 0.8, 1, 0.2), new Rectangle(0.75, 0.5, 0.25, 0.3) }));
                    break;
                }

                case ObjectType.Tile_DoorFrameBottom:
                {
                    Components.Add(new Collider(this, new List<Rectangle>
                    {
                        new Rectangle(0, 0.65, 0.25, 0.5 * 0.2),
                        new Rectangle(0.75, 0.65, 0.25, 0.5 * 0.2),
                        new Rectangle(0.25, 0.5, 0.1, 0.25),
                        new Rectangle(0.7, 0.5, 0.05, 0.25),
                    }));
                    break;
                }

                case ObjectType.Tile_DoorFrameTop:
                {
                    Components.Add(new Collider(this, new List<Rectangle>
                    {
                        new Rectangle(0, 0.5, 0.25, 0.25),
                        new Rectangle(0.75, 0.5, 0.25, 0.25),
                        new Rectangle(0.25, 0.25, 0.1, 0.5),
                        new Rectangle(0.7, 0.25, 0.05, 0.5),
                    }));
                    break;
                }

                case ObjectType.Tile_Pillar:
                {
                    Components.Add(new Collider(this, new Rectangle(0, 0.5, 1, 0.5)));
                    break;
                }
            }
        }
    }
}
