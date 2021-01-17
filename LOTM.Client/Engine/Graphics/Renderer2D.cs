using GlmNet;
using LOTM.Client.Engine.Objects.Components;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects.Components;
using LOTM.Shared.Engine.World;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static GLDotNet.GL;

namespace LOTM.Client.Engine.Graphics
{
    public class Renderer2D
    {
        unsafe struct Vertex
        {
            public fixed float Position[2];
            public fixed float Color[4];
            public fixed float TextureCoordinates[2];
            public float IsColorMasked;

            public Vertex(Vector2 position, Vector4 color, Vector2 textureCoordinates, float isColorMasked = 0)
            {
                Position[0] = (float)position.X;
                Position[1] = (float)position.Y;
                Color[0] = (float)color.X;
                Color[1] = (float)color.Y;
                Color[2] = (float)color.Z;
                Color[3] = (float)color.W;
                TextureCoordinates[0] = (float)textureCoordinates.X;
                TextureCoordinates[1] = (float)textureCoordinates.Y;
                IsColorMasked = isColorMasked;
            }
        }

        class LayeredVertex
        {
            public Vertex Vertex { get; set; }
            public int Layer { get; set; }
            public double ZSort { get; set; }
        }

        protected const int MAX_QUADS = 25000;

        protected OrthographicCamera Camera { get; set; }

        public Shader QuadTextureShader { get; set; }
        public uint VertexArray { get; set; }
        public uint VertexBuffer { get; set; }

        public unsafe Renderer2D(OrthographicCamera camera)
        {
            Camera = camera;

            QuadTextureShader = Shader.QuadTextureShader();

            DebugOverlay.DebugLineShader = Shader.DebugLineShader();

            //Setup white placehold texture
            AssetManager.RegisterTexture(Texture2D.FromColor(Vector4.ONE), "white");

            // --------- setup buffers

            //Create vertex array
            uint VAO;
            glGenVertexArrays(1, &VAO);
            glBindVertexArray(VAO);
            VertexArray = VAO;

            //Create vertex buffer
            uint VBO;
            glGenBuffers(1, &VBO);
            VertexBuffer = VBO;
            glBindBuffer(GL_ARRAY_BUFFER, VBO);
            glBufferData(GL_ARRAY_BUFFER, sizeof(Vertex) * MAX_QUADS * 4, null, GL_DYNAMIC_DRAW);

            glEnableVertexAttribArray(0);
            glVertexAttribPointer(0, 2, GL_FLOAT, false, sizeof(Vertex), Marshal.OffsetOf(typeof(Vertex), "Position"));

            glEnableVertexAttribArray(1);
            glVertexAttribPointer(1, 4, GL_FLOAT, false, sizeof(Vertex), Marshal.OffsetOf(typeof(Vertex), "Color"));

            glEnableVertexAttribArray(2);
            glVertexAttribPointer(2, 2, GL_FLOAT, false, sizeof(Vertex), Marshal.OffsetOf(typeof(Vertex), "TextureCoordinates"));

            glEnableVertexAttribArray(3);
            glVertexAttribPointer(3, 1, GL_FLOAT, false, sizeof(Vertex), Marshal.OffsetOf(typeof(Vertex), "IsColorMasked"));

            //Create index buffer
            const int maxIndices = MAX_QUADS * 6;
            var indices = new uint[maxIndices];
            for (uint i = 0, offset = 0; i < maxIndices; i += 6, offset += 4)
            {
                indices[i + 0] = 0 + offset;
                indices[i + 1] = 1 + offset;
                indices[i + 2] = 2 + offset;

                indices[i + 3] = 1 + offset;
                indices[i + 4] = 3 + offset;
                indices[i + 5] = 2 + offset;
            }

            uint IBO;
            glGenBuffers(1, &IBO);
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, IBO);
            glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * maxIndices, indices, GL_STATIC_DRAW);

