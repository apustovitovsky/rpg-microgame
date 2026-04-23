using UnityEngine;
using VContainer;
using System;

namespace RPG.Gameplay
{
    [RequireComponent(typeof(Collider))]
    [DisallowMultipleComponent]
    public class Pickup : MonoBehaviour
    {
        public bool IsCollected { get; private set; }
        protected IPickupInstance _instance;
        private IPickupCollectionService _pickupService;
        private Collider _collider;

        [Inject]
        public void Construct(IPickupCollectionService pickupService)
        {
            _pickupService = pickupService;
        }

        public virtual void Initialize(IPickupInstance instance)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            IsCollected = true;
            Respawn();
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
            if (_instance == null)
                return;

            IsCollected = false;
            OnRespawn();
        }

        protected virtual void OnRespawn() { }

        private void OnTriggerEnter(Collider other)
        {
            if (IsCollected || _pickupService == null || _instance == null)
                return;

            var collector = other.GetComponent<IPickupCollector>() ?? other.GetComponentInParent<IPickupCollector>();
            if (collector == null)
                return;
                
            if (_pickupService.TryCollect(_instance, collector))
            {
                IsCollected = true;
                OnCollect();
            }
        }

        protected virtual void OnCollect() { }

        private void Reset()
        {
            EnsureTrigger();
        }

        private void OnValidate()
        {
            EnsureTrigger();
        }
    }
}
