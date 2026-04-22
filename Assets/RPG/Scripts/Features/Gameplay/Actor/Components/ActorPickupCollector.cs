using UnityEngine;
using VContainer;

namespace RPG.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class ActorPickupCollector : MonoBehaviour, IPickupCollector
    {
        private IHealth _actorHealth;
        private IInventory _actorInventory;
        
        public string Name => name;
        public IHealth Health => _actorHealth;
        public IInventory Inventory => _actorInventory;

        [Inject]
        public void Construct(
            IHealth actorHealth,
            IInventory actorInventory)
        {
            _actorHealth = actorHealth;
            _actorInventory = actorInventory;
        }
    }
}
