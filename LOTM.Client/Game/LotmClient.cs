using LOTM.Client.Engine;
using LOTM.Client.Engine.Controls;
using LOTM.Client.Game.Network;
using LOTM.Client.Game.Objects.DungeonRoom;
using LOTM.Client.Game.Objects.Player;
using LOTM.Shared.Engine.Controls;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using System.Collections.Generic;
using static LOTM.Client.Engine.Graphics.OrthographicCamera;

namespace LOTM.Client.Game
{
    public class LotmClient : GuiGame
    {
        protected List<Vector2> RoomCoordsList = new List<Vector2>();

        protected LotmNetworkManagerClient NetworkClient { get; set; }

        protected int PlayerGameObjectId { get; set; }
        protected GameObject PlayerObject { get; set; }

        public LotmClient(int windowWidth, int windowHeight, string connectionString, string playerName)
            : base(windowWidth, windowHeight, "Lair of the Midget", "Game/Assets/Textures/icon.png", new LotmNetworkManagerClient(connectionString, playerName))
        {
            NetworkClient = (LotmNetworkManagerClient)NetworkManager;
            PlayerGameObjectId = -1;
        }

        protected override void OnInit()
        {
            //Register main texture atlas
            AssetManager.RegisterTexture("Game/Assets/Textures/0x72_DungeonTilesetII_v1.3.png", "dungeonTiles");

            //Register indivual sprites on the atlas using 16x16 grid indices
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(1, 23, 1, 23), "demonboss_idle_0_1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(2, 23, 2, 23), "demonboss_idle_0_2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(1, 24, 1, 24), "demonboss_idle_0_3");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(2, 24, 2, 24), "demonboss_idle_0_4");

            //Small Skeleton
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(23, 4, 23, 5), "skeleton_small_m_idle_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(24, 4, 24, 5), "skeleton_small_m_idle_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(25, 4, 25, 5), "skeleton_small_m_idle_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(25, 4, 25, 5), "skeleton_small_m_idle_anim_f3");

            //Small Ogre
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(23, 12, 23, 13), "ogre_small_m_idle_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(24, 12, 24, 13), "ogre_small_m_idle_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(25, 12, 25, 13), "ogre_small_m_idle_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(25, 12, 25, 13), "ogre_small_m_idle_anim_f3");


            // Green Blob
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(27, 6, 27, 7), "blob_green_m_idle_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(28, 6, 28, 7), "blob_green_m_idle_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(29, 6, 29, 7), "blob_green_m_idle_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(30, 6, 30, 7), "blob_green_m_idle_anim_f3");


            //Wizard
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(8, 10, 8, 11), "wizzard_m_idle_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(9, 10, 9, 11), "wizzard_m_idle_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(10, 10, 10, 11), "wizzard_m_idle_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(11, 10, 11, 11), "wizzard_m_idle_anim_f3");

            //Dungeon room tiles
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(1, 4, 1, 4), "dungeon_tile_0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(2, 4, 2, 4), "dungeon_tile_1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(3, 4, 3, 4), "dungeon_tile_2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(2, 5, 2, 5), "dungeon_tile_3");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(6, 9, 6, 9), "dungeon_tile_hole");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(1, 8, 1, 8), "dungeon_wall_left");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(0, 8, 0, 8), "dungeon_wall_right");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(1, 0, 1, 1), "dungeon_wall_standard");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(2, 7, 2, 8), "dungeon_corner_top_left");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(3, 7, 3, 8), "dungeon_corner_top_right");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(2, 9, 2, 10), "dungeon_corner_bottom_left");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(3, 9, 3, 10), "dungeon_corner_bottom_right");


