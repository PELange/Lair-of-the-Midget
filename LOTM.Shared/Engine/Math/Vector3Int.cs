namespace LOTM.Shared.Engine.Math
{
    public class Vector3Int : Vector2Int
    {
        public static new Vector3Int ZERO => new Vector3Int(0, 0, 0);

        public int Z { get; set; }

        public Vector3Int(int x, int y, int z) : base(x, y)
        {
            Z = z;
        }
    }
}
