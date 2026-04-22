using UnityEngine;
using UnityEngine.Serialization;

namespace RPG.Gameplay
{
    public sealed class WorldPickup : Pickup
    {
        [FormerlySerializedAs("VerticalBobFrequency")]
        [SerializeField, Tooltip("Frequency at which the item will move up and down")]
        private float _verticalBobFrequency = 1f;

        [FormerlySerializedAs("BobbingAmount")]
        [SerializeField, Tooltip("Distance the item will move up and down")]
        private float _bobbingAmount = 1f;

        [FormerlySerializedAs("RotatingSpeed")]
        [SerializeField, Tooltip("Rotation angle per second")]
        private float _rotatingSpeed = 360f;

        [FormerlySerializedAs("PickupSfx")]
        [SerializeField, Tooltip("Sound played on pickup")]
        private AudioClip _pickupSfx;

        [FormerlySerializedAs("PickupVfxPrefab")]
        [SerializeField, Tooltip("VFX spawned on pickup")]
        private GameObject _pickupVfxPrefab;

        [SerializeField, Tooltip("Optional transform used for bobbing/rotation. Defaults to this transform.")]
        private Transform _visualRoot;

        private Vector3 _startLocalPosition;
        private bool _hasPlayedFeedback;

        private void OnEnable()
        {
            CacheVisualRootPose();
        }

        private void Update()
        {
            var animatedRoot = _visualRoot != null ? _visualRoot : transform;
            float bobbingAnimationPhase = ((Mathf.Sin(Time.time * _verticalBobFrequency) * 0.5f) + 0.5f) * _bobbingAmount;

            animatedRoot.localPosition = _startLocalPosition + Vector3.up * bobbingAnimationPhase;
            animatedRoot.Rotate(Vector3.up, _rotatingSpeed * Time.deltaTime, Space.Self);
        }

        protected override void OnCollected()
        {
            PlayPickupFeedback();
            // Hide();
        }

        protected override void OnRespawned()
        {
            _hasPlayedFeedback = false;
            CacheVisualRootPose();
        }

        public void PlayPickupFeedback()
        {
            if (_hasPlayedFeedback)
                return;

            if (_pickupSfx != null)
            {
                AudioSource.PlayClipAtPoint(_pickupSfx, transform.position);
            }

            if (_pickupVfxPrefab != null)
            {
                Instantiate(_pickupVfxPrefab, transform.position, Quaternion.identity);
            }

            _hasPlayedFeedback = true;
        }

        private void CacheVisualRootPose()
        {
            var animatedRoot = _visualRoot != null ? _visualRoot : transform;
            _startLocalPosition = animatedRoot.localPosition;
        }
    }
}
