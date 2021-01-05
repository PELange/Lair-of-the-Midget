using LOTM.Shared.Engine.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static GLFWDotNet.GLFW;

namespace LOTM.Client.Engine.Controls
{
    public class InputManager
    {
        protected static bool[] KeyStates { get; set; } = new bool[GLFW_KEY_LAST + 1];
        public static GLFWkeyfun KeyCallback { get; set; } = Key_callback;
        public static GLFWjoystickfun JoystickCallback { get; set; } = Joystick_callback;

        public static ConcurrentBag<ButtonPressedEvent> ButtonPressedEvents { get; set; } = new ConcurrentBag<ButtonPressedEvent>();

        protected static void Key_callback(IntPtr window, int key, int scancode, int action, int mods)
        {
            if (key != GLFW_KEY_UNKNOWN)
            {
                var newPressedState = action != GLFW_RELEASE;

                if (newPressedState != KeyStates[key])
                {
                    ButtonPressedEvents.Add(new ButtonPressedEvent(key));
                }

                KeyStates[key] = newPressedState;
            }
        }

        protected static void Joystick_callback(int jid, int @event)
        {
        }

        public static bool WasControlPressed(InputType controlType)
        {
            if (controlType == InputType.WALK_UP && (KeyStates[GLFW_KEY_W] || ButtonPressedEvents.Where(x => x.Button == GLFW_KEY_W).Any())) return true;
            if (controlType == InputType.WALK_DOWN && (KeyStates[GLFW_KEY_S] || ButtonPressedEvents.Where(x => x.Button == GLFW_KEY_S).Any())) return true;
            if (controlType == InputType.WALK_LEFT && (KeyStates[GLFW_KEY_A] || ButtonPressedEvents.Where(x => x.Button == GLFW_KEY_A).Any())) return true;
            if (controlType == InputType.WALK_RIGHT && (KeyStates[GLFW_KEY_D] || ButtonPressedEvents.Where(x => x.Button == GLFW_KEY_D).Any())) return true;

            return false;
        }

        public static void ClearEvents()
        {
            ButtonPressedEvents.Clear();
        }
    }
}
