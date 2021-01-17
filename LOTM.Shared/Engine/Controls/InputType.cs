namespace LOTM.Shared.Engine.Controls
{
    [System.Flags]
    public enum InputType
    {
        NONE = 0x00,
        WALK_UP = 0x01,
        WALK_DOWN = 0x02,
        WALK_LEFT = 0x04,
        WALK_RIGHT = 0x08,
        ATTACK = 0x10,
    }
}
