using UnityEngine;
using VContainer;

namespace RPG.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class PickupCollector : MonoBehaviour, IPickupTarget
    {
        private IHealth _actorHealth;
        private IInventory _actorInventory;

        public string DisplayName => name;

        [Inject]
        public void Construct(
            IHealth actorHealth,
            IInventory actorInventory)
        {
            _actorHealth = actorHealth;
            _actorInventory = actorInventory;
        }

        public bool TryGet<T>(out T service) where T : class
        {
            if (_actorHealth is T healthService)
            {
                service = healthService;
                return true;
            }

            if (_actorInventory is T inventoryService)
            {
                service = inventoryService;
                return true;
            }

            service = null;
            return false;
        }
    }
}
