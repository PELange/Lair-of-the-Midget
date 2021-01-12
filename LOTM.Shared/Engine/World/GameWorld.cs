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
        protected Dictionary<int, GameObject> DynamicObjectLookupCache { get; set; }

        //protected List<GameObject> StaticObjects { get; }
        protected QuadTree StaticObjects { get; }
        protected Dictionary<int, GameObject> StaticObjectLookupCache { get; set; }

        public GameWorld()
        {
            DynamicObjects = new List<GameObject>();
            DynamicObjectLookupCache = new Dictionary<int, GameObject>();

            //StaticObjects = new List<GameObject>();
            StaticObjects = new QuadTree(new Rectangle(-10_000, -10_000, 20_000, 20_000));
            StaticObjectLookupCache = new Dictionary<int, GameObject>();
        }

        public void AddObject(GameObject gameObject)
        {
            if (gameObject is IMoveable)
            {
                DynamicObjects.Add(gameObject);

                if (gameObject.GetComponent<NetworkSynchronization>() is NetworkSynchronization networkSynchronization)
                {
                    DynamicObjectLookupCache.Add(networkSynchronization.NetworkId, gameObject);
                }
            }
            else
            {
                StaticObjects.Add(gameObject);

                if (gameObject.GetComponent<NetworkSynchronization>() is NetworkSynchronization networkSynchronization)
                {
                    StaticObjectLookupCache.Add(networkSynchronization.NetworkId, gameObject);
                }
            }
        }

        public void RemoveObject(GameObject gameObject)
        {
            if (gameObject is IMoveable)
            {
                DynamicObjects.Remove(gameObject);

                if (gameObject.GetComponent<NetworkSynchronization>() is NetworkSynchronization networkSynchronization)
                {
                    DynamicObjectLookupCache.Remove(networkSynchronization.NetworkId);
                }
            }
            else
            {
                StaticObjects.Remove(gameObject);

                if (gameObject.GetComponent<NetworkSynchronization>() is NetworkSynchronization networkSynchronization)
                {
                    StaticObjectLookupCache.Remove(networkSynchronization.NetworkId);
                }
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

        public GameObject GetGameObjectByNetworkId(int networkId)
        {
            if (DynamicObjectLookupCache.TryGetValue(networkId, out var dynamicObject)) return dynamicObject;

            if (StaticObjectLookupCache.TryGetValue(networkId, out var staticObject)) return staticObject;

            return null;
        }
    }
}
