using GlmNet;
using LOTM.Shared.Engine.Math;
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
            public fixed float TextureIndex[1];

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
                TextureIndex[0] = textureIndex;
            }
        }

        protected const int MAX_QUADS = 1000;

        protected OrthographicCamera Camera { get; set; }

        public Shader Shader { get; set; }
        public uint VertexArray { get; set; }
        public uint VertexBuffer { get; set; }
        public uint IndexBuffer { get; set; }


        public Texture2D Texture1 { get; set; }
        public Texture2D Texture2 { get; set; }

        public unsafe Renderer2D(OrthographicCamera camera)
        {
            Camera = camera;

            Shader = Shader.ColoredTexture();
            Shader.Bind();

            Shader.SetMatrix4("projection", Camera.GetProjectionMatrix());

            var samplers = new int[] { 0, 1 };
            fixed (int* data = samplers)
            {
                glUniform1iv(glGetUniformLocation(Shader.ID, "u_Textures"), 2, data);
            }

            Shader.Unbind();

            Texture1 = Texture2D.FromFile("Game/Assets/Textures/One.png");
            Texture2 = Texture2D.FromFile("Game/Assets/Textures/Two.png");
            Texture2 = Texture2D.FromColor(new Vector4(0.5, 0.23, 1, 0.23));

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

            glEnableVertexAttribArray(3);
            glVertexAttribPointer(3, 1, GL_FLOAT, false, sizeof(Vertex), Marshal.OffsetOf(typeof(Vertex), "TextureIndex"));

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
            IndexBuffer = IBO;
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, IBO);
            glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * maxIndices, indices, GL_STATIC_DRAW);

            //Unbind buffers
            glBindVertexArray(0);
            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }

        public unsafe void Render(GameWorld world)
        {
            var verticies = new List<Vertex>
            {
                new Vertex(new Vector2( 200, 200), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector2(0.0f, 0.0f), 0),
                new Vertex(new Vector2( 200, 250), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector2(0.0f, 1.0f), 0),
                new Vertex(new Vector2( 250, 200), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector2(1.0f, 0.0f), 0),
                new Vertex(new Vector2( 250, 250), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector2(1.0f, 1.0f), 0),

                new Vertex(new Vector2(  0,  0), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector2(0.0f, 0.0f), 0),
                new Vertex(new Vector2(  0, 50), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector2(0.0f, 1.0f), 0),
                new Vertex(new Vector2( 50,  0), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector2(1.0f, 0.0f), 0),
                new Vertex(new Vector2( 50, 50), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector2(1.0f, 1.0f), 0),

                new Vertex(new Vector2(100,  0), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector2(0.0f, 0.0f), 1),
                new Vertex(new Vector2(100, 50), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector2(0.0f, 1.0f), 1),
                new Vertex(new Vector2(150,  0), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector2(1.0f, 0.0f), 1),
                new Vertex(new Vector2(150, 50), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector2(1.0f, 1.0f), 1),

                new Vertex(new Vector2(200,  0), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector2(0.0f, 0.0f), 1),
                new Vertex(new Vector2(200, 50), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector2(0.0f, 1.0f), 1),
                new Vertex(new Vector2(250,  0), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector2(1.0f, 0.0f), 1),
                new Vertex(new Vector2(250, 50), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector2(1.0f, 1.0f), 1),
            };

            for (int i = 0; i < 4; i++)
            {
                var data = verticies[i];

                //var transform = glm.translate(mat4.identity(), new vec3(data.Position[0], data.Position[1], 0))
                //        * glm.rotate(mat4.identity(), glm.radians(45.0f), new vec3(0, 0, 1))
                //        * glm.scale(mat4.identity(), new vec3(1.5f, 1.5f, 1));
                ////transform = glm.translate(transform, new vec3(-data.Position[0], -data.Position[1], 0));

                ////var transform = glm.translate(mat4.identity(), new vec3(data.Position[0], data.Position[1], 1))
                ////    * glm.rotate(mat4.identity(), glm.radians(45.0f), new vec3(0, 0, 1))
                ////    * glm.scale(mat4.identity(), new vec3(1.0f, 1.0f, 1));

                ////float size = 1;
                ////float rotate = 45.0f;
                ////var transform = new mat4(1.0f);
                ////transform = glm.translate(new mat4(1.0f), new vec3(data.Position[0], data.Position[1], 0.0f));
                ////transform = glm.translate(transform, new vec3(0.5f * size, 0.5f * size, 0.0f));
                ////transform = glm.rotate(transform, glm.radians(rotate), new vec3(0.0f, 0.0f, 1.0f));
                ////transform = glm.translate(transform, new vec3(-0.5f * size, -0.5f * size, 0.0f));
                ////transform = glm.scale(transform, new vec3(size, size, 1.0f));

                //////var transform = glm.translate(mat4.identity(), new vec3(data.Position[0], data.Position[1], 0.0f));
                //////transform = glm.rotate(transform, glm.radians(45), new vec3(0.0f, 0.0f, 1.0f));
                //////transform = glm.translate(transform, new vec3(-data.Position[0], -data.Position[1], 0));

                ////var transformedPos = transform * qvp;
                //var transformedPos = transform * new vec4(1, 1, 1, 1);
                ////var transformedPos = transform * new vec4(10, 10, 1, 1);
                //data.Position[0] = transformedPos.x;
                //data.Position[1] = transformedPos.y;

                var v4In = new vec4(data.Position[0], data.Position[1], 0.0f, 1.0f);
                var v4RotCenter = new vec4(225, 225, 0.0f, 1.0f);
                var v3RotAxis = new vec3(0.0f, 0.0f, 1.0f);
                var matRot = glm.rotate(glm.radians(45.0f), v3RotAxis);
                var result = rotateAround(v4In, v4RotCenter, matRot);

                data.Position[0] = result.x;
                data.Position[1] = result.y;

                verticies.RemoveAt(i);
                verticies.Insert(i, data);
            }

            //Set dynamic vertex buffer
            glBindBuffer(GL_ARRAY_BUFFER, VertexBuffer);
            fixed (Vertex* data = verticies.ToArray())
            {
                glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(Vertex) * verticies.Count, data);
            }

            Shader.Bind();

            glActiveTexture(GL_TEXTURE0);
            Texture1.Bind();

            glActiveTexture(GL_TEXTURE1);
            Texture2.Bind();

            glBindVertexArray(VertexArray);

            glDrawElements(GL_TRIANGLES, (verticies.Count / 4) * 6, GL_UNSIGNED_INT, null);

            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindVertexArray(0);

            Shader.Unbind();
        }

        vec4 rotateAround(vec4 aPointToRotate, vec4 aRotationCenter, mat4 aRotationMatrix)
        {
            var translate = glm.translate(new mat4(1.0f), new vec3(aRotationCenter.x, aRotationCenter.y, aRotationCenter.z));
            var invTranslate = glm.inverse(translate);

            // The idea:
            // 1) Translate the object to the center
            // 2) Make the rotation
            // 3) Translate the object back to its original location

            var transform = translate * aRotationMatrix * invTranslate;

            return transform * aPointToRotate;
        }
    }
}
