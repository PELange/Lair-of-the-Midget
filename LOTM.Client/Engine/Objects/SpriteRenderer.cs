﻿using LOTM.Client.Engine.Graphics;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using System.Collections.Generic;

namespace LOTM.Client.Engine.Objects
{
    public class SpriteRenderer : IComponent
    {
        public class Segment
        {
            public int RenderLayer { get; set; }
            public Vector2 Size { get; set; }
            public Vector2 Offset { get; set; }
            public Sprite Sprite { get; set; }
            public Vector4 Color { get; set; }

            public Segment(Sprite sprite = null, Vector2 size = null, Vector2 offset = null, Vector4 color = null, int layer = 1000)
            {
                RenderLayer = layer;
                Size = size ?? new Vector2(1, 1);
                Offset = offset ?? Vector2.ZERO;
                Sprite = sprite;
                Color = color ?? new Vector4(1, 1, 1, 1);
            }
        }

        public List<Segment> Segments { get; }

        public SpriteRenderer(List<Segment> segments)
        {
            Segments = segments;
        }
    }
}
