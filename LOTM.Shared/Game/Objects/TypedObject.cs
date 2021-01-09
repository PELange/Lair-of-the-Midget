using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;

namespace LOTM.Shared.Game.Objects
{
    public class TypedObject : GameObject
    {
        public ObjectType Type { get; }

        public TypedObject(ObjectType type, Vector2 position = default, double rotation = 0, Vector2 scale = default)
             : base(position, rotation, scale)
        {
            Type = type;
        }
    }
}
