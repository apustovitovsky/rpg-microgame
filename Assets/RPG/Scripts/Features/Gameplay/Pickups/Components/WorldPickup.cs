using UnityEngine;
using VContainer;
using System;


namespace RPG.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class WorldPickup : MonoBehaviour
    {
        private IPickupCollectionService _pickupService;
        private Func<PickupDefinitionSO, WorldPickup, IPickupInstance> _pickupFactory;
        private IPickupInstance _pickup;
        private Collider _collider;
        private bool _isAvailable = true;

        [field: SerializeField] public PickupDefinitionSO Definition { get; private set; }
        public bool IsCollected { get; private set; }

        public event Action<WorldPickup> Collected;

        [Inject]
        public void Construct(
            IPickupCollectionService pickupService,
            Func<PickupDefinitionSO, WorldPickup, IPickupInstance> pickupFactory)
        {
            _pickupService = pickupService;
            _pickupFactory = pickupFactory;
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
            InitializePickupInstance();
        }

        private void EnsureTrigger()
        {
            _collider = _collider != null ? _collider : GetComponent<Collider>();
            if (_collider != null) _collider.isTrigger = true;
        }

        public void Respawn()
        {
            _isAvailable = true;
            IsCollected = false;
            gameObject.SetActive(true);
            InitializePickupInstance();
            OnRespawned();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isAvailable || _pickupService == null || _pickup == null)
                return;

            if (!other.TryGetComponent<IPickupCollector>(out var collector))
                return;

            if (_pickupService.TryCollect(_pickup, collector))
            {
                _isAvailable = false;
                IsCollected = true;
                OnCollected();
                Collected?.Invoke(this);
            }
        }

        protected void Hide()
        {
            gameObject.SetActive(false);
        }

        protected virtual void OnCollected()
        {
            Destroy(gameObject);
        }

        protected virtual void OnRespawned()
        {
        }

        private void InitializePickupInstance()
        {
            _pickup = Definition != null
                ? _pickupFactory?.Invoke(Definition, this)
                : null;
        }
    }
}
