using LOTM.Shared.Engine.Objects.Components;

namespace LOTM.Shared.Game.Objects.Components
{
    class Health : IComponent
    {
        public double Value { get; set; }

        public Health(double value)
        {
            Value = value;
        }
    }
}
