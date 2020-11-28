namespace LOTM.Shared.Engine.Math
{
    public class Vector2
    {
        public static Vector2 ZERO => new Vector2(0, 0);

        public double X { get; set; }
        public double Y { get; set; }

        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}