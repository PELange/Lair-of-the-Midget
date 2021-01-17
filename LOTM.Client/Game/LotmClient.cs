using LOTM.Client.Engine;
using LOTM.Client.Engine.Controls;
using LOTM.Client.Engine.Graphics;
using LOTM.Client.Engine.Objects;
using LOTM.Client.Game.Network;
using LOTM.Client.Game.Objects;
using LOTM.Client.Game.Objects.Environment;
using LOTM.Client.Game.Objects.Interactable;
using LOTM.Client.Game.Objects.Player;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Game.Logic;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Components;
using LOTM.Shared.Game.Objects.Environment;
using LOTM.Shared.Game.Objects.Interactable;
using System;
using System.Collections.Generic;
using System.Linq;
using static LOTM.Client.Engine.Graphics.OrthographicCamera;

namespace LOTM.Client.Game
{
    public class LotmClient : GuiGame
    {
        protected LotmNetworkManagerClient NetworkClient { get; set; }

        protected int PlayerGameObjectId { get; set; }
        protected PlayerBaseClient PlayerObject { get; set; }
        protected TextCanvas TextCanvas { get; set; }

        protected int LobbySize { get; set; }
        protected int WorldSeed { get; set; }
        protected List<DungeonRoom> DungeonRooms { get; }

        protected ObjectType DesiredPlayerType { get; set; }

        protected enum GameState
        {
            Connecting,
            Lobby,
            Gameplay
        }

        protected GameState State { get; set; }

        public LotmClient(int windowWidth, int windowHeight, string connectionString, string playerName, string desiredPlayerType)
            : base(windowWidth, windowHeight, "Lair of the Midget", "Game/Assets/Textures/icon.png", new LotmNetworkManagerClient(connectionString))
        {
            NetworkClient = (LotmNetworkManagerClient)NetworkManager;
            PlayerGameObjectId = -1;
            DungeonRooms = new List<DungeonRoom>();
            State = GameState.Connecting;

            NetworkClient.SendPacket(new PlayerJoin
            {
                RequiresAck = true, //Guarantee delivery
                PlayerName = playerName,
                PlayerType = desiredPlayerType.ToLower() switch
                {
                    "elf" => ObjectType.Player_Elf_Female,
                    "wizard" => ObjectType.Player_Wizard_Male,
                    _ => ObjectType.Player_Knight_Male
                }
            });
        }

        protected override void OnInit()
        {
            //Register main texture atlas
            if (!AssetManager.RegisterTexture("Game/Assets/Textures/0x72_DungeonTilesetII_v1.3.png", "dungeonTiles")) return;

            //Register fonts
            if (!AssetManager.RegisterFont("Game/Assets/Fonts/Showcard Gothic.ttf", 64, "showcard_gothic")) return;
            if (!AssetManager.RegisterFont("Game/Assets/Fonts/Arial.ttf", 64, "arial")) return;

            //Register indivual sprites on the atlas using 16x16 grid indices

            //Plain white
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(30, 30, 30, 30), "solid_white");

            //Players

