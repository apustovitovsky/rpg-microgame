using UnityEngine;

namespace RPG.Gameplay
{
    public sealed class ActorHealth
    {
        public float CurrentHealth { get; private set; }
        public float MaxHealth { get; }

        public ActorHealth(float maxHealth)
        {
            MaxHealth = Mathf.Max(1f, maxHealth);
            CurrentHealth = MaxHealth / 2f;
        }

        public bool CanReceiveHealing()
        {
            return CurrentHealth < MaxHealth;
        }

        public bool Heal(float amount)
        {
            if (amount <= 0f || !CanReceiveHealing())
                return false;

            CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0f, MaxHealth);
            return true;
        }
    }
}
