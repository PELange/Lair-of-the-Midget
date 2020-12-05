using LOTM.Shared.Engine.Objects;
using System.Collections.Generic;

namespace LOTM.Shared.Engine.World
{
    public class GameWorld
    {
        //public List<GameObject> Objects { get; set; } = new List<GameObject>();

        public QuadTree<GameObject> Objects { get; set; }

        public GameWorld(int width, int height)
        {
            Objects = new QuadTree<GameObject>(new System.Drawing.RectangleF(-width, -height, width * 2, height * 2))
            {
                GetBounds = obj =>
                {
                    var transformation = obj.GetComonent<Transformation2D>();

                    return new System.Drawing.RectangleF((float)transformation.Position.X, (float)transformation.Position.Y, (float)transformation.Scale.X, (float)transformation.Scale.Y);
                }
            };
        }
    }
}
