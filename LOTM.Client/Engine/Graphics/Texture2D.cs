using ImageDotNet;
using LOTM.Shared.Engine.Math;
using System;
using static GLDotNet.GL;

namespace LOTM.Client.Engine.Graphics
{
    public class Texture2D
    {
        public uint ID { get; set; }

        public uint Width { get; set; }

        public uint Height { get; set; }

        public static unsafe Texture2D FromColor(Vector4 color)
        {
            var texture = new Texture2D
            {
                Width = 1,
                Height = 1
            };

            uint texId;
            glGenTextures(1, &texId);
            texture.ID = texId;

            // create Texture
            glBindTexture(GL_TEXTURE_2D, texture.ID);

            var bytes = new byte[4];
            bytes[0] = (byte)(255 * color.X);
            bytes[1] = (byte)(255 * color.Y);
            bytes[2] = (byte)(255 * color.Z);
            bytes[3] = (byte)(255 * color.W);

            fixed (byte* data = bytes)
            {
                glTexImage2D(GL_TEXTURE_2D, 0, (int)GL_RGBA, 1, 1, 0, GL_RGBA, GL_UNSIGNED_BYTE, data);
            }

            // set Texture wrap and filter modes
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, (int)GL_REPEAT);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, (int)GL_REPEAT);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, (int)GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, (int)GL_NEAREST);

            // unbind texture
            glBindTexture(GL_TEXTURE_2D, 0);

            return texture;
        }

        public static unsafe Texture2D FromFile(string sourceFile)
        {
            try
            {
                var texture = new Texture2D();

                // load image
                var image = Image.Load(sourceFile);

                using (var data = image.GetDataPointer())
                {
                    uint texId;
                    glGenTextures(1, &texId);
                    texture.ID = texId;

                    texture.Width = (uint)image.Width;
                    texture.Height = (uint)image.Height;

                    // create Texture
                    glBindTexture(GL_TEXTURE_2D, texture.ID);
                    glTexImage2D(GL_TEXTURE_2D, 0, (int)GL_RGBA, image.Width, image.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, data.Pointer);

                    // set Texture wrap and filter modes
                    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, (int)GL_REPEAT);
                    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, (int)GL_REPEAT);
                    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, (int)GL_NEAREST);
                    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, (int)GL_NEAREST);

                    // unbind texture
                    glBindTexture(GL_TEXTURE_2D, 0);
                }

                return texture;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Texture2D::FromFile() Failed to read texture '{sourceFile}' - Details {ex.Message}");
            }

            return null;
        }

        public void Bind()
        {
            glBindTexture(GL_TEXTURE_2D, ID);
        }

        public void Unbind()
        {
            glBindTexture(GL_TEXTURE_2D, 0);
        }
    }
}
