using LOTM.Client.Engine.Graphics;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;

namespace LOTM.Client.Engine.Objects
{
    public class SpriteRenderer : IComponent
    {
        public Sprite Sprite { get; set; }

        public Vector4 Color { get; set; }

        public SpriteRenderer(Sprite sprite, Vector4 color = null)
        {
            Sprite = sprite;

            Color = color ?? new Vector4(1, 1, 1, 1);
        }
    }
}
