using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using LOTM.Shared.Game.Network.Packets;
using System;

namespace LOTM.Shared.Game.Objects
{
    public class PlayerObject : DynamicHealthObject
    {
        public PlayerObject(Vector2 position = null, double rotation = 0, Vector2 scale = null, double health = default)
            : base(position, rotation, scale, health)
        {
        }

        public override void OnFixedUpdate(double deltaTime)
        {
            while (PacketsInbound.TryDequeue(out var packet))
            {
                switch (packet)
                {
                    //Recieve sync from server. Apply on client
                    case DynamicHealthObjectSync dynamicHealthObjectSync:
                    {
                        Console.WriteLine($"{DateTime.Now} Recieved sync: <{dynamicHealthObjectSync.PositionX}, {dynamicHealthObjectSync.PositionY}>");
                        ApplyNetworkPacket(dynamicHealthObjectSync);
                        break;
                    }

                    //Receive input from client. Apply on server.
                    case PlayerInput playerInput:
                    {
                        var walkSpeed = 100;

                        Console.WriteLine($"{DateTime.Now} processed input {playerInput.InputType}");

                        if (GetComponent<Transformation2D>() is Transformation2D transformation)
                        {
                            switch (playerInput.InputType)
                            {
                                case Engine.Controls.InputType.WALK_UP:
                                    transformation.Position.Y -= walkSpeed * deltaTime;
                                    break;

                                case Engine.Controls.InputType.WALK_DOWN:
                                    transformation.Position.Y += walkSpeed * deltaTime;
                                    break;

                                case Engine.Controls.InputType.WALK_LEFT:
                                    transformation.Position.X -= walkSpeed * deltaTime;
                                    break;

                                case Engine.Controls.InputType.WALK_RIGHT:
                                    transformation.Position.X += walkSpeed * deltaTime;
                                    break;
                            }
                        }

                        break;
                    }
                }
            }
        }

        public override void OnUpdate(double deltaTime)
        {
            //... stuff here

            //Sync the player if we made any changes
            PacketsOutbound.Enqueue(WriteToNetworkPacket(new DynamicHealthObjectSync()));
        }
    }
}
