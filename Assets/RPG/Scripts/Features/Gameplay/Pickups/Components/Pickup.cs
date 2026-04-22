using UnityEngine;
using VContainer;
using System;

namespace RPG.Gameplay
{
    [RequireComponent(typeof(Collider))]
    [DisallowMultipleComponent]
    public class Pickup : MonoBehaviour
    {
        private IPickupCollectionService _pickupService;
        private IPickupInstance _instance;
        private Collider _collider;
        public bool IsAvailable { get; private set; }
        public bool IsCollected { get; private set; }
        public event Action<Pickup> Collected;

        [Inject]
        public void Construct(IPickupCollectionService pickupService)
        {
            _pickupService = pickupService;
        }

        public void Initialize(PickupDefinitionSO definition)
        {
            _instance = new PickupInstance(definition);
            IsAvailable = true;
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

        public void Respawn()
        {
            IsAvailable = true;
            IsCollected = false;
            OnRespawned();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsAvailable || _pickupService == null || _instance == null)
                return;

            var collector = other.GetComponent<IPickupCollector>() ?? other.GetComponentInParent<IPickupCollector>();
            if (collector == null)
                return;

            if (_pickupService.TryCollect(_instance, collector))
            {
                IsAvailable = false;
                IsCollected = true;
                OnCollected();
                Collected?.Invoke(this);
            }
        }

        protected virtual void OnCollected()
        {
            Destroy(gameObject);
        }

        protected virtual void OnRespawned()
        {
        }
    }
}
