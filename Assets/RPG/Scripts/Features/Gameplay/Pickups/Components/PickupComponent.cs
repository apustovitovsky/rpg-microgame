using UnityEngine;
using VContainer;
using System;


namespace RPG.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class PickupComponent : MonoBehaviour
    {
        private IPickupService _pickupService;
        private Func<PickupDefinitionSO, IPickupInstance> _pickupFactory;
        private IPickupInstance _pickup;
        private PickupDefinitionSO _pickupDefinition;
        private Collider _collider;

        [Inject]
        public void Construct(
            IPickupService pickupService,
            Func<PickupDefinitionSO, IPickupInstance> pickupFactory,
            PickupDefinitionSO pickupDefinition)
        {
            _pickupService = pickupService;
            _pickupFactory = pickupFactory;
            _pickupDefinition = pickupDefinition;
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
            _pickup = _pickupFactory?.Invoke(_pickupDefinition);
        }

        private void EnsureTrigger()
        {
            _collider = _collider != null ? _collider : GetComponent<Collider>();
            if (_collider != null) _collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_pickupService == null || _pickup == null)
                return;

            if (!other.TryGetComponent<IPickupCollector>(out var collector))
                return;

            if (_pickupService.TryCollect(_pickup, collector))
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