            //Elf
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(8, 0, 8, 1), "elf_f_idle_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(9, 0, 9, 1), "elf_f_idle_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(10, 0, 10, 1), "elf_f_idle_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(11, 0, 11, 1), "elf_f_idle_anim_f3");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(12, 0, 12, 1), "elf_f_walk_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(13, 0, 13, 1), "elf_f_walk_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(14, 0, 14, 1), "elf_f_walk_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(15, 0, 15, 1), "elf_f_walk_anim_f3");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(16, 0, 16, 1), "elf_f_jump_anim_f0");

            //Knight
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(8, 6, 8, 7), "knight_m_idle_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(9, 6, 9, 7), "knight_m_idle_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(10, 6, 10, 7), "knight_m_idle_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(11, 6, 11, 7), "knight_m_idle_anim_f3");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(12, 6, 12, 7), "knight_m_walk_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(13, 6, 13, 7), "knight_m_walk_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(14, 6, 14, 7), "knight_m_walk_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(15, 6, 15, 7), "knight_m_walk_anim_f3");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(16, 6, 16, 7), "knight_m_jump_anim_f0");

            //Wizard
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(8, 10, 8, 11), "wizzard_m_idle_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(9, 10, 9, 11), "wizzard_m_idle_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(10, 10, 10, 11), "wizzard_m_idle_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(11, 10, 11, 11), "wizzard_m_idle_anim_f3");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(12, 10, 12, 11), "wizzard_m_walk_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(13, 10, 13, 11), "wizzard_m_walk_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(14, 10, 14, 11), "wizzard_m_walk_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(15, 10, 15, 11), "wizzard_m_walk_anim_f3");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(16, 10, 16, 11), "wizzard_m_jump_anim_f0");

            // Enemies

            //Small Ogre
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(23, 12, 23, 13), "ogre_small_m_idle_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(24, 12, 24, 13), "ogre_small_m_idle_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(25, 12, 25, 13), "ogre_small_m_idle_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(26, 12, 26, 13), "ogre_small_m_idle_anim_f3");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(27, 12, 27, 13), "ogre_small_m_walk_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(28, 12, 28, 13), "ogre_small_m_walk_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(29, 12, 29, 13), "ogre_small_m_walk_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(30, 12, 30, 13), "ogre_small_m_walk_anim_f3");

            //Small Skeleton
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(23, 4, 23, 5), "skeleton_small_m_idle_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(24, 4, 24, 5), "skeleton_small_m_idle_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(25, 4, 25, 5), "skeleton_small_m_idle_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(26, 4, 26, 5), "skeleton_small_m_idle_anim_f3");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(27, 4, 27, 5), "skeleton_small_m_walk_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(28, 4, 28, 5), "skeleton_small_m_walk_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(29, 4, 29, 5), "skeleton_small_m_walk_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(30, 4, 30, 5), "skeleton_small_m_walk_anim_f3");

            //Green Blob
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(27, 6, 27, 7), "blob_green_m_idle_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(28, 6, 28, 7), "blob_green_m_idle_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(29, 6, 29, 7), "blob_green_m_idle_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(30, 6, 30, 7), "blob_green_m_idle_anim_f3");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(27, 6, 27, 7), "blob_green_m_walk_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(28, 6, 28, 7), "blob_green_m_walk_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(29, 6, 29, 7), "blob_green_m_walk_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(30, 6, 30, 7), "blob_green_m_walk_anim_f3");

            //Dungeon room

            //Ground tiles
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(1, 4, 1, 4), "dungeon_tile_0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(2, 4, 2, 4), "dungeon_tile_1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(3, 4, 3, 4), "dungeon_tile_2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(2, 5, 2, 5), "dungeon_tile_3");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(6, 9, 6, 9), "dungeon_tile_hole");

            //Wall tiles
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

            //Empty tile
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(1, 16, 1, 16), "dungeon_tile_empty");

            // Pillar tiles
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(5, 5, 5, 5), "dungeon_pillar_top");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(5, 6, 5, 6), "dungeon_pillar_bottom");

            //Pickups
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(18, 14, 18, 14), "pickup_pot_orange_big");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(18, 15, 18, 15), "pickup_pot_orange_small");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(19, 14, 19, 14), "pickup_pot_blue_big");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(19, 15, 19, 15), "pickup_pot_blue_small");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(20, 14, 20, 14), "pickup_pot_green_big");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(20, 15, 20, 15), "pickup_pot_green_small");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(21, 14, 21, 14), "pickup_pot_yellow_big");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(21, 15, 21, 15), "pickup_pot_yellow_small");

            //Hearts
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(18, 16, 18, 16), "heart_full");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(19, 16, 19, 16), "heart_half");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(20, 16, 20, 16), "heart empty");

            //Weapons
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(20, 5, 20, 6), "sword");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(18, 11, 18, 12), "spear");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(21, 9, 21, 10), "staff");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(18, 2, 18, 4), "hammer_big");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(19, 9, 19, 10), "golden_sword");

            //Misc
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(18, 20, 18, 20), "skull");

            //Setup inital states

            //Join screen
            Camera.SetViewport(new Viewport(new Vector2(0, 0), new Vector2(100, 100)));
            TextCanvas = new TextCanvas(-10, new Vector2(50, 50), $"Connecting to {NetworkClient.CurrentServer} ...");
            World.AddObject(TextCanvas);
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
                        OnJoin(playerJoinAck);
                        break;
                    }

                    case GameStateUpdate gameStateUpdate:
                    {
                        ApplyGameStateUpdate(gameStateUpdate.Active);
                        break;
                    }

                    case PlayerInputAck playerInputAck:
                    {
                        InputManager.OnPlayerInputAck(playerInputAck);
                        break;
                    }

                    //Player creations
                    case PlayerCreation playerCreation:
                    {
                        //Try to loctate the object using the network id
                        var gameObject = World.GetObjectById(playerCreation.ObjectId);

                        //We seem to already know the object? Dublicate packet arrival or faulty serverside logic. Either way, we keep the local version and wait for more updates to come to refresh it's state
                        if (gameObject != null)
                        {
                            break;
                        }

                        //Create the object based on remote info
                        gameObject = new PlayerBaseClient(
                            playerCreation.ObjectId,
                            playerCreation.Name,
                            playerCreation.Type,
                            new Vector2(playerCreation.PositionX, playerCreation.PositionY),
                            new Vector2(playerCreation.ScaleX, playerCreation.ScaleY),
                            playerCreation.Health);

                        World.AddObject(gameObject);

                        break;
                    }

                    case ObjectPositionUpdate _:
                    case ObjectHealthUpdate _:
                    case PickupStateUpdate _:
                    case DoorStateUpdate _:
                    case AttackStateUpdate _:
                    {
                        var gameObject = World.GetObjectById((inbound as ObjectBoundPacket).ObjectId);

                        if (gameObject != null)
                        {
                            gameObject.GetComponent<NetworkSynchronization>().PacketsInbound.Add(inbound);
                        }

                        break;
                    }
                }
            }

            //Tasks during active gameplay aka connected and past the lobby
            if (State == GameState.Gameplay)
            {
                //Poll controls and send input to server if needed
                if (InputManager.UpdateControls(out var playerInput))
                {
                    NetworkClient.SendPacket(playerInput);
                }

                //Run fixed simulation on all relevant world objects
                foreach (var worldObject in World.GetAllObjects())
                {
                    worldObject.OnFixedUpdate(deltaTime, World);
                }
            }

            MaintainDungeonRooomBuffer();
        }

        protected override void OnUpdate(double deltaTime)
        {
            foreach (var worldObject in World.GetAllObjects())
            {
                worldObject.OnUpdate(deltaTime);
            }

            DebugCollisions();

            UpdateSpectator();
        }

        protected void OnJoin(PlayerJoinAck playerJoinAck)
        {
            if (State != GameState.Connecting) return; //Ignore join packet if we are already connected.

            Console.WriteLine($"Successfully joined {NetworkClient.CurrentServer}");

            WorldSeed = playerJoinAck.WorldSeed;
            LobbySize = playerJoinAck.LobbySize;

            if (!playerJoinAck.GameActive) //Joined the lobby
            {
                TextCanvas.Text = $"Waiting for the game to start ...";
                State = GameState.Lobby;
            }
            else if (State != GameState.Gameplay) //Joined into the active game
            {
                State = GameState.Gameplay;
                ApplyGameStateUpdate(true);
            }

            PlayerGameObjectId = playerJoinAck.PlayerObjectId;

            AddDungeonRoom(LevelGenerator.AddSpawn(Vector2.ZERO));
        }

        protected void ApplyGameStateUpdate(bool gameStarted)
        {
            if (gameStarted && State != GameState.Gameplay)
            {
                State = GameState.Gameplay;
                OnGameStart();
            }
        }

        protected void OnGameStart()
        {
            TextCanvas.Show = false;
        }

        protected void UpdateSpectator()
        {
            if (State != GameState.Gameplay) return;

            var cameraCenterPos = Vector2.ZERO;
            var viewportPadding = 16 * 6;

            //Try find the player object if we did not already have it
            if (PlayerObject == null)
            {
                PlayerObject = World.GetObjectById(PlayerGameObjectId) as PlayerBaseClient;
            }

            if (PlayerObject != null)
            {
                var transformation = PlayerObject.GetComponent<Transformation2D>();

                cameraCenterPos.X += transformation.Position.X + transformation.Scale.X / 2;
                cameraCenterPos.Y += transformation.Position.Y + transformation.Scale.Y / 2;
            }

            Camera.SetViewport(new Viewport(
                new Vector2(cameraCenterPos.X - viewportPadding, cameraCenterPos.Y - viewportPadding),
                new Vector2(cameraCenterPos.X + viewportPadding, cameraCenterPos.Y + viewportPadding)));
        }

        protected void DebugCollisions()
        {
            foreach (var worldObject in World.GetAllObjects())
            {
                if (worldObject.GetComponent<Collider>() is Collider collider)
                {
                    if (collider.Active) collider.AsBoundingBoxes().ForEach(dbgBox => DebugOverlay.DrawBox(dbgBox.X, dbgBox.Y, dbgBox.Width, dbgBox.Height, new Vector4(0, 1, 0, 0.5)));
                }
            }

            if (PlayerObject == null) return;

            var playerCollider = PlayerObject.GetComponent<Collider>();
            var playerTransform = PlayerObject.GetComponent<Transformation2D>();

            var objectBounds = playerCollider.AsBoundingBoxes().First();

            var desiredDelta = new Vector2(0, -16);
            var possibleDelta = new Vector2(desiredDelta.X, desiredDelta.Y);

            var ray = new Ray(objectBounds.X + objectBounds.Width / 2, objectBounds.Y + objectBounds.Height / 2, desiredDelta.X, desiredDelta.Y);

            //DebugOverlay.DebugLines.Add((ray.Origin, new Vector2(ray.Origin.X + ray.Direction.X, ray.Origin.Y + ray.Direction.Y), new Vector4(1, 1, 0, 1)));

            //foreach (var worldObject in World.GetObjectsInArea(PlayerObject.GetComponent<Transformation2D>().GetBoundingBox()))
            foreach (var worldObject in World.GetAllObjects())
            {
                if (worldObject == PlayerObject) continue;

                var objectCollider = worldObject.GetComponent<Collider>();

                if (objectCollider != null)
                {
                    if (playerCollider.CollidesWith(objectCollider, out var collisionResult))
                    {
                        foreach (var intersection in collisionResult.Intersections)
                        {
                            DebugOverlay.DrawBox(intersection.X, intersection.Y, intersection.Width, intersection.Height, new Vector4(1, 0, 0, 1));
                        }
                    }

                    //foreach (var rectangle in objectCollider.AsBoundingBoxes())
                    //{
                    //    //if (rectangle.IntersectsWith(ray, out var contactpoint, out var contactNormal, out var contactRayTime))
                    //    //{
                    //    //    DebugOverlay.DrawBox(contactpoint.X - 0.5, contactpoint.Y - 0.5, 1, 1, new Vector4(1, 0, 0, 1));

                    //    //    //DebugOverlay.DebugLines.Add((new Vector2(contactpoint.X, contactpoint.Y), new Vector2(contactpoint.X + contactNormal.X * 16, contactpoint.Y + contactNormal.Y * 16), new Vector4(1, 0, 1, 1)));

                    //    //    possibleDelta.X += contactNormal.X * System.Math.Abs(possibleDelta.X) * (1 - contactRayTime);
                    //    //    possibleDelta.Y += contactNormal.Y * System.Math.Abs(possibleDelta.Y) * (1 - contactRayTime);
                    //    //}

                    //    //Enlarge boxes by half the dimensions of the object that wants to move to catch tunneling and find the correct position to slide along walls
                    //    rectangle.X -= objectBounds.Width / 2;
                    //    rectangle.Width += objectBounds.Width;

                    //    rectangle.Y -= objectBounds.Height / 2;
                    //    rectangle.Height += objectBounds.Height;

                    //    var collisionRay = new Ray(objectBounds.X + objectBounds.Width / 2, objectBounds.Y + objectBounds.Height / 2, possibleDelta.X, possibleDelta.Y);

                    //    if (rectangle.IntersectsWith(collisionRay, out var contactPoint, out var contactNormal, out var contactRayTime))
                    //    {
                    //        DebugOverlay.DrawBox(contactPoint.X - 0.5, contactPoint.Y - 0.5, 1, 1, new Vector4(1, 0, 0, 1));

                    //        possibleDelta.X += contactNormal.X * System.Math.Abs(possibleDelta.X) * (1 - contactRayTime);
                    //        possibleDelta.Y += contactNormal.Y * System.Math.Abs(possibleDelta.Y) * (1 - contactRayTime);
                    //    }

                    //}
                }
            }

            //DebugOverlay.DebugLines.Add((
            //    new Vector2(objectBounds.X + objectBounds.Width / 2, objectBounds.Y + objectBounds.Height / 2),
            //    new Vector2(objectBounds.X + objectBounds.Width / 2 + possibleDelta.X, objectBounds.Y + objectBounds.Height / 2 + possibleDelta.Y),
            //    new Vector4(1, 0, 0, 1)));

            //DebugOverlay.DebugLines.Add((new Vector2(contactpoint.X, contactpoint.Y), new Vector2(contactpoint.X + contactNormal.X * 16, contactpoint.Y + contactNormal.Y * 16), new Vector4(1, 0, 1, 1)));
        }

        protected void AddDungeonRoom(DungeonRoom dungeonRoom)
        {
            //System.Console.WriteLine($"Added room no. {dungeonRoom.RoomNumber} at <{dungeonRoom.Position.X};{dungeonRoom.Position.Y}>");

            for (int nObject = 0; nObject < dungeonRoom.Objects.Count; nObject++)
            {
                var obj = dungeonRoom.Objects[nObject];

                if (obj is DungeonDoor dungeonDoor)
                {
                    dungeonRoom.Objects[nObject] = new DungeonDoorRenderable(dungeonDoor.ObjectId, dungeonDoor.Type, dungeonDoor.GetComponent<Transformation2D>().Position, dungeonDoor.Open);
                }
                else if (obj is DungeonTile dungeonTile)
                {
                    DungeonTileRenderable.AddRenderable(dungeonTile);
                }
                else if (obj is Pickup pickup)
                {
                    dungeonRoom.Objects[nObject] = new PickupRenderable(pickup.ObjectId, pickup.Type, pickup.GetComponent<Transformation2D>().Position);
                }
                else if (obj is LivingObject livingObject)
                {
                    var transform = livingObject.GetComponent<Transformation2D>();
                    var collider = livingObject.GetComponent<Collider>().Rects;
                    var health = livingObject.GetComponent<Health>();

                    //Replace object with upgraded instance
                    dungeonRoom.Objects[nObject] = new LivingObjectClient(livingObject.ObjectId, livingObject.Type, transform.Position, transform.Scale, collider.FirstOrDefault(), health.CurrentHealth);

                    continue;
                }
            }

            //Add room label
            if (dungeonRoom.RoomNumber > 0)
            {
                dungeonRoom.Objects.Add(new TextCanvas(dungeonRoom.RoomNumber * 10000, new Vector2(dungeonRoom.Position.X - 48, dungeonRoom.Position.Y - 86), $"Room {dungeonRoom.RoomNumber}"));
            }

            //Add objects to world and remeber the room meta data
            dungeonRoom.Objects.ForEach(x => World.AddObject(x));
            DungeonRooms.Add(dungeonRoom);

            //Request network updates for any created room, unless its the spawnroom
            if (dungeonRoom.RoomNumber > 0)
            {
                NetworkClient.SendPacket(new DungeonRoomSyncRequest { RequiresAck = true, RoomNumber = dungeonRoom.RoomNumber });
            }
        }

        protected void MaintainDungeonRooomBuffer()
        {
            if (PlayerObject == null) return;

            var transformation = PlayerObject.GetComponent<Transformation2D>();
            var playerY = transformation.Position.Y + transformation.Scale.Y;

            //Find roomnumber
            DungeonRoom currentRoom = default;

            foreach (var room in DungeonRooms)
            {
                if (playerY < room.Position.Y && playerY >= room.Position.Y - room.Size.Y)
                {
                    currentRoom = room;
                    break;
                }
            }

            const int ROOM_BUFFER_COUNT = 1;

            if (currentRoom != default)
            {
                //System.Console.WriteLine(currentRoom.RoomNumber);

                /*
                    * Remove rooms that are out of range 
                    * 
                    * Range is buffer count + 1, so we avoid rapid removal and readding when player is on threshold of one room to the next one.
                    * We rather keep one more room in memory and free later, than constant recration requests to server
                    */
                var outOfRangeRooms = DungeonRooms.Where(x => x.RoomNumber < currentRoom.RoomNumber - (ROOM_BUFFER_COUNT + 1) || x.RoomNumber > currentRoom.RoomNumber + (ROOM_BUFFER_COUNT + 1)).ToList();

                foreach (var room in outOfRangeRooms)
                {
                    room.Objects.ForEach(x => World.RemoveObject(x));
                }

                DungeonRooms.RemoveAll(x => outOfRangeRooms.Contains(x));

                //Re-addd rooms above if needed
                for (int nAbove = 1; nAbove <= ROOM_BUFFER_COUNT; nAbove++)
                {
                    if (!DungeonRooms.Any(x => x.RoomNumber == currentRoom.RoomNumber + nAbove))
                    {
                        AddDungeonRoom(LevelGenerator.AppendRoom(DungeonRooms.First(x => x.RoomNumber == currentRoom.RoomNumber + nAbove - 1), LobbySize, WorldSeed));
                    }
                }

                //Re-addd rooms below if needed
                if (currentRoom.RoomNumber > 0)
                {
                    for (int nBelow = 1; nBelow <= ROOM_BUFFER_COUNT; nBelow++)
                    {
                        if (!DungeonRooms.Any(x => x.RoomNumber == currentRoom.RoomNumber - nBelow))
                        {
                            AddDungeonRoom(LevelGenerator.PrependRoom(DungeonRooms.First(x => x.RoomNumber == currentRoom.RoomNumber - nBelow + 1), LobbySize, WorldSeed));
                        }
                    }
                }
            }
        }
    }
}
