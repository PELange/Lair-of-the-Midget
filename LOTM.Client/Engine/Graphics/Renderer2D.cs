using GlmNet;
using LOTM.Client.Engine.Objects;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using LOTM.Shared.Engine.World;
using System.Collections.Generic;
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

            public Vertex(Vector2 position, Vector4 color, Vector2 textureCoordinates, float textureIndex)
            {
                Position[0] = (float)position.X;
                Position[1] = (float)position.Y;
                Color[0] = (float)color.X;
                Color[1] = (float)color.Y;
                Color[2] = (float)color.Z;
                Color[3] = (float)color.W;
                TextureCoordinates[0] = (float)textureCoordinates.X;
                TextureCoordinates[1] = (float)textureCoordinates.Y;
            }
        }

        protected const int MAX_QUADS = 1000;

        protected OrthographicCamera Camera { get; set; }

        public Shader WorldObjectShader { get; set; }
        public uint VertexArray { get; set; }
        public uint VertexBuffer { get; set; }

        public unsafe Renderer2D(OrthographicCamera camera)
        {
            Camera = camera;

            WorldObjectShader = Shader.SpriteShader();

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
            glVertexAttribPointer(0, 3, GL_FLOAT, false, sizeof(Vertex), Marshal.OffsetOf(typeof(Vertex), "Position"));

            glEnableVertexAttribArray(1);
            glVertexAttribPointer(1, 4, GL_FLOAT, false, sizeof(Vertex), Marshal.OffsetOf(typeof(Vertex), "Color"));

            glEnableVertexAttribArray(2);
            glVertexAttribPointer(2, 2, GL_FLOAT, false, sizeof(Vertex), Marshal.OffsetOf(typeof(Vertex), "TextureCoordinates"));

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
            WorldObjectShader.Bind();

            //Set current camera projection
            WorldObjectShader.SetMatrix4("projection", Camera.GetProjectionMatrix());

            //todo refactor use of current texture better. currently hardcoded to texture1 on slot0
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, 1);
            WorldObjectShader.SetInteger("textureSlot", 0);

            //Collect all verticies to be drawn for the current view
            var verticies = new List<Vertex>();

            foreach (var worldObject in world.Objects)
            {
                if (worldObject.GetComonent<SpriteRenderer>() is SpriteRenderer spriteRenderer)
                {
                    var textureCoordinates = spriteRenderer.Sprite.TextureCoordinates;
                    var transformation = worldObject.GetComonent<Transformation2D>();

                    var quad = new List<Vertex>
                    {
                        //Top left quad vertex
                        new Vertex(
                        new Vector2(transformation.Position.X, transformation.Position.Y),
                        spriteRenderer.Color,
                        new Vector2(textureCoordinates.X, textureCoordinates.Y),
                        spriteRenderer.Sprite.Texture.ID),

                        //Top right quad vertex
                        new Vertex(
                        new Vector2(transformation.Position.X + transformation.Scale.X, transformation.Position.Y),
                        spriteRenderer.Color,
                        new Vector2(textureCoordinates.Z, textureCoordinates.Y),
                        spriteRenderer.Sprite.Texture.ID),

                        //Bottom left quad vertex
                        new Vertex(
                        new Vector2(transformation.Position.X, transformation.Position.Y + transformation.Scale.Y),
                        spriteRenderer.Color,
                        new Vector2(textureCoordinates.X, textureCoordinates.W),
                        spriteRenderer.Sprite.Texture.ID),

                        //Bottom right quad vertex
                        new Vertex(
                        new Vector2(transformation.Position.X + transformation.Scale.X, transformation.Position.Y + transformation.Scale.Y),
                        spriteRenderer.Color,
                        new Vector2(textureCoordinates.Z, textureCoordinates.W),
                        spriteRenderer.Sprite.Texture.ID)
                    };

                    //Apply quad center rotation
                    for (int nVertex = 0; nVertex < 4; nVertex++)
                    {
                        var vertex = quad[nVertex];

                        var translate = glm.translate(
                            mat4.identity(),
                            new vec3(
                                (float)(transformation.Position.X + (transformation.Scale.X / 2)),
                                (float)(transformation.Position.Y + (transformation.Scale.Y / 2)),
                                0));

                        var rotated = translate
                            * glm.rotate(glm.radians((float)transformation.Rotation), new vec3(0.0f, 0.0f, 1.0f))
                            * glm.inverse(translate)
                            * new vec4(vertex.Position[0], vertex.Position[1], 0.0f, 1.0f);

                        vertex.Position[0] = rotated.x;
                        vertex.Position[1] = rotated.y;

                        verticies.Add(vertex);
                    }
                }
            }

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

            WorldObjectShader.Unbind();
        }
    }
}
