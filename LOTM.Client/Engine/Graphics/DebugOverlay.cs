using LOTM.Shared.Engine.Math;
using System.Collections.Generic;

namespace LOTM.Client.Engine.Graphics
{
    public class DebugOverlay
    {
        public static Shader DebugLineShader { get; set; }

        public static List<(Vector2, Vector2, Vector4)> DebugLines { get; } = new List<(Vector2, Vector2, Vector4)>();

        public static void DrawBox(double x, double y, double width, double height, Vector4 color)
        {
            DebugLines.Add((new Vector2(x, y), new Vector2(x + width, y), color));
            DebugLines.Add((new Vector2(x + width, y), new Vector2(x + width, y + height), color));
            DebugLines.Add((new Vector2(x, y), new Vector2(x, y + height), color));
            DebugLines.Add((new Vector2(x, y + height), new Vector2(x + width, y + height), color));
        }
    }
}
