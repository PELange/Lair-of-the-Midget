namespace LOTM.Shared.Engine.Math
{
    public class Vector3 : Vector2
    {
        public static new Vector3 ZERO => new Vector3(0, 0, 0);

        public double Z { get; set; }

        public Vector3(double x, double y, double z) : base(x, y)
        {
            Z = z;
        }
    }
}
