using LOTM.Shared.Engine.Controls;
using System;
using static GLFWDotNet.GLFW;

namespace LOTM.Client.Engine.Controls
{
    public class InputManager
    {
        protected static bool[] KeyStates { get; set; } = new bool[GLFW_KEY_LAST + 1];

        public static GLFWkeyfun KeyCallback { get; set; } = Key_callback;
        public static GLFWjoystickfun JoystickCallback { get; set; } = Joystick_callback;

        protected static void Key_callback(IntPtr window, int key, int scancode, int action, int mods)
        {
            if (key != GLFW_KEY_UNKNOWN)
            {
                KeyStates[key] = action != GLFW_RELEASE;
            }
        }

        protected static void Joystick_callback(int jid, int @event)
        {
        }

        public static bool IsControlPressed(InputType controlType)
        {
            if (controlType == InputType.WALK_UP && KeyStates[GLFW_KEY_W]) return true;
            if (controlType == InputType.WALK_DOWN && KeyStates[GLFW_KEY_S]) return true;
            if (controlType == InputType.WALK_LEFT && KeyStates[GLFW_KEY_A]) return true;
            if (controlType == InputType.WALK_RIGHT && KeyStates[GLFW_KEY_D]) return true;

            return false;
        }
    }
}
