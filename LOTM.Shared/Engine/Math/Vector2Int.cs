namespace LOTM.Shared.Engine.Math
{
    public class Vector2Int
    {
        public static Vector2Int ZERO => new Vector2Int(0, 0);

        public int X { get; set; }
        public int Y { get; set; }

        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
