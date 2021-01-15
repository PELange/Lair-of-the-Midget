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
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_tile_0"), layer: 0));
                    break;

                case ObjectType.Tile_Ground_1:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_tile_1"), layer: 0));
                    break;

                case ObjectType.Tile_Ground_2:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_tile_2"), layer: 0));
                    break;

                case ObjectType.Tile_Ground_3:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_tile_3"), layer: 0));
                    break;

                case ObjectType.Tile_Hole:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_tile_hole"), layer: 0));
                    break;

                // Wall section
                case ObjectType.Tile_TopWall:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_wall_standard"), layer: 0));
                    break;

                case ObjectType.Tile_BottomWall:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_wall_standard"), layer: 2000));
                    break;

                case ObjectType.Tile_LeftWall:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_wall_left"), layer: 1200));
                    break;

                case ObjectType.Tile_LeftWallUnderDoor:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_wall_left"), layer: 0));
                    break;

                case ObjectType.Tile_RightWall:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_wall_right"), layer: 1200));
                    break;

                case ObjectType.Tile_RightWallUnderDoor:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_wall_right"), layer: 0));
                    break;

                case ObjectType.Tile_TopLeftCorner:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_corner_top_left"), layer: 0));
                    break;

                case ObjectType.Tile_TopRightCorner:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_corner_top_right"), layer: 0));
                    break;

                case ObjectType.Tile_BottomLeftCorner:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_corner_bottom_left"), layer: 2000));
                    break;

                case ObjectType.Tile_BottomRightCorner:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_corner_bottom_right"), layer: 2000));
                    break;

                // Door section
                case ObjectType.Tile_DoorFrameTop:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_wall_left"), new Vector2(0.25, 0.5), new Vector2(0, 0.25), layer: 0));
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_wall_right"), new Vector2(0.25, 0.5), new Vector2(0.75, 0.25), layer: 0));
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_arch"), new Vector2(0.5, 0.25), new Vector2(0.25, 0), layer: 2000));

                    break;

                case ObjectType.Tile_DoorFrameBottom:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_wall_left"), new Vector2(0.25, 0.5), new Vector2(0, 0.25), layer: 2000));
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_wall_right"), new Vector2(0.25, 0.5), new Vector2(0.75, 0.25), layer: 2000));
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_arch"), new Vector2(0.5, 0.25), new Vector2(0.25, 0), layer: 2000));
                    break;

                case ObjectType.Tile_DoorClosed:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_closed")));
                    break;

                case ObjectType.Tile_DoorOpened:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_opened_top"), new Vector2(1, 0.5), new Vector2(0, 0.0), layer: 2000));
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_opened_bottom"), new Vector2(1, 0.5), new Vector2(0, 0.5), layer: 100));
                    break;

                // Pillar section
                case ObjectType.Tile_Pillar:
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_pillar_top"), new Vector2(1, 0.5), new Vector2(0, 0), layer: 1100));
                    spriteSegments.Add(new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_pillar_bottom"), new Vector2(1, 0.5), new Vector2(0, 0.5), layer: 100));
                    break;
            }

            if (spriteSegments.Count > 0)
            {
                Components.Add(new SpriteRenderer(spriteSegments));
            }
        }
    }
}
