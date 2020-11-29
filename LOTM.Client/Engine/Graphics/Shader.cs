using GlmNet;
using static GLDotNet.GL;

namespace LOTM.Client.Engine.Graphics
{
    public class Shader
    {
        public uint ID { get; set; }

        public void Bind()
        {
            glUseProgram(ID);
        }

        public void Unbind()
        {
            glUseProgram(0);
        }

        public void SetMatrix4(string name, mat4 matrix)
        {
            unsafe
            {
                fixed (float* data = matrix.to_array())
                {
                    glUniformMatrix4fv(glGetUniformLocation(ID, name), 1, false, data);
                }
            }
        }

        public void SetInteger(string name, int value)
        {
            glUniform1i(glGetUniformLocation(ID, name), value);
        }

        public static unsafe Shader SpriteShader()
        {
            var shader = new Shader
            {
                ID = glCreateProgram()
            };

            var vertexSource = @"
                #version 330 core
                layout (location = 0) in vec2 position;
                layout (location = 1) in vec4 color;
                layout (location = 2) in vec2 textureCoord;
                
                uniform mat4 projection;

                out vec4 vertexColor;
                out vec2 vertexTextureCoord;

                void main()
                {
                    gl_Position = projection * vec4(position.x, position.y, 1.0, 1.0);
                    vertexColor = color;
                    vertexTextureCoord = textureCoord;
                }
            ";

            var fragmentSource = @"
                #version 330 core
                out vec4 color;
  
                in vec4 vertexColor;
                in vec2 vertexTextureCoord;
                
                uniform sampler2D textureSlot;

                void main()
                {
                    color = texture(textureSlot, vertexTextureCoord) * vertexColor;
                } 
            ";

            //Vertex shader compilation
            var vertexId = glCreateShader(GL_VERTEX_SHADER);
            glShaderSource(vertexId, vertexSource);
            glCompileShader(vertexId);
            glAttachShader(shader.ID, vertexId);

            //Fragment shader  compilation
            var fragmentId = glCreateShader(GL_FRAGMENT_SHADER);
            glShaderSource(fragmentId, fragmentSource);
            glCompileShader(fragmentId);
            glAttachShader(shader.ID, fragmentId);

            //Link shaders
            glLinkProgram(shader.ID);

            //Compilation cleanup
            glDeleteShader(vertexId);
            glDeleteShader(fragmentId);

            return shader;
        }
    }
}
