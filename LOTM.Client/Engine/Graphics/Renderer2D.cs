﻿using LOTM.Shared.Engine.World;

using static GLDotNet.GL;

namespace LOTM.Client.Engine.Graphics
{
    public class Renderer2D
    {
        protected OrthographicCamera Camera { get; set; }

        //public uint VertexArray { get; set; }
        //public uint VertexBuffer { get; set; }
        //public uint IndexBuffer { get; set; }

        public Shader Shader { get; set; }

        public Renderer2D(OrthographicCamera camera)
        {
            Camera = camera;

            Shader = Shader.CameraShaderColored();
            Shader.Bind();
            Shader.SetMatrix4("projection", Camera.GetProjectionMatrix());
            Shader.Unbind();
        }

        public unsafe void Render(GameWorld world)
        {
            Shader.Bind();

            var vertices = new float[] {
                100, 100, 1.0f, 1.0f, 0.0f, 1.0f,
                100, 200, 1.0f, 1.0f, 0.0f, 1.0f,
                200, 100, 1.0f, 1.0f, 0.0f, 1.0f,
                200, 200, 1.0f, 1.0f, 0.0f, 1.0f,

                200+100, 200+100, 1.0f, 0.0f, 1.0f, 1.0f,
                200+100, 200+200, 1.0f, 0.0f, 1.0f, 1.0f,
                200+200, 200+100, 1.0f, 0.0f, 1.0f, 1.0f,
                200+200, 200+200, 1.0f, 0.0f, 1.0f, 1.0f,
            };

            var indices = new uint[] {
                0, 1, 2, 1, 3, 2,
                4+0, 4+1, 4+2, 4+1, 4+3, 4+2,
            };

            //Create vertex array
            uint VAO;
            glGenVertexArrays(1, &VAO);
            glBindVertexArray(VAO);

            //Create vertex buffer
            uint VBO;
            glGenBuffers(1, &VBO);
            glBindBuffer(GL_ARRAY_BUFFER, VBO);
            glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, vertices, GL_STATIC_DRAW);

            glEnableVertexAttribArray(0);
            glVertexAttribPointer(0, 3, GL_FLOAT, false, 6 * sizeof(float), (void*)0);

            glEnableVertexAttribArray(1);
            glVertexAttribPointer(1, 4, GL_FLOAT, false, 6 * sizeof(float), (void*)(sizeof(float) * 2));

            //Create index buffer
            uint IBO;
            glGenBuffers(1, &IBO);
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, IBO);
            glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indices.Length, indices, GL_STATIC_DRAW);

            glDrawElements(GL_TRIANGLES, 12, GL_UNSIGNED_INT, null);

            Shader.Unbind();
        }
    }
}
