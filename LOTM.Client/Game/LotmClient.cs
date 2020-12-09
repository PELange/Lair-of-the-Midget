using LOTM.Client.Engine;
using LOTM.Client.Engine.Controls;
using LOTM.Client.Game.Objects;
using LOTM.Client.Game.Objects.DungeonRoom;
using LOTM.Shared.Engine.Math;

namespace LOTM.Client.Game
{
    public class LotmClient : GuiGame
    {
        public LotmClient(int windowWidth, int windowHeight) : base(windowWidth, windowHeight, "Lair of the Midget", "Game/Assets/Textures/icon.png")
        {
        }

        protected override void OnInit()
        {
            //Register main texture atlas
            AssetManager.RegisterTexture("Game/Assets/Textures/0x72_DungeonTilesetII_v1.3.png", "dungeonTiles");

            //Register indivual sprites on the atlas using 16x16 grid indices
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(1, 23, 2, 24), "demonboss_idle_0");

            //Wizard
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(8, 10, 8, 11), "wizzard_m_idle_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(9, 10, 9, 11), "wizzard_m_idle_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(10, 10, 10, 11), "wizzard_m_idle_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(11, 10, 11, 11), "wizzard_m_idle_anim_f3");

            //Dungeon Room Tiles
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(1, 4, 1, 4), "dungeon_tile_0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(2, 4, 2, 4), "dungeon_tile_1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(3, 4, 3, 4), "dungeon_tile_2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(2, 5, 2, 5), "dungeon_tile_3");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(1, 7, 1, 9), "dungeon_wall_left");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(0, 7, 0, 9), "dungeon_wall_right");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(1, 0, 1, 1), "dungeon_wall_standard");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(2, 7, 2, 8), "dungeon_corner_top_left");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(3, 7, 3, 8), "dungeon_corner_top_right");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(2, 9, 2, 10), "dungeon_corner_bottom_left");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(3, 9, 3, 10), "dungeon_corner_bottom_right");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(2, 14, 3, 15), "dungeon_door_closed");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(5, 15, 6, 15), "dungeon_door_opened_bottom");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(5, 14, 6, 14), "dungeon_door_opened_top");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(2, 13, 3, 13), "dungeon_door_arch");

            //PickUps
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(18, 14, 18, 14), "pickup_pot_orange_big");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(18, 15, 18, 15), "pickup_pot_orange_small");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(19, 14, 19, 14), "pickup_pot_blue_big");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(19, 15, 19, 15), "pickup_pot_blue_small");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(20, 14, 20, 14), "pickup_pot_green_big");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(20, 15, 20, 15), "pickup_pot_green_small");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(21, 14, 21, 14), "pickup_pot_yellow_big");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(21, 15, 21, 15), "pickup_pot_yellow_small");


            //var seed = System.Guid.NewGuid().GetHashCode();
            var seed = 125;
            int roomCount = 1;
            for (int i = 0; i < roomCount; i++)
            {
                DungeonRoom dungeonRoom = new DungeonRoom(new Vector2(32, 32), 10, 10, seed);

                foreach (var tile in dungeonRoom.TileList)
                {
                    World.Objects.Add(tile);
                }
            }


            
            //World.Objects.Add(new DemonBoss(new Vector2(160, 160), 45, new Vector2(32, 32)));
            World.Objects.Add(new WizardOfWisdom(new Vector2(6 * 16, 6 * 16), 0, new Vector2(16, 16 * 2)));

        }

        protected override void OnFixedUpdate(double deltaTime)
        {
            var cameraMovementSpeed = 100;
            if (InputManager.IsControlPressed(InputManager.ControlType.WALK_LEFT))
            {
                Camera.PanViewport(new Vector2(-cameraMovementSpeed * deltaTime, 0));
            }
            else if (InputManager.IsControlPressed(InputManager.ControlType.WALK_RIGHT))
            {
                Camera.PanViewport(new Vector2(cameraMovementSpeed * deltaTime, 0));
            }

            if (InputManager.IsControlPressed(InputManager.ControlType.WALK_UP))
            {
                Camera.PanViewport(new Vector2(0, -cameraMovementSpeed * deltaTime));
            }
            else if (InputManager.IsControlPressed(InputManager.ControlType.WALK_DOWN))
            {
                Camera.PanViewport(new Vector2(0, cameraMovementSpeed * deltaTime));
            }
        }

        protected override void OnUpdate(double deltaTime)
        {

        }
    }
}
