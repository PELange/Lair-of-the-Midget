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

        public bool AddHealthTotal(double amount)
        {
            if (CurrentHealth >= 1) return false;

            CurrentHealth = System.Math.Min(MaxHealth, CurrentHealth + amount);

            return true;
        }

        /// <summary>
        /// Adds health based on max hp
        /// </summary>
        /// <param name="amount">Percentage between 0.0 and 1.0</param>
        /// <returns></returns>
        public bool AddHealthPercentage(double amount)
        {
            return AddHealthTotal(MaxHealth * amount);
        }


        public bool DepleteHealthTotal(double amount)
        {
            if (CurrentHealth <= 0) return false;

            CurrentHealth = System.Math.Max(0, CurrentHealth - amount);

            return true;
        }


        /// <summary>
        /// Adds health based on max hp
        /// </summary>
        /// <param name="amount">Percentage between 0.0 and 1.0</param>
        /// <returns></returns>
        public bool DepleteHealthPercentage(double amount)
        {
            return DepleteHealthTotal(MaxHealth * amount);
        }

        public bool IsDead()
        {
            return CurrentHealth <= 0;
        }
    }
}
