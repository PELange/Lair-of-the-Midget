namespace LOTM.Shared.Engine.Math
{
    public class Vector4 : Vector3
    {
        public static new Vector4 ZERO => new Vector4(0, 0, 0, 0);

        public double W { get; set; }

        public Vector4(double x, double y, double z, double w) : base(x, y, z)
        {
            W = w;
        }
    }
}
