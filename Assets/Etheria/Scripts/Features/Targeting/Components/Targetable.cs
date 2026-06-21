using System;
using UnityEngine;

namespace Etheria.Features.Targeting
{
    [DisallowMultipleComponent]
    public sealed class Targetable : MonoBehaviour, ITargetable
    {
        [SerializeField] private string _id;
        [SerializeField] private Transform _root;
        [SerializeField] private Transform _aimPoint;
        [SerializeField] private Transform _uiAnchor;
        [SerializeField] private bool _isTargetable = true;

        public Guid Id =>
            Guid.TryParse(_id, out var id)
                ? id
                : Guid.Empty;

        public Transform Root => _root != null ? _root : transform;
        public Transform AimPoint => _aimPoint;
        public Transform UiAnchor => _uiAnchor;
        public bool IsTargetable => _isTargetable && isActiveAndEnabled;

        private void Reset()
        {
            _root = transform;
            EnsureId();
        }

        private void OnValidate()
        {
            EnsureId();
        }

        private void EnsureId()
        {
            if (!Guid.TryParse(_id, out _))
                _id = Guid.NewGuid().ToString();
        }
    }
}
