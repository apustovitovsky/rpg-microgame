using UnityEngine;
using VContainer;
using System;

namespace RPG.Gameplay
{
    [RequireComponent(typeof(Collider))]
    [DisallowMultipleComponent]
    public class Pickup : MonoBehaviour
    {
        public event Action<Pickup> Collected;

        public bool IsCollected { get; private set; }
        protected PickupInstance _instance;
        private IPickupInteractionHandler _handler;
        private Collider _collider;
        private Action<Pickup> _release;

        [Inject]
        public void Construct(IPickupInteractionHandler handler)
        {
            _handler = handler
                ?? throw new ArgumentNullException(nameof(handler));
        }

        public void SetInstance(PickupInstance instance)
        {
            _instance = instance
                ?? throw new ArgumentNullException(nameof(instance));

            IsCollected = true;
            Respawn();
        }

        public void Respawn()
        {
            if (_instance == null)
                return;

            IsCollected = false;
            OnRespawn();
        }

        public void SetRelease(Action<Pickup> release)
        {
            _release = release;
        }

        public virtual void PrepareForRelease()
        {
            IsCollected = true;
            _instance = null;
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

        protected virtual void OnRespawn() { }

        private void OnTriggerEnter(Collider other)
        {
            if (IsCollected || _handler == null || _instance == null)
                return;

            var collector = other.GetComponent<IPickupTarget>() ?? other.GetComponentInParent<IPickupTarget>();
            if (collector == null)
                return;

            if (_handler.TryCollect(_instance, collector))
            {
                IsCollected = true;
                Collected?.Invoke(this);
                OnCollect();
            }
        }

        protected virtual void OnCollect()
        {
            if (_release != null)
            {
                _release(this);
                return;
            }

            Destroy(gameObject);
            return;
        }

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
