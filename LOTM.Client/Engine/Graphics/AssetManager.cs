﻿using LOTM.Client.Engine.Graphics;
using LOTM.Shared.Engine.Math;
using System.Collections.Generic;

namespace LOTM.Client.Engine
{
    public class AssetManager
    {
        protected static Dictionary<string, Texture2D> Textures { get; set; } = new Dictionary<string, Texture2D>();
        protected static Dictionary<string, Font> Fonts { get; set; } = new Dictionary<string, Font>();
        protected static Dictionary<string, Sprite> Sprites { get; set; } = new Dictionary<string, Sprite>();

        public static bool RegisterTexture(string texturePath, string textureName)
        {
            var texture = Texture2D.FromFile(texturePath);

            Textures[textureName] = texture;

            return texture != null;
        }

        public static bool RegisterTexture(Texture2D texture, string textureName)
        {
            Textures[textureName] = texture;

            return texture != null;
        }

        public static void RegisterSpriteByGridIndex(string textureName, int gridsize, Vector4Int textureCoordinates, string spriteName)
        {
            var texture = Textures[textureName];

            var atlasCoordinats = Vector4.ZERO;

            atlasCoordinats.X = gridsize * textureCoordinates.X;
            atlasCoordinats.Y = gridsize * textureCoordinates.Y;

            atlasCoordinats.Z = (gridsize * textureCoordinates.Z) + gridsize;
            atlasCoordinats.W = (gridsize * textureCoordinates.W) + gridsize;

            atlasCoordinats.X /= texture.Width;
            atlasCoordinats.Z /= texture.Width;

            atlasCoordinats.Y /= texture.Height;
            atlasCoordinats.W /= texture.Height;

            Sprites[spriteName] = new Sprite(texture, atlasCoordinats);
        }

        public static Texture2D GetTexture(string textureName)
        {
            return Textures[textureName];
        }

        public static Sprite GetSprite(string spriteName)
        {
            return Sprites[spriteName];
        }

        public static bool RegisterFont(string fontFilePath, uint fontSize, string fontName)
        {
            var font = Font.FromFile(fontFilePath, fontSize);

            Fonts[fontName] = font;

            return font != null;
        }

        public static Font GetFont(string fontName)
        {
            return Fonts[fontName];
        }
    }
}
