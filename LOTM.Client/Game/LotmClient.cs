using LOTM.Client.Engine;
using LOTM.Client.Engine.Controls;
using LOTM.Client.Engine.Graphics;
using LOTM.Client.Game.Network;
using LOTM.Client.Game.Objects.Environment;
using LOTM.Client.Game.Objects.Player;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Game.Logic;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects.Environment;
using System;
using System.Linq;
using static LOTM.Client.Engine.Graphics.OrthographicCamera;

namespace LOTM.Client.Game
{
    public class LotmClient : GuiGame
    {
        protected LotmNetworkManagerClient NetworkClient { get; set; }

        protected int PlayerGameObjectId { get; set; }
        protected GameObject PlayerObject { get; set; }


        protected enum GameState
        {
            Connecting,
            Lobby,
            Gameplay
        }

        protected GameState State { get; set; }
        public DateTime LastStateSyncAttempt { get; set; }

        public LotmClient(int windowWidth, int windowHeight, string connectionString, string playerName)
            : base(windowWidth, windowHeight, "Lair of the Midget", "Game/Assets/Textures/icon.png", new LotmNetworkManagerClient(connectionString, playerName))
        {
            NetworkClient = (LotmNetworkManagerClient)NetworkManager;
            PlayerGameObjectId = -1;
            State = GameState.Connecting;
        }

        protected override void OnInit()
        {
            //Register main texture atlas
            AssetManager.RegisterTexture("Game/Assets/Textures/0x72_DungeonTilesetII_v1.3.png", "dungeonTiles");

            //Register indivual sprites on the atlas using 16x16 grid indices


            //Players

            //Elf
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(8, 0, 8, 1), "elf_m_idle_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(9, 0, 9, 1), "elf_m_idle_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(10, 0, 10, 1), "elf_m_idle_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(11, 0, 11, 1), "elf_m_idle_anim_f3");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(12, 0, 12, 1), "elf_m_walk_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(13, 0, 13, 1), "elf_m_walk_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(14, 0, 14, 1), "elf_m_walk_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(15, 0, 15, 1), "elf_m_walk_anim_f3");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(16, 0, 16, 1), "elf_m_jump_anim_f0");

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

