using LOTM.Client.Engine;
using LOTM.Client.Engine.Objects;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using System;
using System.Collections.Generic;
using static LOTM.Client.Game.Objects.DungeonRoom.DungeonRoom;

namespace LOTM.Client.Game.Objects.DungeonRoom
{
    class DungeonTile : GameObject
    {

        public DungeonTile(TileType tileType, Random random, Vector2 position = null, double rotation = 0, Vector2 scale = null) : base(position, rotation, scale)
        {
            List<SpriteRenderer.Segment> spriteSegments = new List<SpriteRenderer.Segment>();
            Random rnd = random;
            switch (tileType)
            {
                // Ground section
                case TileType.Ground:
                    int tileNum = rnd.Next(0, 4);
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_tile_" + tileNum), null, null, null, 0));
                    break;

                case TileType.Hole:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_tile_hole"), null, null, null, 0));
                    break;

                // Wall section
                case TileType.TopWall:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_wall_standard"), null, null, null, 0));
                    break;

                case TileType.BottomWall:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_wall_standard"), null, null, null, 2000));
                    break;

                case TileType.LeftWall:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_wall_left"), null, null, null, 1200));
                    break;

                case TileType.LeftWallUnderDoor:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_wall_left"), null, null, null, 0));
                    break;

                case TileType.RightWall:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_wall_right"), null, null, null, 1200));
                    break;

                case TileType.RightWallUnderDoor:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_wall_right"), null, null, null, 0));
                    break;

                case TileType.TopLeftCorner:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_corner_top_left"), null, null, null, 0));
                    break;

                case TileType.TopRightCorner:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_corner_top_right"), null, null, null, 0));
                    break;

                case TileType.BottomLeftCorner:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_corner_bottom_left"), null, null, null, 2000));
                    break;

                case TileType.BottomRightCorner:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_corner_bottom_right"), null, null, null, 2000));
                    break;

                // Door section
                case TileType.DoorFrameTop:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_wall_left"), new Vector2(0.25, (float)2 / 3), new Vector2(-0.25, 0.0), null, 0));
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_wall_right"), new Vector2(0.25, (float)2 / 3), new Vector2(0.5, 0.0), null, 0));
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_arch"), new Vector2(0.5, (float)1 / 3), new Vector2(0.0, (float)-1 / 3), null, 2000));
                    break;

                case TileType.DoorFrameBottom:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_wall_left"), new Vector2(0.25, (float)2 / 3), new Vector2(-0.25, 0.0), null, 2000));
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_wall_right"), new Vector2(0.25, (float)2 / 3), new Vector2(0.5, 0.0), null, 2000));
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_arch"), new Vector2(0.5, (float)1 / 3), new Vector2(0.0, (float)-1 / 3), null, 2000));
                    break;

                case TileType.DoorClosed:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_closed")));
                    break;

                case TileType.DoorOpened:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_opened_top"), new Vector2(1, 0.5), new Vector2(0, 0.0), null, 2000));
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_opened_bottom"), new Vector2(1, 0.5), new Vector2(0, 0.5), null, 0));
                    break;

                // Pillar section
                case TileType.Pillar:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_pillar_top"), new Vector2(1, 0.5), new Vector2(0, 0), null, 1100));
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_pillar_bottom"), new Vector2(1, 0.5), new Vector2(0, 0.5), null, 100));
                    break;

                default:
                    break;

            }

            Components.Add(new SpriteRenderer(spriteSegments));
        }
    }
}