            // Door tiles
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(2, 14, 3, 15), "dungeon_door_closed");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(5, 15, 6, 15), "dungeon_door_opened_bottom");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(5, 14, 6, 14), "dungeon_door_opened_top");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(2, 13, 3, 13), "dungeon_door_arch");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(1, 14, 1, 15), "dungeon_door_wall_left");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(4, 14, 4, 15), "dungeon_door_wall_right");


            // Pillar
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(5, 5, 5, 5), "dungeon_pillar_top");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(5, 6, 5, 6), "dungeon_pillar_bottom");

            //PickUps
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(18, 14, 18, 14), "pickup_pot_orange_big");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(18, 15, 18, 15), "pickup_pot_orange_small");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(19, 14, 19, 14), "pickup_pot_blue_big");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(19, 15, 19, 15), "pickup_pot_blue_small");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(20, 14, 20, 14), "pickup_pot_green_big");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(20, 15, 20, 15), "pickup_pot_green_small");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(21, 14, 21, 14), "pickup_pot_yellow_big");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(21, 15, 21, 15), "pickup_pot_yellow_small");
        }

        protected override void OnBeforeUpdate()
        {
            base.OnBeforeUpdate();

            //todo free up distant world object
        }

        protected override void OnFixedUpdate(double deltaTime)
        {
            //Handle incoming packets
            while (NetworkManager.TryGetPacket(out var inbound))
            {
                switch (inbound)
                {
                    //Received an answer to join
                    case PlayerJoinAck playerJoinAck:
                    {
                        if (NetworkClient.OnPlayerJoinAck(playerJoinAck))
                        {
                            OnJoin(playerJoinAck.WorldSeed, playerJoinAck.PlayerObjectNetworkId);
                        }

                        break;
                    }

                    //Player or enemy object creation or full state update
                    case MovingHealthObjectUpdate movingHealthObjectUpdate:
                    {
                        //Try to loctate the object using the network id
                        var gameObject = World.GetGameObjectByNetworkId(movingHealthObjectUpdate.ObjectId);

                        //No known gameobject with that id yet -> Create
                        if (gameObject == null)
                        {
                            if (movingHealthObjectUpdate.Type == MovingHealthObjectType.PLAYER_WIZARD)
                            {
                                gameObject = new PlayerBaseClient(
                                    movingHealthObjectUpdate.ObjectId,
                                    MovingHealthObjectType.PLAYER_WIZARD,
                                    new Vector2(movingHealthObjectUpdate.PositionX, movingHealthObjectUpdate.PositionY),
                                    new Vector2(movingHealthObjectUpdate.ScaleX, movingHealthObjectUpdate.ScaleY),
                                    movingHealthObjectUpdate.Health);

                                World.AddObject(gameObject);
                            }
                        }
                        else
                        {
                            gameObject.GetComponent<NetworkSynchronization>().PacketsInbound.Add(movingHealthObjectUpdate);
                        }

                        break;
                    }

                    case ObjectHealthUpdate objectHealthUpdate:
                    {
                        var gameObject = World.GetGameObjectByNetworkId(objectHealthUpdate.ObjectId);

                        if (gameObject != null)
                        {
                            gameObject.GetComponent<NetworkSynchronization>().PacketsInbound.Add(objectHealthUpdate);
                        }

                        break;
                    }

                    case ObjectPositionUpdate objectPositionUpdate:
                    {
                        var gameObject = World.GetGameObjectByNetworkId(objectPositionUpdate.ObjectId);

                        if (gameObject != null)
                        {
                            gameObject.GetComponent<NetworkSynchronization>().PacketsInbound.Add(objectPositionUpdate);
                        }

                        break;
                    }
                }
            }

            //Make sure we are "connected" to the server -> If we did not get an ack from server yet, resend our join request.
            NetworkClient.EnsureServerConnection();

            //Poll controls and send input to server if needed
            PollInputs();

            //Run fixed simulation on all relevant world objects
            foreach (var worldObject in World.GetAllObjects())
            {
                worldObject.OnFixedUpdate(FixedUpdateDeltaTime);
            }

            //Update camera ... todo make it follow the player
            UpdateCamera();
        }

        protected override void OnUpdate(double deltaTime)
        {
            foreach (var worldObject in World.GetAllObjects())
            {
                worldObject.OnUpdate(deltaTime);
            }
        }

        void OnJoin(int seed, int playerGameObjectId)
        {
            System.Console.WriteLine($"Successfully joined {NetworkClient.CurrentServer}");

            PlayerGameObjectId = playerGameObjectId;

            int playerCount = 5; // Get num of connected players to spawn more or less pickups and enemys
            int roomCount = 3;
            int roomWidth = 10 + playerCount;
            int roomHeight = 10 + playerCount;
            int tunnelLength = 5;
            Vector2 roomCoords;
            // Create rooms
            for (int i = 0; i < roomCount; i++)
            {
                roomCoords = new Vector2(0, -i * (roomHeight + tunnelLength) * 16 + 32);
                RoomCoordsList.Add(roomCoords);
                DungeonRoom dungeonRoom = new DungeonRoom(roomCoords, roomWidth, roomHeight, tunnelLength, playerCount, seed);
                if (i == 0) dungeonRoom.CreateDungeonEntrance();

                foreach (var tile in dungeonRoom.DungeonObjectList)
                {
                    World.AddObject(tile);
                }
            }

            //World.Objects.Add(new DemonBoss(new Vector2(160, 160), 45, new Vector2(32, 32)));
            //World.Objects.Add(new WizardOfWisdom(new Vector2(6 * 16, 6 * 16), 0, new Vector2(16, 16 * 2)));
        }

        void PollInputs()
        {
            var inputs = InputType.NONE;

            if (InputManager.WasControlPressed(InputType.WALK_LEFT))
            {
                inputs |= InputType.WALK_LEFT;
            }
            else if (InputManager.WasControlPressed(InputType.WALK_RIGHT))
            {
                inputs |= InputType.WALK_RIGHT;
            }

            if (InputManager.WasControlPressed(InputType.WALK_UP))
            {
                inputs |= InputType.WALK_UP;
            }
            else if (InputManager.WasControlPressed(InputType.WALK_DOWN))
            {
                inputs |= InputType.WALK_DOWN;
            }

            if (inputs != InputType.NONE) NetworkClient.SendPacket(new PlayerInput { Inputs = inputs });

            //Clear all events such as button presses, as we processed them all for this frame.
            InputManager.ClearEvents();
        }

        void UpdateCamera()
        {
            //Try find the player object if we did not already have it
            if (PlayerObject == null)
            {
                PlayerObject = World.GetGameObjectByNetworkId(PlayerGameObjectId);
            }

            if (PlayerObject != null)
            {
                var transformation = PlayerObject.GetComponent<Transformation2D>();

                var cameraCenterPos = Vector2.ZERO;
                cameraCenterPos.X += transformation.Position.X + transformation.Scale.X / 2;
                cameraCenterPos.Y += transformation.Position.Y + transformation.Scale.Y / 2;

                var viewportPadding = 16 * 10;

                Camera.SetViewport(new Viewport(
                    new Vector2(cameraCenterPos.X - viewportPadding, cameraCenterPos.Y - viewportPadding),
                    new Vector2(cameraCenterPos.X + viewportPadding, cameraCenterPos.Y + viewportPadding)));
            }
        }
    }
}
