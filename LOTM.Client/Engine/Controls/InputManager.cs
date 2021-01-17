using LOTM.Shared.Engine.Controls;
using LOTM.Shared.Game.Network.Packets;
using System;
using System.Collections.Concurrent;
using System.Linq;
using static GLFWDotNet.GLFW;

namespace LOTM.Client.Engine.Controls
{
    public class InputManager
    {
        protected bool[] KeyStates { get; }
        public GLFWkeyfun KeyCallback { get; }
        public GLFWjoystickfun JoystickCallback { get; }

        protected ConcurrentBag<ButtonPressedEvent> ButtonPressedEvents { get; }

        protected PlayerInput LastSentInput { get; set; }
        protected bool InputsSynced { get; set; }

        public DateTime LastSyncAttempt { get; set; }

        public InputManager()
        {
            KeyStates = new bool[GLFW_KEY_LAST + 1];
            KeyCallback = Key_callback;
            JoystickCallback = Joystick_callback;
            ButtonPressedEvents = new ConcurrentBag<ButtonPressedEvent>();
        }

        protected void Key_callback(IntPtr window, int key, int scancode, int action, int mods)
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

        protected void Joystick_callback(int jid, int @event)
        {
        }

        protected bool WasControlPressed(InputType controlType)
        {
            if (controlType == InputType.WALK_UP && (KeyStates[GLFW_KEY_W] || ButtonPressedEvents.Where(x => x.Button == GLFW_KEY_W).Any())) return true;
            if (controlType == InputType.WALK_DOWN && (KeyStates[GLFW_KEY_S] || ButtonPressedEvents.Where(x => x.Button == GLFW_KEY_S).Any())) return true;
            if (controlType == InputType.WALK_LEFT && (KeyStates[GLFW_KEY_A] || ButtonPressedEvents.Where(x => x.Button == GLFW_KEY_A).Any())) return true;
            if (controlType == InputType.WALK_RIGHT && (KeyStates[GLFW_KEY_D] || ButtonPressedEvents.Where(x => x.Button == GLFW_KEY_D).Any())) return true;

            if (controlType == InputType.ATTACK && (KeyStates[GLFW_KEY_SPACE] || ButtonPressedEvents.Where(x => x.Button == GLFW_KEY_SPACE).Any())) return true;

            return false;
        }

        public void OnPlayerInputAck(PlayerInputAck packet)
        {
            if (LastSentInput != null && LastSentInput.Id == packet.AckPacketId)
            {
                InputsSynced = true;
            }
        }

        public bool UpdateControls(out PlayerInput playerInput)
        {
            playerInput = default;

            var inputs = InputType.NONE;

            //Exclusive left right
            if (WasControlPressed(InputType.WALK_LEFT) && !WasControlPressed(InputType.WALK_RIGHT))
            {
                inputs |= InputType.WALK_LEFT;
            }
            else if (WasControlPressed(InputType.WALK_RIGHT) && !WasControlPressed(InputType.WALK_LEFT))
            {
                inputs |= InputType.WALK_RIGHT;
            }

            //Exclusive up down
            if (WasControlPressed(InputType.WALK_UP) && !WasControlPressed(InputType.WALK_DOWN))
            {
                inputs |= InputType.WALK_UP;
            }
            else if (WasControlPressed(InputType.WALK_DOWN) && !WasControlPressed(InputType.WALK_UP))
            {
                inputs |= InputType.WALK_DOWN;
            }

            if (WasControlPressed(InputType.ATTACK))
            {
                inputs |= InputType.ATTACK;
            }

            //Clear all events such as button presses, as we processed them all for this frame.
            ButtonPressedEvents.Clear();

            //Regardless of ack state, we have something new to sync as the last sent state differs from now most up to date state
            if (LastSentInput == null || inputs != LastSentInput.Inputs)
            {
                InputsSynced = false;

                playerInput = new PlayerInput
                {
                    Inputs = inputs
                };

                LastSentInput = playerInput;

                return true;
            }
            else if (!InputsSynced && (LastSyncAttempt == null || (DateTime.Now - LastSyncAttempt).TotalMilliseconds > 250)) //Our last player input was not yet acknowledged by the server, so resend the packet
            {
                LastSyncAttempt = DateTime.Now;

                playerInput = LastSentInput;

                return true;
            }

            return false;
        }
    }
}
