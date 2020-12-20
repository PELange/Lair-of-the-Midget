using LOTM.Shared.Engine.Controls;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Game.Network.Packets;
using System;

namespace LOTM.Shared.Game.Objects
{
    public class PlayerObject : DynamicHealthObject
    {
        public PlayerObject(Vector2 position = null, double rotation = 0, Vector2 scale = null, NetworkInstanceType instanceType = default, double health = default)
            : base(position, rotation, scale, instanceType, health)
        {
        }

        public override void OnFixedUpdate(double deltaTime)
        {
            //Process inbound packets
            while (PacketsInbound.TryDequeue(out var inbound))
            {
                switch (inbound)
                {
                    //Recieve sync from server. Apply on client
                    case DynamicHealthObjectSync dynamicHealthObjectSync:
                    {
                        //Console.WriteLine($"{DateTime.Now} Recieved sync for {dynamicHealthObjectSync.NetworkId}: <{dynamicHealthObjectSync.PositionX}, {dynamicHealthObjectSync.PositionY}>");
                        ApplyNetworkPacket(dynamicHealthObjectSync);
                        break;
                    }

                    //Receive input from client. Apply on server.
                    case PlayerInput playerInput:
                    {
                        var walkSpeed = 100;

                        //Console.WriteLine($"{DateTime.Now} processed input {playerInput.Inputs}");

                        if (GetComponent<Transformation2D>() is Transformation2D transformation)
                        {
                            var walkDirection = Vector2.ZERO;

                            if ((playerInput.Inputs & InputType.WALK_UP) != 0)
                            {
                                walkDirection.Y -= 1;
                            }
                            else if ((playerInput.Inputs & InputType.WALK_DOWN) != 0)
                            {
                                walkDirection.Y += 1;
                            }

                            if ((playerInput.Inputs & InputType.WALK_LEFT) != 0)
                            {
                                walkDirection.X -= 1;
                            }
                            else if ((playerInput.Inputs & InputType.WALK_RIGHT) != 0)
                            {
                                walkDirection.X += 1;
                            }

                            if (walkDirection.X != 0 || walkDirection.Y != 0)
                            {
                                //Normalize direction vector
                                var magnitude = Math.Sqrt(walkDirection.X * walkDirection.X + walkDirection.Y * walkDirection.Y);

                                walkDirection.X /= magnitude;
                                walkDirection.Y /= magnitude;

                                transformation.Position.X += walkDirection.X * walkSpeed * deltaTime;
                                transformation.Position.Y += walkDirection.Y * walkSpeed * deltaTime;

                                NetworkSyncFlag = true;
                            }
                        }

                        break;
                    }
                }
            }

            if (InstanceType == NetworkInstanceType.Server && NetworkSyncFlag)
            {
                PacketsOutbound.Enqueue(WriteToNetworkPacket(new DynamicHealthObjectSync()));
                NetworkSyncFlag = false;
            }
        }
    }
}
