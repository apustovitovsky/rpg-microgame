using UnityEngine;
using VContainer;

namespace RPG.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class ActorPickupReceiver : MonoBehaviour
    {
        private ActorHealth _actorHealth;

        [Inject]
        public void Construct(ActorHealth actorHealth)
        {
            _actorHealth = actorHealth;
        }

        public bool TryReceiveHealing(float amount)
        {
            return _actorHealth != null && _actorHealth.Heal(amount);
        }
    }
}
