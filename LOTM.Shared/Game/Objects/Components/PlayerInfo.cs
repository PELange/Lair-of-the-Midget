using LOTM.Shared.Engine.Objects.Components;

namespace LOTM.Shared.Game.Objects.Components
{
    public class PlayerInfo : IComponent
    {
        public string Name { get; set; }

        public PlayerInfo(string name)
        {
            Name = name;
        }
    }
}
