using ImageDotNet;
using LOTM.Client.Engine.Controls;
using LOTM.Client.Engine.Graphics;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Network;
using System;
using System.Runtime.InteropServices;
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

        protected OrthographicCamera Camera { get; }

        public GuiGame(int windowWidth, int windowHeight, string title, string iconPath, NetworkManager networkManager)
            : base(networkManager)
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

            if (!string.IsNullOrEmpty(iconPath))
            {
                try
                {
                    var image = Image.Load(iconPath);

                    using var data = image.GetDataPointer();
                    var img = new GLFWimage()
                    {
                        width = image.Width,
                        height = image.Height,
                        pixels = data.Pointer
                    };

                    IntPtr unmanagedAddr = Marshal.AllocHGlobal(Marshal.SizeOf(img));

                    Marshal.StructureToPtr(img, unmanagedAddr, true);

                    glfwSetWindowIcon(Window, 1, unmanagedAddr);
                }
                catch (Exception)
                {
                }
            }

            glInit(glfwGetProcAddress, VersionMajor, VersionMinor);
            glfwSetFramebufferSizeCallback(Window, Framebuffer_size_callback);

            //Input events
            glfwSetKeyCallback(Window, InputManager.KeyCallback);
            glfwSetJoystickCallback(InputManager.JoystickCallback);

            //OpenGL configuration
            glViewport(0, 0, WindowWidth, WindowHeight);
            glEnable(GL_BLEND);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

            //MSAA
            glEnable(GL_MULTISAMPLE);

            //Setup camera
            Camera = new OrthographicCamera(new OrthographicCamera.Viewport(Vector2.ZERO, new Vector2(WindowWidth, WindowHeight)));

            //Setup renderer
            Renderer = new Renderer2D(Camera);
        }

        public void Framebuffer_size_callback(IntPtr window, int width, int height)
        {
            glViewport(0, 0, width, height);
        }

        protected override void OnBeforeUpdate()
        {
            //Poll GLFW events
            if (glfwWindowShouldClose(Window) == GL_TRUE) Shutdown();

            glfwPollEvents();
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
