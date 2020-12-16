using LOTM.Client.Engine;
using LOTM.Client.Engine.Objects;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using static LOTM.Client.Game.Objects.DungeonRoom.DungeonRoom;

namespace LOTM.Client.Game.Objects.DungeonRoom
{
    class DungeonTile : GameObject
    {

        public DungeonTile(TileType tileType, Random random, Vector2 position = null, double rotation = 0, Vector2 scale = null) : base(position, rotation, scale)
        {
            string tileName;
            Random rnd = random;
            switch (tileType)
            {
                case TileType.Ground:
                    int tileNum = rnd.Next(0, 4);
                    tileName = "dungeon_tile_" + tileNum;
                    break;

                case TileType.Hole:
                    tileName = "dungeon_tile_hole";
                    break;

                case TileType.StandardWall:
                    tileName = "dungeon_wall_standard";
                    break;

                case TileType.LeftWall:
                    tileName = "dungeon_wall_left";
                    break;
                    
                case TileType.RightWall:
                    tileName = "dungeon_wall_right";
                    break;

                case TileType.TopLeftCorner:
                    tileName = "dungeon_corner_top_left";
                    break;

                case TileType.TopRightCorner:
                    tileName = "dungeon_corner_top_right";
                    break;

                case TileType.BottomLeftCorner:
                    tileName = "dungeon_corner_bottom_left";
                    break;

                case TileType.BottomRightCorner:
                    tileName = "dungeon_corner_bottom_right";
                    break;

                case TileType.DoorClosed:
                    tileName = "dungeon_door_closed";
                    break;

                case TileType.DoorOpenedBottom:
                    tileName = "dungeon_door_opened_bottom";
                    break;

                case TileType.DoorOpenedTop:
                    tileName = "dungeon_door_opened_top";
                    break;

                case TileType.DoorArch:
                    tileName = "dungeon_door_arch";
                    break;

                case TileType.Pillar:
                    tileName = "dungeon_pillar";
                    break;

                default:
                    tileName = "";
                    break;

            }

            Components.Add(new SpriteRenderer(AssetManager.GetSprite(tileName)));

        }

        public override void OnFixedUpdate(double deltaTime)
        {
        }

        public override void OnUpdate(double deltaTime)
        {
        }
    }
}
