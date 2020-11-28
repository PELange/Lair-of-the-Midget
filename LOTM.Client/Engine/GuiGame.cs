using LOTM.Client.Engine.Graphics;
using LOTM.Shared.Engine.Math;
using System;
using static GLDotNet.GL;
using static GLFWDotNet.GLFW;

namespace LOTM.Client.Engine
{
    public abstract class GuiGame : Shared.Engine.Game
    {
        private int WindowWidth { get; }

        private int WindowHeight { get; }

        private IntPtr Window { get; set; }

        private Renderer2D Renderer { get; set; }

        public GuiGame(int windowWidth, int windowHeight, string title)
        {
            WindowWidth = windowWidth;
            WindowHeight = windowHeight;

            //glfw init
            const int VersionMajor = 3;
            const int VersionMinor = 3;

            if (glfwInit() == 0) return;

            glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, VersionMajor);
            glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, VersionMinor);
            glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
            glfwWindowHint(GLFW_RESIZABLE, 0);

            Window = glfwCreateWindow(WindowWidth, WindowHeight, title, IntPtr.Zero, IntPtr.Zero);

            if (Window == IntPtr.Zero)
            {
                Shutdown();
                return;
            }

            glfwMakeContextCurrent(Window);

            glInit(glfwGetProcAddress, VersionMajor, VersionMinor);
            glfwSetFramebufferSizeCallback(Window, Framebuffer_size_callback);

            // OpenGL configuration
            // --------------------
            glViewport(0, 0, WindowWidth, WindowHeight);
            glEnable(GL_BLEND);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

            //Setup renderer
            Renderer = new Renderer2D(new OrthographicCamera(new OrthographicCamera.Viewport(Vector2.ZERO, new Vector2(WindowWidth, WindowHeight))));
        }

        protected void Framebuffer_size_callback(IntPtr window, int width, int height)
        {
            glViewport(0, 0, width, height);
        }

        protected override void OnBeforeUpdate()
        {
            glfwPollEvents();

            if (glfwWindowShouldClose(Window) == GL_TRUE) Shutdown();
        }

        protected override void OnAfterUpdate()
        {
            glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

            Render();

            glfwSwapBuffers(Window);
        }

        protected override void OnShutdown()
        {
            glfwTerminate();
        }

        private void Render()
        {
            Renderer.Render(World);
        }
    }
}
