﻿using LOTM.Server.Game.Objects.Living;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Engine.World;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Components;
using LOTM.Shared.Game.Objects.Interactable;
using System.Collections.Generic;
using System.Linq;

namespace LOTM.Server.Game.Objects.Interactable
{
    class PickupServer : Pickup
    {
        public PickupServer(int id, ObjectType type, Vector2 position)
            : base(id, type, position)
        {
        }

        public override void OnFixedUpdate(double deltaTime, GameWorld world)
        {
            base.OnFixedUpdate(deltaTime, world);

            if (!Active) return;

            var collidingObjets = new List<(GameObject, double)>();

            foreach (var colliderBox in GetComponent<Collider>().AsBoundingBoxes())
            {
                var objectBoundsCenter = new Vector2(colliderBox.X + colliderBox.Width / 2, colliderBox.Y + colliderBox.Height / 2);

                foreach (var worldObject in world.GetDynamicObjectsInArea(colliderBox))
                {
                    if (!(worldObject is PlayerBaseServer)) continue; //Pickup collisions only allowed with player objects

                    var objectCollider = worldObject.GetComponent<Collider>();

                    if (objectCollider != null)
                    {
                        //Foreach found collider box
                        foreach (var rect in objectCollider.AsBoundingBoxes())
                        {
                            if (rect.IntersectsWith(colliderBox))
                            {
                                var worldObjectRectCenter = new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

                                collidingObjets.Add((worldObject, DistanceMetrics.EuclideanSquared(objectBoundsCenter, worldObjectRectCenter)));
                            }
                        }
                    }
                }
            }

            if (collidingObjets.OrderBy(x => x.Item2).Select(x => x.Item1).FirstOrDefault() is PlayerBaseServer playerBaseServer)
            {
                var healthAmount = Type switch
                {
                    ObjectType.Pickup_Health_Minor => 25,
                    ObjectType.Pickup_Health_Major => 50,
                    _ => 0
                };

                playerBaseServer.GetComponent<Health>().AddHealthAbsolute(healthAmount);

                //Deactive pickup
                Active = false;
            }
        }
    }
}