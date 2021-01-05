namespace LOTM.Client.Engine.Controls
{
    public class ButtonPressedEvent
    {
        public int Button { get; set; }

        public ButtonPressedEvent(int button)
        {
            Button = button;
        }
    }
}
