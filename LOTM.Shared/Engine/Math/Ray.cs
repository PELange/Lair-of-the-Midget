namespace LOTM.Shared.Engine.Math
{
    public class Ray
    {
        public Vector2 Origin { get; set; }

        public Vector2 Direction { get; set; }

        public Ray(Vector2 origin, Vector2 direction)
        {
            Origin = origin;
            Direction = direction;
        }

        public Ray(double originX, double originY, double directionX, double directionY)
            : this(new Vector2(originX, originY), new Vector2(directionX, directionY))
        {
        }
    }
}
