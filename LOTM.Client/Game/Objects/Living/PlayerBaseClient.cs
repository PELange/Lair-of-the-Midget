using LOTM.Shared.Engine.Math;
using LOTM.Shared.Game.Objects;
using LOTM.Shared.Game.Objects.Components;

namespace LOTM.Client.Game.Objects.Player
{
    public class PlayerBaseClient : LivingObjectClient
    {
        public PlayerBaseClient(int networkId, string name, ObjectType type, Vector2 position, Vector2 scale, double health)
            : base(networkId, type, position, scale, new Rectangle(0, 0.75, 1, 0.25), health)
        {
            AddComponent(new PlayerInfo(name));
        }
    }
}
