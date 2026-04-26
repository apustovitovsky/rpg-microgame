using UnityEngine;

namespace RPG.Gameplay
{
    public sealed class TargetDebugMarker : MonoBehaviour
    {
        [SerializeField] private Vector3 _offset = new(0f, 0.5f, 0f);
        [SerializeField] private GameObject _visual;

        [SerializeField] private float _verticalBobFrequency = 1f;
        [SerializeField] private float _bobbingAmount = 0.25f;
        [SerializeField] private float _rotatingSpeed = 360f;

        private Transform _targetAnchor;
        private Vector3 _visualStartLocalPosition;

        private void Awake()
        {
            if (_visual != null)
                _visualStartLocalPosition = _visual.transform.localPosition;

            UpdateVisibility();
        }

        public void SetTarget(Transform targetAnchor)
        {
            _targetAnchor = targetAnchor;
            UpdateVisibility();
        }

        public void ClearTarget()
        {
            _targetAnchor = null;
            UpdateVisibility();
        }

        private void LateUpdate()
        {
            if (_targetAnchor == null)
                return;

            transform.position = _targetAnchor.position + _offset;

            if (_visual == null)
                return;

            var bob = ((Mathf.Sin(Time.time * _verticalBobFrequency) * 0.5f) + 0.5f)
                      * _bobbingAmount;

            _visual.transform.localPosition = _visualStartLocalPosition + Vector3.up * bob;
            _visual.transform.Rotate(Vector3.up, _rotatingSpeed * Time.deltaTime, Space.Self);
        }

        private void UpdateVisibility()
        {
            if (_visual != null)
                _visual.SetActive(_targetAnchor != null);
        }
    }
}