using LOTM.Client.Engine;
using LOTM.Client.Engine.Objects.Components;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Environment;
using System.Collections.Generic;

namespace LOTM.Client.Game.Objects.Environment
{
    class DungeonTileRenderable : DungeonTile
    {
        public DungeonTileRenderable(ObjectType type, Vector2 position, Vector2 scale)
            : base(type, position, scale)
        {
            var spriteSegments = new List<SpriteRenderer.Segment>();

            switch (Type)
            {
                // Ground section
                case ObjectType.Tile_Ground_0:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_tile_0"), null, null, null, 0));
                    break;

                case ObjectType.Tile_Ground_1:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_tile_1"), null, null, null, 0));
                    break;

                case ObjectType.Tile_Ground_2:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_tile_2"), null, null, null, 0));
                    break;

                case ObjectType.Tile_Ground_3:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_tile_3"), null, null, null, 0));
                    break;

                case ObjectType.Tile_Hole:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_tile_hole"), null, null, null, 0));
                    break;

                // Wall section
                case ObjectType.Tile_TopWall:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_wall_standard"), null, null, null, 0));
                    break;

                case ObjectType.Tile_BottomWall:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_wall_standard"), null, null, null, 2000));
                    break;

                case ObjectType.Tile_LeftWall:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_wall_left"), null, null, null, 1200));
                    break;

                case ObjectType.Tile_LeftWallUnderDoor:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_wall_left"), null, null, null, 0));
                    break;

                case ObjectType.Tile_RightWall:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_wall_right"), null, null, null, 1200));
                    break;

                case ObjectType.Tile_RightWallUnderDoor:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_wall_right"), null, null, null, 0));
                    break;

                case ObjectType.Tile_TopLeftCorner:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_corner_top_left"), null, null, null, 0));
                    break;

                case ObjectType.Tile_TopRightCorner:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_corner_top_right"), null, null, null, 0));
                    break;

                case ObjectType.Tile_BottomLeftCorner:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_corner_bottom_left"), null, null, null, 2000));
                    break;

                case ObjectType.Tile_BottomRightCorner:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_corner_bottom_right"), null, null, null, 2000));
                    break;

                // Door section
                case ObjectType.Tile_DoorFrameTop:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_wall_left"), new Vector2(0.25, (float)2 / 3), new Vector2(-0.25, 0.0), null, 0));
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_wall_right"), new Vector2(0.25, (float)2 / 3), new Vector2(0.5, 0.0), null, 0));
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_arch"), new Vector2(0.5, (float)1 / 3), new Vector2(0.0, (float)-1 / 3), null, 2000));
                    break;

                case ObjectType.Tile_DoorFrameBottom:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_wall_left"), new Vector2(0.25, (float)2 / 3), new Vector2(-0.25, 0.0), null, 2000));
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_wall_right"), new Vector2(0.25, (float)2 / 3), new Vector2(0.5, 0.0), null, 2000));
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_arch"), new Vector2(0.5, (float)1 / 3), new Vector2(0.0, (float)-1 / 3), null, 2000));
                    break;

                case ObjectType.Tile_DoorClosed:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_closed")));
                    break;

                case ObjectType.Tile_DoorOpened:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_opened_top"), new Vector2(1, 0.5), new Vector2(0, 0.0), null, 2000));
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_opened_bottom"), new Vector2(1, 0.5), new Vector2(0, 0.5), null, 100));
                    break;

                // Pillar section
                case ObjectType.Tile_Pillar:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_pillar_top"), new Vector2(1, 0.5), new Vector2(0, 0), null, 1100));
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_pillar_bottom"), new Vector2(1, 0.5), new Vector2(0, 0.5), null, 100));
                    break;
            }

            if (spriteSegments.Count > 0)
            {
                Components.Add(new SpriteRenderer(spriteSegments));
            }
        }
    }
}
