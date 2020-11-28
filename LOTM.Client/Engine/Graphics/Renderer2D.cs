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

            Shader = Shader.CameraShaderTexture();
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

            // --------- setup buffers
            /*var vertices = new float[] {
                 0,  0,     1.0f, 1.0f, 0.0f, 1.0f,     0.0f, 0.0f,     0.0f,
                 0, 50,     1.0f, 1.0f, 0.0f, 1.0f,     0.0f, 1.0f,     0.0f,
                50,  0,     1.0f, 1.0f, 0.0f, 1.0f,     1.0f, 0.0f,     0.0f,
                50, 50,     1.0f, 1.0f, 0.0f, 1.0f,     1.0f, 1.0f,     0.0f,

               100,  0,     1.0f, 0.0f, 1.0f, 1.0f,     0.0f, 0.0f,     1.0f,
               100, 50,     1.0f, 0.0f, 1.0f, 1.0f,     0.0f, 1.0f,     1.0f,
               150,  0,     1.0f, 0.0f, 1.0f, 1.0f,     1.0f, 0.0f,     1.0f,
               150, 50,     1.0f, 0.0f, 1.0f, 1.0f,     1.0f, 1.0f,     1.0f,
            };
            */

            var indices = new uint[] {
                0, 1, 2, 1, 3, 2,
                4+0, 4+1, 4+2, 4+1, 4+3, 4+2,
            };

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
            //glBufferData(GL_ARRAY_BUFFER, sizeof(Vertex) * 8, vertices, GL_STATIC_DRAW);
            glBufferData(GL_ARRAY_BUFFER, sizeof(Vertex) * (MAX_QUADS / 4), null, GL_DYNAMIC_DRAW);

            glEnableVertexAttribArray(0);
            glVertexAttribPointer(0, 3, GL_FLOAT, false, sizeof(Vertex), Marshal.OffsetOf(typeof(Vertex), "Position"));

            glEnableVertexAttribArray(1);
            glVertexAttribPointer(1, 4, GL_FLOAT, false, sizeof(Vertex), Marshal.OffsetOf(typeof(Vertex), "Color"));

            glEnableVertexAttribArray(2);
            glVertexAttribPointer(2, 2, GL_FLOAT, false, sizeof(Vertex), Marshal.OffsetOf(typeof(Vertex), "TextureCoordinates"));

            glEnableVertexAttribArray(3);
            glVertexAttribPointer(3, 1, GL_FLOAT, false, sizeof(Vertex), Marshal.OffsetOf(typeof(Vertex), "TextureIndex"));

            //Create index buffer
            uint IBO;
            glGenBuffers(1, &IBO);
            IndexBuffer = IBO;
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, IBO);
            glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indices.Length, indices, GL_STATIC_DRAW);

            //Unbind buffers
            glBindVertexArray(0);
            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }

        public unsafe void Render(GameWorld world)
        {
            var verticies = new List<Vertex>
            {
                new Vertex(new Vector2(  0,  0), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector2(0.0f, 0.0f), 0),
                new Vertex(new Vector2(  0, 50), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector2(0.0f, 1.0f), 0),
                new Vertex(new Vector2( 50,  0), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector2(1.0f, 0.0f), 0),
                new Vertex(new Vector2( 50, 50), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), new Vector2(1.0f, 1.0f), 0),

                new Vertex(new Vector2(100,  0), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector2(0.0f, 0.0f), 1),
                new Vertex(new Vector2(100, 50), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector2(0.0f, 1.0f), 1),
                new Vertex(new Vector2(150,  0), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector2(1.0f, 0.0f), 1),
                new Vertex(new Vector2(150, 50), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), new Vector2(1.0f, 1.0f), 1),
            };

            //Set dynamic vertex buffer
            glBindBuffer(GL_ARRAY_BUFFER, VertexBuffer);
            fixed (Vertex* data = verticies.ToArray())
            {
                glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(Vertex) * 8, data);
            }

            Shader.Bind();

            glActiveTexture(GL_TEXTURE0);
            Texture1.Bind();

            glActiveTexture(GL_TEXTURE1);
            Texture2.Bind();

            glBindVertexArray(VertexArray);

            glDrawElements(GL_TRIANGLES, 12, GL_UNSIGNED_INT, null);

            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindVertexArray(0);

            Shader.Unbind();
        }
    }
}
