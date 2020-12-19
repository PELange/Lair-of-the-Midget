using LOTM.Server.Game.Network;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using System.Collections.Generic;

namespace LOTM.Server.Game
{
    public class LotmServer : Shared.Engine.Game
    {
        protected LotmNetworkManagerServer NetworkServer { get; set; }

        public int NextFreeEntityId { get; set; }

        protected Dictionary<string, PlayerObject> Players { get; set; }

        public LotmServer(string listenAddress)
            : base(new LotmNetworkManagerServer(listenAddress))
        {
            NetworkServer = (LotmNetworkManagerServer)NetworkManager;
            Players = new Dictionary<string, PlayerObject>();
        }

        protected override void OnInit()
        {
            //World.Seed = new System.Random().Next(0, 100000);
            World.Seed = 130;
        }

        protected override void OnBeforeUpdate()
        {
            if (NetworkManager.TryGetPacket(out var packet))
            {
                switch (packet)
                {
                    //A player wants to join / and or has no recived our join ack yet
                    case PlayerJoin playerJoin:
                    {
                        NetworkServer.OnPlayerJoin(playerJoin, CreatePlayerJoinAck);
                        break;
                    }

                    //A player sends input for the character he controls
                    case PlayerInput playerInput:
                    {
                        if (Players.TryGetValue(playerInput.Sender.ToString(), out var playerObject))
                        {
                            //Console.WriteLine($"{DateTime.Now} Recieved input {playerInput.InputType}");
                            playerObject.PacketsInbound.Enqueue(playerInput);
                        }

                        break;
                    }
                }
            }
        }

        protected override void OnFixedUpdate(double deltaTime)
        {
        }

        protected override void OnUpdate(double deltaTime)
        {
        }

        protected override void OnAfterUpdate()
        {
            //After all the updates, collect outbond packets from all gameobjects
            foreach (var gameObject in World.Objects)
            {
                while (gameObject.PacketsOutbound.TryDequeue(out var packet))
                {
                    NetworkServer.Broadcast(packet);
                }
            }
        }

        protected override void OnShutdown()
        {
        }

        public PlayerJoinAck CreatePlayerJoinAck(PlayerJoin playerJoin)
        {
            System.Console.WriteLine($"{playerJoin.PlayerName}({playerJoin.Sender}) joined the server.");

            var playerObject = new PlayerObject(new Vector2(6 * 16, 6 * 16), scale: new Vector2(16, 16 * 2))
            {
                Id = NextFreeEntityId++
            };

            Players[playerJoin.Sender.ToString()] = playerObject;

            World.Objects.Add(playerObject);

            return new PlayerJoinAck
            {
                PlayerGameObjectId = playerObject.Id,
                WorldSeed = World.Seed
            };
        }
    }
}
