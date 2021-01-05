﻿using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Components;
using System.Linq;

namespace LOTM.Client.Game.Objects
{
    public class MovingHealthObjectClient : MovingHealthObject
    {
        public MovingHealthObjectClient(int networkId, MovingHealthObjectType type, Vector2 position, Vector2 scale, double health)
            : base(type, position, scale, health)
        {
            GetComponent<NetworkSynchronization>().NetworkId = networkId;
        }

        public override void OnFixedUpdate(double deltaTime)
        {
            var networkSynchronization = GetComponent<NetworkSynchronization>();

            //Process inbound packets

            //1. Check for position changes and only apply the latest one
            if (networkSynchronization.PacketsInbound.Where(x => x is ObjectPositionUpdate).OrderByDescending(x => x.Id).FirstOrDefault() is ObjectPositionUpdate objectPositionUpdate)
            {
                var transform = GetComponent<Transformation2D>();

                transform.Position.X = objectPositionUpdate.PositionX;
                transform.Position.Y = objectPositionUpdate.PositionY;
            }

            //2. Check for health updates and only apply the lastest one
            if (networkSynchronization.PacketsInbound.Where(x => x is ObjectHealthUpdate).OrderByDescending(x => x.Id).FirstOrDefault() is ObjectHealthUpdate objectHealthUpdate)
            {
                var health = GetComponent<Health>();

                health.Value = objectHealthUpdate.Health;
            }

            networkSynchronization.PacketsInbound.Clear();
        }
    }
}
