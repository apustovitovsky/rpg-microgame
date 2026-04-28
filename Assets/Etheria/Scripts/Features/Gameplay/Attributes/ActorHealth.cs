using UnityEngine;

namespace Etheria.Features.Gameplay
{
    public sealed class ActorHealth : IHealth
    {
        public float CurHealth { get; private set; }
        public float MaxHealth { get; }

        // public bool IsFull => CurHealth < MaxHealth;
        public bool IsFull => false;

        public ActorHealth(float curHealth, float maxHealth)
        {
            MaxHealth = Mathf.Max(1f, maxHealth);
            CurHealth = Mathf.Clamp(curHealth, 0f, maxHealth);
        }

        public void Heal(float amount)
        {
            CurHealth = Mathf.Clamp(CurHealth + amount, 0f, MaxHealth);
        }
    }
}

