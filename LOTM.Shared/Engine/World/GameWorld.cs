using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using LOTM.Shared.Engine.Objects.Components;
using System.Collections.Generic;
using System.Linq;

namespace LOTM.Shared.Engine.World
{
    public class GameWorld
    {
        protected List<GameObject> DynamicObjects { get; }
        protected Dictionary<int, GameObject> DynamicObjectLookupCache { get; }

        protected QuadTree StaticObjects { get; }
        protected Dictionary<int, GameObject> StaticObjectLookupCache { get; }

        public GameWorld()
        {
            DynamicObjects = new List<GameObject>();
            DynamicObjectLookupCache = new Dictionary<int, GameObject>();

            StaticObjects = new QuadTree(new Rectangle(-10_000, -10_000, 20_000, 20_000)); //Todo adjust quadtree to dungeon dimensions -> use world constructor parameter for this?
            StaticObjectLookupCache = new Dictionary<int, GameObject>();
        }

        public void AddObject(GameObject gameObject)
        {
            if (gameObject is IMoveable)
            {
                DynamicObjects.Add(gameObject);
                DynamicObjectLookupCache.Add(gameObject.ObjectId, gameObject);
            }
            else
            {
                StaticObjects.Add(gameObject);
                StaticObjectLookupCache.Add(gameObject.ObjectId, gameObject);
            }
        }

        public void RemoveObject(GameObject gameObject)
        {
            if (gameObject is IMoveable)
            {
                DynamicObjects.Remove(gameObject);
                DynamicObjectLookupCache.Remove(gameObject.ObjectId);
            }
            else
            {
                StaticObjects.Remove(gameObject);
                StaticObjectLookupCache.Remove(gameObject.ObjectId);
            }
        }

        public IEnumerable<GameObject> GetAllObjects()
        {
            return StaticObjects.GetAllObjects().Concat(DynamicObjects);
        }

        public IEnumerable<GameObject> GetObjectsInArea(Rectangle area)
        {
            return StaticObjects.GetObjects(area).Concat(DynamicObjects.Where(x => area.IntersectsWith(x.GetComponent<Transformation2D>().GetBoundingBox())));
        }

        public IEnumerable<GameObject> GetDynamicObjectsInArea(Rectangle area)
        {
            return DynamicObjects.Where(x => area.IntersectsWith(x.GetComponent<Transformation2D>().GetBoundingBox()));
        }

        public GameObject GetObjectById(int objectId)
        {
            if (DynamicObjectLookupCache.TryGetValue(objectId, out var dynamicObject)) return dynamicObject;

            if (StaticObjectLookupCache.TryGetValue(objectId, out var staticObject)) return staticObject;

            return null;
        }
    }
}
