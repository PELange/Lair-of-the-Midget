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

        public static unsafe Shader SimpleShader()
        {
            var shader = new Shader
            {
                ID = glCreateProgram()
            };

            var vertexSource = @"
                #version 330 core
                layout (location = 0) in vec2 position;
                //layout (location = 1) in vec4 color;
  
                out vec4 vertexColor;

                void main()
                {
                    gl_Position = vec4(position.x, position.y, 1.0, 1.0);
                    vertexColor = vec4(1.0, 0.0, 1.0, 1.0); //color;
                }
            ";

            var fragmentSource = @"
                #version 330 core
                out vec4 FragColor;
  
                in vec4 vertexColor;

                void main()
                {
                    FragColor = vertexColor;
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

        public static unsafe Shader CameraShader()
        {
            var shader = new Shader
            {
                ID = glCreateProgram()
            };

            var vertexSource = @"
                #version 330 core
                layout (location = 0) in vec2 position;
                
                uniform mat4 projection;

                out vec4 vertexColor;

                void main()
                {
                    gl_Position = projection * vec4(position.x, position.y, 1.0, 1.0);
                    vertexColor = vec4(1.0, 1.0, 1.0, 1.0);
                }
            ";

            var fragmentSource = @"
                #version 330 core
                out vec4 color;
  
                in vec4 vertexColor;

                void main()
                {
                    color = vertexColor;
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

        public static unsafe Shader CameraShaderColored()
        {
            var shader = new Shader
            {
                ID = glCreateProgram()
            };

            var vertexSource = @"
                #version 330 core
                layout (location = 0) in vec2 position;
                layout (location = 1) in vec4 color;
                
                uniform mat4 projection;

                out vec4 vertexColor;

                void main()
                {
                    gl_Position = projection * vec4(position.x, position.y, 1.0, 1.0);
                    vertexColor = color;
                }
            ";

            var fragmentSource = @"
                #version 330 core
                out vec4 color;
  
                in vec4 vertexColor;

                void main()
                {
                    color = vertexColor;
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


        public static unsafe Shader SpriteShader()
        {
            var shader = new Shader
            {
                ID = glCreateProgram()
            };

            var vertexSource = @"
                #version 330 core
                layout (location = 0) in vec4 vertex; // <vec2 position, vec2 texCoords>

                out vec2 TexCoords;

                uniform mat4 model;
                uniform mat4 projection;

                void main()
                {
                    TexCoords = vertex.zw;
                    gl_Position = projection * model * vec4(vertex.xy, 0.0, 1.0);
                }
            ";

            var fragmentSource = @"
                #version 330 core
                in vec2 TexCoords;
                out vec4 color;

                uniform sampler2D image;
                uniform vec3 spriteColor;

                void main()
                {    
                    color = vec4(spriteColor, 1.0) * texture(image, TexCoords);
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
