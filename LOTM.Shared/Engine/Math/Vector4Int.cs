namespace LOTM.Shared.Engine.Math
{
    public class Vector4Int : Vector3Int
    {
        public static new Vector4Int ZERO => new Vector4Int(0, 0, 0, 0);

        public int W { get; set; }

        public Vector4Int(int x, int y, int z, int w) : base(x, y, z)
        {
            W = w;
        }
    }
}