            //Unbind buffers
            glBindVertexArray(0);
            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }

        public unsafe void Render(GameWorld world)
        {
            QuadTextureShader.Bind();

            //Set current camera projection
            QuadTextureShader.SetMatrix4("projection", Camera.GetProjectionMatrix());

            //Setup texture slots
            QuadTextureShader.SetInteger("textureSlot", 1);
            QuadTextureShader.SetInteger("colorMaskSlot", 2);

            //Collect all verticies to be drawn for the current view
            var viewPort = Camera.GetViewport();
            var searchRect = new Rectangle(viewPort.TopLeft.X, viewPort.TopLeft.Y, System.Math.Abs(viewPort.BottomRight.X - viewPort.TopLeft.X), System.Math.Abs(viewPort.BottomRight.Y - viewPort.TopLeft.Y));

            var worldObjects = world.GetObjectsInArea(searchRect);

            var layeredVerticies = new List<LayeredVertex>();

            bool spriteTextureBound = false;
            bool fontTextureBound = false;

            foreach (var worldObject in worldObjects)
            {
                if (worldObject.GetComponent<SpriteRenderer>() is SpriteRenderer spriteRenderer)
                {
                    if (!spriteTextureBound && spriteRenderer.Segments.Count > 0)
                    {
                        //Bind sprite textures
                        glActiveTexture(GL_TEXTURE1);
                        glBindTexture(GL_TEXTURE_2D, spriteRenderer.Segments[0].Sprite.Texture.ID);
                        spriteTextureBound = true;
                    }

                    foreach (var segment in spriteRenderer.Segments)
                    {
                        if (!segment.Active) continue;

                        var textureCoordinates = segment.Sprite.TextureCoordinates;

                        if (segment.VerticalFlip)
                        {
                            textureCoordinates = new Vector4(textureCoordinates.Z, textureCoordinates.Y, textureCoordinates.X, textureCoordinates.W);
                        }

                        var transformation = worldObject.GetComponent<Transformation2D>();

                        var offsetX = transformation.Scale.X * segment.Offset.X;
                        var offsetY = transformation.Scale.Y * segment.Offset.Y;

                        var quad = new List<Vertex>
                        {
                            //Top left quad vertex
                            new Vertex(
                                new Vector2(transformation.Position.X + offsetX, transformation.Position.Y + offsetY),
                                segment.Color,
                                new Vector2(textureCoordinates.X, textureCoordinates.Y)),

                            //Top right quad vertex
                            new Vertex(
                                new Vector2(transformation.Position.X + offsetX + transformation.Scale.X * segment.Size.X, transformation.Position.Y + offsetY),
                                segment.Color,
                                new Vector2(textureCoordinates.Z, textureCoordinates.Y)),

                            //Bottom left quad vertex
                            new Vertex(
                                new Vector2(transformation.Position.X + offsetX, transformation.Position.Y + offsetY + transformation.Scale.Y * segment.Size.Y),
                                segment.Color,
                                new Vector2(textureCoordinates.X, textureCoordinates.W)),

                            //Bottom right quad vertex
                            new Vertex(
                                new Vector2(transformation.Position.X + offsetX + transformation.Scale.X * segment.Size.X, transformation.Position.Y + offsetY + transformation.Scale.Y * segment.Size.Y),
                                segment.Color,
                                new Vector2(textureCoordinates.Z, textureCoordinates.W))
                        };

                        //Apply quad center rotation
                        for (int nVertex = 0; nVertex < 4; nVertex++)
                        {
                            var vertex = quad[nVertex];

                            var totalRotation = transformation.Rotation + segment.Rotation;

                            var rotationCenter = new Vector2(transformation.Position.X + (transformation.Scale.X / 2), transformation.Position.Y + (transformation.Scale.Y / 2));

                            rotationCenter.X += transformation.Scale.X * segment.RotationCenterOffset.X;
                            rotationCenter.Y += transformation.Scale.Y * segment.RotationCenterOffset.Y;

                            if (totalRotation != default)
                            {
                                var translate = glm.translate(mat4.identity(), new vec3((float)rotationCenter.X, (float)rotationCenter.Y, 0));

                                var rotated = translate
                                    * glm.rotate(glm.radians((float)totalRotation), new vec3(0.0f, 0.0f, 1.0f))
                                    * glm.inverse(translate)
                                    * new vec4(vertex.Position[0], vertex.Position[1], 0.0f, 1.0f);

                                vertex.Position[0] = rotated.x;
                                vertex.Position[1] = rotated.y;
                            }

                            layeredVerticies.Add(new LayeredVertex
                            {
                                Vertex = vertex,
                                Layer = segment.RenderLayer,
                                ZSort = transformation.Position.Y
                            });
                        }
                    }
                }

                if (worldObject.GetComponent<TextRenderer>() is TextRenderer textRenderer && textRenderer.Segments.Count > 0)
                {
                    var preloadFont = textRenderer.Segments.Where(x => x.Active).FirstOrDefault();

                    if (preloadFont == null) continue;

                    var font = AssetManager.GetFont(preloadFont.FontName);

                    if (!fontTextureBound)
                    {
                        //Bind sprite textures
                        glActiveTexture(GL_TEXTURE2);
                        glBindTexture(GL_TEXTURE_2D, font.Bitmap.ID);
                        fontTextureBound = true;
                    }

                    foreach (var segment in textRenderer.Segments)
                    {
                        var transformation = worldObject.GetComponent<Transformation2D>();

                        var startX = transformation.Position.X + transformation.Scale.X * segment.Offset.X;
                        var initialX = startX;
                        var startY = transformation.Position.Y + transformation.Scale.Y * segment.Offset.Y;

                        var fontSizeScale = segment.FontSize / (font.BitMapFontSize * 1.0);

                        var textVerticies = new List<LayeredVertex>();

                        foreach (var character in segment.Text)
                        {
                            if (!font.Charaters.TryGetValue(character, out var charInfo))
                            {
                                charInfo = font.Charaters['?']; //Replace unknown chars by questionmarks
                            }

                            var left = startX + charInfo.Bearing.X * fontSizeScale;
                            var top = startY - charInfo.Bearing.Y * fontSizeScale;

                            //Top left quad vertex
                            textVerticies.Add(new LayeredVertex
                            {
                                Vertex = new Vertex(new Vector2(left, top), segment.Color, new Vector2(charInfo.TextureCoordinates.X, charInfo.TextureCoordinates.Y), 1),
                                Layer = segment.RenderLayer,
                                ZSort = transformation.Position.Y
                            });

                            //Top right quad vertex
                            textVerticies.Add(new LayeredVertex
                            {
                                Vertex = new Vertex(new Vector2(left + charInfo.Size.X * fontSizeScale, top), segment.Color, new Vector2(charInfo.TextureCoordinates.Z, charInfo.TextureCoordinates.Y), 1),
                                Layer = segment.RenderLayer,
                                ZSort = transformation.Position.Y
                            });

                            //Bottom left quad vertex
                            textVerticies.Add(new LayeredVertex
                            {
                                Vertex = new Vertex(new Vector2(left, top + charInfo.Size.Y * fontSizeScale), segment.Color, new Vector2(charInfo.TextureCoordinates.X, charInfo.TextureCoordinates.W), 1),
                                Layer = segment.RenderLayer,
                                ZSort = transformation.Position.Y
                            });

                            //Bottom right quad vertex
                            textVerticies.Add(new LayeredVertex
                            {
                                Vertex = new Vertex(new Vector2(left + charInfo.Size.X * fontSizeScale, top + charInfo.Size.Y * fontSizeScale), segment.Color, new Vector2(charInfo.TextureCoordinates.Z, charInfo.TextureCoordinates.W), 1),
                                Layer = segment.RenderLayer,
                                ZSort = transformation.Position.Y
                            });

                            startX += charInfo.Advance * fontSizeScale;
                        }

                        if (segment.UseCenterPosition && textVerticies.Count > 2)
                        {
                            var xCenterOffset = (startX - initialX) / 2.0;

                            for (int nVertex = 0; nVertex < textVerticies.Count; nVertex++)
                            {
                                var element = textVerticies[nVertex].Vertex;

                                element.Position[0] -= (float)xCenterOffset;

                                layeredVerticies.Add(new LayeredVertex
                                {
                                    Vertex = element,
                                    Layer = segment.RenderLayer,
                                    ZSort = transformation.Position.Y
                                });
                            }
                        }
                        else
                        {
                            layeredVerticies.AddRange(textVerticies);
                        }
                    }
                }
            }

            //Sort layers and select the result verticies again
            var verticies = layeredVerticies.OrderBy(x => x.Layer).ThenBy(x => x.ZSort).Select(x => x.Vertex).ToList();

            //Set dynamic vertex buffer
            glBindBuffer(GL_ARRAY_BUFFER, VertexBuffer);
            fixed (Vertex* data = verticies.ToArray())
            {
                glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(Vertex) * verticies.Count, data);
            }

            glBindVertexArray(VertexArray);

            glDrawElements(GL_TRIANGLES, (verticies.Count / 4) * 6, GL_UNSIGNED_INT, null);

            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindVertexArray(0);

            QuadTextureShader.Unbind();

            //Debug overlay
            if (DebugOverlay.DebugLines.Count > 0)
            {
                DebugOverlay.DebugLineShader.Bind();
                DebugOverlay.DebugLineShader.SetMatrix4("projection", Camera.GetProjectionMatrix());

                uint lineVAO, lineVBO;
                glGenVertexArrays(1, &lineVAO);
                glGenBuffers(1, &lineVBO);
                glBindVertexArray(lineVAO);
                glBindBuffer(GL_ARRAY_BUFFER, lineVBO);

                foreach (var line in DebugOverlay.DebugLines)
                {
                    var lineData = new float[]
                    {
                        (float)line.Item1.X, (float)line.Item1.Y, (float)line.Item3.X, (float)line.Item3.Y, (float)line.Item3.Z, (float)line.Item3.W,

                        (float)line.Item2.X, (float)line.Item2.Y, (float)line.Item3.X, (float)line.Item3.Y, (float)line.Item3.Z, (float)line.Item3.W,
                    };

                    fixed (float* data = lineData)
                    {
                        glBufferData(GL_ARRAY_BUFFER, sizeof(float) * lineData.Length, data, GL_STATIC_DRAW);
                    }

                    glEnableVertexAttribArray(0);
                    glVertexAttribPointer(0, 2, GL_FLOAT, false, sizeof(float) * 6, (void*)0);

                    glEnableVertexAttribArray(1);
                    glVertexAttribPointer(1, 4, GL_FLOAT, false, sizeof(float) * 6, (void*)(sizeof(float) * 2));

                    glDrawArrays(GL_LINES, 0, 2);
                }

                DebugOverlay.DebugLines.Clear();
                DebugOverlay.DebugLineShader.Unbind();
            }
        }
    }
}
