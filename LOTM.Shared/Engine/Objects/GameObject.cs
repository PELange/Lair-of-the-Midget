using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Engine.World;
using System.Collections.Generic;
using System.Linq;

namespace LOTM.Shared.Engine.Objects
{
    public class GameObject
    {
        public int ObjectId { get; }

        protected ICollection<IComponent> Components { get; }

        public GameObject(int id, Vector2 position = null, double rotation = 0, Vector2 scale = null)
        {
            ObjectId = id;

            Components = new LinkedList<IComponent>();

            AddComponent(new Transformation2D
            {
                Position = position ?? Vector2.ZERO,
                Rotation = rotation,
                Scale = scale ?? new Vector2(1, 1)
            });

            OnInit();
        }

        public void AddComponent(IComponent component)
        {
            Components.Add(component);
        }

        public T GetComponent<T>() where T : IComponent
        {
            return (T)Components.Where(x => x is T).FirstOrDefault();
        }

        public virtual void OnInit()
        {
        }

        public virtual void OnBeforeUpdate()
        {
        }

        public virtual void OnFixedUpdate(double deltaTime, GameWorld world)
        {
        }

        public virtual void OnUpdate(double deltaTime)
        {
        }

        public virtual void OnAfterUpdate()
        {
        }
    }
}
