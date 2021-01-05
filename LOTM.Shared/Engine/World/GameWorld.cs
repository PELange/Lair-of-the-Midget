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

        protected List<GameObject> StaticObjects { get; }
        protected Dictionary<int, GameObject> StaticObjectLookupCache { get; set; }

        //public QuadTree<GameObject> Objects { get; set; }

        public GameWorld()
        {
            DynamicObjects = new List<GameObject>();
            DynamicObjectLookupCache = new Dictionary<int, GameObject>();

            StaticObjects = new List<GameObject>();
            StaticObjectLookupCache = new Dictionary<int, GameObject>();

            //Objects = new QuadTree<GameObject>(new System.Drawing.RectangleF(-width, -height, width * 2, height * 2))
            //{
            //    GetBounds = obj =>
            //    {
            //        var transformation = obj.GetComonent<Transformation2D>();

            //        return new System.Drawing.RectangleF((float)transformation.Position.X, (float)transformation.Position.Y, (float)transformation.Scale.X, (float)transformation.Scale.Y);
            //    }
            //};
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

        public IEnumerable<GameObject> GetAllObjects()
        {
            return DynamicObjects.Concat(StaticObjects);
        }

        public IEnumerable<GameObject> GetDynamicObjects()
        {
            return DynamicObjects;
        }

        public GameObject GetGameObjectByNetworkId(int networkId)
        {
            if (DynamicObjectLookupCache.TryGetValue(networkId, out var dynamicObject)) return dynamicObject;

            if (StaticObjectLookupCache.TryGetValue(networkId, out var staticObject)) return staticObject;

            return null;
        }
    }
}
