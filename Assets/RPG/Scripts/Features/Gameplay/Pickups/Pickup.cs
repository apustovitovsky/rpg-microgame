using UnityEngine;

namespace RPG.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public abstract class Pickup : MonoBehaviour
    {
        [SerializeField] private bool destroyOnPickup = true;

        private bool _isCollected;

        protected virtual void Reset()
        {
            var pickupCollider = GetComponent<Collider>();
            pickupCollider.isTrigger = true;
        }

        protected virtual void Awake()
        {
            var pickupCollider = GetComponent<Collider>();
            pickupCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isCollected)
                return;

            if (!TryCollect(other))
                return;

            _isCollected = true;
            OnCollected();

            if (destroyOnPickup)
                Destroy(gameObject);
        }

        protected abstract bool TryCollect(Collider other);

        protected virtual void OnCollected()
        {
        }
    }
}
