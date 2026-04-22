using UnityEngine;
using VContainer;


namespace RPG.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class PickupComponent : MonoBehaviour
    {
        private IPickupService _pickupService;
        private IPickup _pickup;
        private Collider _collider;

        [Inject]
        public void Construct(
            IPickupService pickupService,
            IPickup pickup)
        {
            _pickupService = pickupService;
            _pickup = pickup;
        }

        private void Reset()
        {
            EnsureTrigger();
        }

        private void OnValidate()
        {
            EnsureTrigger();
        }

        private void Awake()
        {
            EnsureTrigger();
        }

        private void EnsureTrigger()
        {
            _collider = _collider != null ? _collider : GetComponent<Collider>();
            if (_collider != null) _collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_pickupService.TryCollect(_pickup, other))
            {
                OnCollected();
                Destroy(gameObject);
            }
        }

        protected virtual void OnCollected()
        {
        }
    }
}
