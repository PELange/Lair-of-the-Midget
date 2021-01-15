using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;

namespace LOTM.Shared.Game.Objects
{
    public class TypedObject : GameObject
    {
        public ObjectType Type { get; }

        public TypedObject(int id, ObjectType type, Vector2 position = default, double rotation = 0, Vector2 scale = default)
             : base(id, position, rotation, scale)
        {
            Type = type;
        }
    }
}
