using LOTM.Client.Engine;
using LOTM.Client.Engine.Objects.Components;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Engine.World;
using LOTM.Shared.Game.Network.Packets;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Environment;
using System.Collections.Generic;
using System.Linq;

namespace LOTM.Client.Game.Objects.Environment
{
    class DungeonDoorRenderable : DungeonDoor
    {
        public int LastStatePacketId { get; set; }

        public DungeonDoorRenderable(int id, ObjectType type, Vector2 position, bool open)
            : base(id, type, position, open)
        {
            AddComponent(new SpriteRenderer(new List<SpriteRenderer.Segment>
            {
                //Closed door
                new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_closed"), active: !open),

                //Open door
                new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_opened_top"), new Vector2(1, 0.5), new Vector2(0, 0.0), layer: 2000, active: open),
                new SpriteRenderer.Segment(AssetManager.GetSprite("dungeon_door_opened_bottom"), new Vector2(1, 0.5), new Vector2(0, 0.5), layer: 100, active: open)
            }));
        }

        public override void OnFixedUpdate(double deltaTime, GameWorld world)
        {
            //Process inbound packets
            var networkSynchronization = GetComponent<NetworkSynchronization>();

            //1. Check for state changes and only apply the latest one
            if (networkSynchronization.PacketsInbound.Where(x => x is DoorStateUpdate).OrderByDescending(x => x.Id).FirstOrDefault() is DoorStateUpdate doorStateUpdate)
            {
                //Only accept the door state update, if the packet id is larger than the last known update about it. This avoids retransmission issues.
                if (doorStateUpdate.Id > LastStatePacketId)
                {
                    Open = doorStateUpdate.Open;

                    GetComponent<Collider>().Active = !Open;
                    GetComponent<SpriteRenderer>().Segments[0].Active = !Open;
                    GetComponent<SpriteRenderer>().Segments[1].Active = Open;
                    GetComponent<SpriteRenderer>().Segments[2].Active = Open;
                }
            }

            networkSynchronization.PacketsInbound.Clear();
        }
    }
}