            //Deamon boss
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(1, 23, 2, 24), "demonboss_m_idle_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(3, 23, 4, 24), "demonboss_m_idle_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(5, 23, 6, 24), "demonboss_m_idle_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(7, 23, 8, 24), "demonboss_m_idle_anim_f3");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(9, 23, 10, 24), "demonboss_m_walk_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(11, 23, 12, 24), "demonboss_m_walk_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(13, 23, 14, 24), "demonboss_m_walk_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(15, 23, 16, 24), "demonboss_m_walk_anim_f3");

            //Small Skeleton
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(23, 4, 23, 5), "skeleton_small_m_idle_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(24, 4, 24, 5), "skeleton_small_m_idle_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(25, 4, 25, 5), "skeleton_small_m_idle_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(26, 4, 26, 5), "skeleton_small_m_idle_anim_f3");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(27, 4, 27, 5), "skeleton_small_m_walk_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(28, 4, 28, 5), "skeleton_small_m_walk_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(29, 4, 29, 5), "skeleton_small_m_walk_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(30, 4, 30, 5), "skeleton_small_m_walk_anim_f3");

            //Small Ogre
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(23, 12, 23, 13), "ogre_small_m_idle_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(24, 12, 24, 13), "ogre_small_m_idle_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(25, 12, 25, 13), "ogre_small_m_idle_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(26, 12, 26, 13), "ogre_small_m_idle_anim_f3");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(27, 12, 27, 13), "ogre_small_m_walk_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(28, 12, 28, 13), "ogre_small_m_walk_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(29, 12, 29, 13), "ogre_small_m_walk_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(30, 12, 30, 13), "ogre_small_m_walk_anim_f3");

            //Green Blob
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(27, 6, 27, 7), "blob_green_m_idle_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(28, 6, 28, 7), "blob_green_m_idle_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(29, 6, 29, 7), "blob_green_m_idle_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(30, 6, 30, 7), "blob_green_m_idle_anim_f3");

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

                    case GameStateUpdate gameStateUpdate:
                    {
                        if (gameStateUpdate.Active && State == GameState.Lobby)
                        {
                            State = GameState.Gameplay;
                        }

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
                        var gameObject = World.GetGameObjectByNetworkId(playerCreation.ObjectId);

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

                    //Enemy creations
                    case LivingObjectCreation livingObjectCreation:
                    {
                        //Try to loctate the object using the network id
                        var gameObject = World.GetGameObjectByNetworkId(livingObjectCreation.ObjectId);

                        //We seem to already know the object? Dublicate packet arrival or faulty serverside logic. Either way, we keep the local version and wait for more updates to come to refresh it's state
                        if (gameObject != null)
                        {
                            break;
                        }

                        //switch enemy types here

                        ////Create the object based on remote info
                        //gameObject = new LivingObjectClient(
                        //    livingObjectCreation.ObjectId,
                        //    livingObjectCreation.Type,
                        //    new Vector2(livingObjectCreation.PositionX, livingObjectCreation.PositionY),
                        //    new Vector2(livingObjectCreation.ScaleX, livingObjectCreation.ScaleY),
                        //    livingObjectCreation.Health);

                        //World.AddObject(gameObject);

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

                    case ObjectHealthUpdate objectHealthUpdate:
                    {
                        var gameObject = World.GetGameObjectByNetworkId(objectHealthUpdate.ObjectId);

                        if (gameObject != null)
                        {
                            gameObject.GetComponent<NetworkSynchronization>().PacketsInbound.Add(objectHealthUpdate);
                        }

                        break;
                    }
                }
            }

            //Make sure we are "connected" to the server -> If we did not get an ack from server yet, resend our join request.
            NetworkClient.EnsureServerConnection();

            //If we are waiting in the lobby, ask the server if the game has started in case we missed the inital start packet
            if (State == GameState.Lobby && (LastStateSyncAttempt == null || (DateTime.Now - LastStateSyncAttempt).TotalMilliseconds > 1))
            {
                LastStateSyncAttempt = DateTime.Now;

                NetworkClient.SendPacket(new GameStateRequest());
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
        }

        protected override void OnUpdate(double deltaTime)
        {
            foreach (var worldObject in World.GetAllObjects())
            {
                worldObject.OnUpdate(deltaTime);
            }

            DebugCollisions();

            UpdateCamera();
        }

        protected override void Render()
        {
            switch (State)
            {
                case GameState.Connecting:
                {
                    Console.WriteLine($"Connecting to {NetworkClient.CurrentServer} ...");
                    break;
                }

                case GameState.Lobby:
                {
                    Console.WriteLine($"Waiting for game to start ...");
                    break;
                }

                case GameState.Gameplay:
                {
                    base.Render();
                    break;
                }
            }
        }

        void OnJoin(int seed, int playerGameObjectId)
        {
            Console.WriteLine($"Successfully joined {NetworkClient.CurrentServer}");

            //Joined the lobby
            State = GameState.Lobby;

            PlayerGameObjectId = playerGameObjectId;

            //During lobby idle, pregenerate the world while waiting
            foreach (var obj in LevelGenerator.PreGenerate(4, seed))
            {
                if (obj is DungeonTile dungeonTile)
                {
                    var transform = dungeonTile.GetComponent<Transformation2D>();
                    World.AddObject(new DungeonTileRenderable(dungeonTile.Type, transform.Position, transform.Scale));
                }
                else
                {
                    //todo add other types
                }
            }
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

        void DebugCollisions()
        {
            foreach (var worldObject in World.GetAllObjects())
            {
                if (worldObject.GetComponent<Collider>() is Collider collider)
                {
                    collider.AsBoundingBoxes().ForEach(dbgBox => DebugOverlay.DrawBox(dbgBox.X, dbgBox.Y, dbgBox.Width, dbgBox.Height, new Vector4(0, 1, 0, 0.5)));
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
    }
}
