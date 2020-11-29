using LOTM.Shared.Engine.Math;

namespace LOTM.Shared.Engine.Objects
{
    public class Transformation2D : IComponent
    {
        public Vector2 Position { get; set; }

        public double Rotation { get; set; }

        public Vector2 Scale { get; set; }
    }
}
