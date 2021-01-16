using LOTM.Server.Game.Objects.Living;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Engine.World;
using LOTM.Shared.Game.Logic;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Components;
using LOTM.Shared.Game.Objects.Environment;
using System.Linq;

namespace LOTM.Server.Game.Objects.Environment
{
    class DungeonDoorServer : DungeonDoor
    {
        protected DungeonRoom DungeonRoom { get; }

        public DungeonDoorServer(int id, ObjectType type, Vector2 position, bool open, DungeonRoom room)
            : base(id, type, position, open)
        {
            DungeonRoom = room;
        }

        public override void OnFixedUpdate(double deltaTime, GameWorld world)
        {
            base.OnFixedUpdate(deltaTime, world);

            if (Open) return;

            //Unlock it if no enemy with hp left is found -> aka all dead
            if (DungeonRoom.Objects.Count(x => x is EnemyBaseServer enemyBaseServer && enemyBaseServer.GetComponent<Health>().CurrentHealth > 0) == 0)
            {
                Open = true;
                GetComponent<Collider>().Active = false;
                GetComponent<NetworkSynchronization>().PacketsOutbound.Enqueue(new DoorStateUpdate { ObjectId = ObjectId, Open = Open });
            }
        }
    }
}
