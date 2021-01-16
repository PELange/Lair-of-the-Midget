using LOTM.Shared.Engine.Objects.Components;

namespace LOTM.Shared.Game.Objects.Components
{
    public class Health : IComponent
    {
        public double MaxHealth { get; set; }
        public double CurrentHealth { get; set; }

        public Health(double maxHealth, double currentHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = currentHealth;
        }

        public void DeplateHealthAbsolute(double amount)
        {
            CurrentHealth = System.Math.Max(0, CurrentHealth - amount);
        }

        public void AddHealthAbsolute(double amount)
        {
            CurrentHealth = System.Math.Min(MaxHealth, CurrentHealth + amount);
        }
    }
}
