using ImageDotNet;
using System;
using static GLDotNet.GL;

namespace LOTM.Client.Engine.Graphics
{
    class Texture2D
    {
        public uint ID { get; set; }

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

                    // create Texture
                    glBindTexture(GL_TEXTURE_2D, texture.ID);
                    glTexImage2D(GL_TEXTURE_2D, 0, (int)GL_RGBA, image.Width, image.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, data.Pointer);

                    // set Texture wrap and filter modes
                    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, (int)GL_REPEAT);
                    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, (int)GL_REPEAT);
                    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, (int)GL_LINEAR);
                    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, (int)GL_LINEAR);

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
