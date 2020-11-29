using LOTM.Shared.Engine.Math;

namespace LOTM.Client.Engine.Graphics
{
    public class Sprite
    {
        public Texture2D Texture { get; set; }

        public Vector4 TextureCoordinates { get; set; }

        public Sprite(Texture2D texture, Vector4 textureCoordinates)
        {
            Texture = texture;
            TextureCoordinates = textureCoordinates;
        }
    }
}
