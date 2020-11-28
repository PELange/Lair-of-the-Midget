using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Object;

namespace LOTM.Client.Game.Objects
{
    class SpinnyCubeOfDeath : GameObject
    {
        public int EdgeLength { get; set; }

        public SpinnyCubeOfDeath(int edgeLength, Vector2 position, double angle) : base(position, angle)
        {
            EdgeLength = edgeLength;
        }
    }
}
