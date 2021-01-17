using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using System.Collections.Generic;

namespace LOTM.Client.Engine.Objects.Components
{
    public class TextRenderer : IComponent
    {
        public class Segment
        {
            public string Text { get; set; }
            public string FontName { get; set; }
            public int FontSize { get; set; }
            public Vector2 Offset { get; set; }
            public Vector4 Color { get; set; }
            public bool UseCenterPosition { get; set; }
            public int RenderLayer { get; set; }
            public bool Active { get; set; }

            public Segment(string text, string fontName, int fontSize, Vector2 offset = null, Vector4 color = null, bool useCenterPosition = true, int layer = 3000)
            {
                Text = text;
                FontName = fontName;
                FontSize = fontSize;
                Offset = offset ?? Vector2.ZERO;
                Color = color ?? new Vector4(1, 1, 1, 1);
                UseCenterPosition = useCenterPosition;
                RenderLayer = layer;
                Active = true;
            }
        }

        public List<Segment> Segments { get; }

        public TextRenderer(List<Segment> segments)
        {
            Segments = segments;
        }
    }
}
