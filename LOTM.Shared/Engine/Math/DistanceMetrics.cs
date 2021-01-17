namespace LOTM.Shared.Engine.Math
{
    public class DistanceMetrics
    {
        public static double EuclideanSquared(Vector2 first, Vector2 second)
        {
            return (first.X - second.X) * (first.X - second.X) + (first.Y - second.Y) * (first.Y - second.Y);
        }

        public static double Euclidean(Vector2 first, Vector2 second)
        {
            return System.Math.Sqrt((first.X - second.X) * (first.X - second.X) + (first.Y - second.Y) * (first.Y - second.Y));
        }
    }
}
