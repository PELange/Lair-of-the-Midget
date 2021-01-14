using FreeTypeSharp;
using LOTM.Shared.Engine.Math;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static FreeTypeSharp.Native.FT;


namespace LOTM.Client.Engine.Graphics
{
    public class Font
    {
        public Texture2D Bitmap { get; set; }

        public int BitMapFontSize { get; set; }

        public Dictionary<char, Charater> Charaters { get; }

        public Font()
        {
            Charaters = new Dictionary<char, Charater>();
        }

        public class Charater
        {
            public Vector4 TextureCoordinates { get; }
            public Vector2 Size { get; }
            public Vector2 Bearing { get; }
            public int Advance { get; }

            public Charater(Vector4 textureCoordinates, Vector2 size, Vector2 bearing, int advance)
            {
                TextureCoordinates = textureCoordinates;
                Size = size;
                Bearing = bearing;
                Advance = advance;
            }
        }

        private class RawTTFCharData
        {
            public int CharCode { get; }
            public byte[] GlyphData { get; }
            public uint GlyphWidth { get; }
            public uint GlyphHeight { get; }
            public int GlyphLeft { get; }
            public int GlyphTop { get; }
            public int GlyphAdvance { get; }

            public RawTTFCharData(int charCode, byte[] glyphData, uint glyphWidth, uint glyphHeight, int glyphLeft, int glyphTop, int glyphAdvance)
            {
                CharCode = charCode;
                GlyphData = glyphData;
                GlyphWidth = glyphWidth;
                GlyphHeight = glyphHeight;
                GlyphLeft = glyphLeft;
                GlyphTop = glyphTop;
                GlyphAdvance = glyphAdvance;
            }
        }

        public static Font FromFile(string fontFilePath, uint fontSize)
        {
            var font = new Font
            {
                BitMapFontSize = (int)fontSize
            };

            var library = new FreeTypeLibrary();

            if (FT_New_Face(library.Native, fontFilePath, 0, out var face) != 0)
            {
                System.Console.WriteLine($"Failed to load font from path '{fontFilePath}'.");
            }

            //Load glyphs in correct size
            FT_Set_Pixel_Sizes(face, 0, fontSize);

            var facade = new FreeTypeFaceFacade(library, face);

            //Load all ASCII chars
            var characters = new List<RawTTFCharData>();

            for (int charCode = 0; charCode < 128; charCode++)
            {
                //Load renderable glyph into facade buffer
                if (FT_Load_Char(face, (uint)charCode, FT_LOAD_RENDER) != 0)
                {
                    System.Console.WriteLine($"Failed to load font glyphs for code '{charCode}'.");
                    continue;
                }

                var glypthByteCount = (int)(facade.GlyphBitmap.width * facade.GlyphBitmap.rows);

                byte[] glyphData;

                if (glypthByteCount > 0)
                {
                    glyphData = new byte[glypthByteCount];
                    Marshal.Copy(facade.GlyphBitmap.buffer, glyphData, 0, glypthByteCount);
                }
                else
                {
                    glyphData = new byte[0];
                }

                characters.Add(new RawTTFCharData(charCode, glyphData, facade.GlyphBitmap.width, facade.GlyphBitmap.rows, facade.GlyphBitmapLeft, facade.GlyphBitmapTop, facade.GlyphMetricHorizontalAdvance));
            }

            //Free up ttf library allocation
            FT_Done_Face(face);
            FT_Done_FreeType(library.Native);

            //Setup combined texture buffer
            var maxGlyphWidth = (int)characters.Max(x => x.GlyphWidth);
            var maxGlyphHeight = (int)characters.Max(x => x.GlyphHeight);
            var totalWidth = maxGlyphWidth * characters.Count;
            var combinedBuffer = Enumerable.Repeat((byte)0x00, totalWidth * maxGlyphHeight).ToArray();

            //Iterate through each char, filling in their position in the buffer
            for (int nCharacter = 0; nCharacter < characters.Count; nCharacter++)
            {
                var currentChar = characters[nCharacter];

                for (int nRow = 0; nRow < currentChar.GlyphHeight; nRow++)
                {
                    for (int nCol = 0; nCol < currentChar.GlyphWidth; nCol++)
                    {
                        combinedBuffer[(nRow * totalWidth) + (nCharacter * maxGlyphWidth) + nCol] = currentChar.GlyphData[(nRow * currentChar.GlyphWidth) + nCol];
                    }
                }

                font.Charaters.Add(
                    (char)currentChar.CharCode,
                    new Charater(
                        new Vector4(nCharacter * maxGlyphWidth / (totalWidth * 1.0), 0, (nCharacter * maxGlyphWidth + currentChar.GlyphWidth) / (totalWidth * 1.0), currentChar.GlyphHeight / (maxGlyphHeight * 1.0)),
                        new Vector2(currentChar.GlyphWidth, currentChar.GlyphHeight),
                        new Vector2(currentChar.GlyphLeft, currentChar.GlyphTop),
                        currentChar.GlyphAdvance));
            }

            font.Bitmap = Texture2D.FromFontData(combinedBuffer, (uint)totalWidth, (uint)maxGlyphHeight);

            return font;
        }
    }
}
