using LOTM.Shared.Engine.Math;
using System.Collections.Generic;
using System.Linq;

namespace LOTM.Shared.Engine.Object
{
    public class GameObject
    {
        public ICollection<IComponent> Components { get; } = new LinkedList<IComponent>();

        public GameObject(Vector2 position = null, double rotation = 0, Vector2 scale = null)
        {
            var transform = new Transformation2D
            {
                Position = position ?? Vector2.ZERO,
                Rotation = rotation,
                Scale = scale ?? new Vector2(1, 1)
            };

            Components.Add(transform);
        }

        public T GetComonent<T>() where T : IComponent
        {
            return (T)Components.Where(x => x is T).FirstOrDefault();
        }
    }
}
