using LOTM.Shared.Engine.Object;
using System.Collections.Generic;

namespace LOTM.Shared.Engine.World
{
    public class GameWorld
    {
        public ICollection<GameObject> Objects { get; set; } = new List<GameObject>();
    }
}
