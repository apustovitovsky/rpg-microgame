using System;
using Etheria.Core.Helpers;
using Etheria.Game.Targeting;
using UnityEngine;

namespace Etheria.Features.Character
{
    public sealed class TargetCandidate :
    MonoBehaviour,
    ITargetCandidate
    {
        [SerializeField, ReadOnlyField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Transform _aimPoint;
        [SerializeField] private Transform _uiAnchor;

        public Guid Id =>
            Guid.TryParse(_id, out var id)
                ? id
                : Guid.Empty;

        public string DisplayName => _displayName;

        public Transform Root => transform;
        public Transform AimPoint => _aimPoint;
        public Transform UiAnchor => _uiAnchor;

        public bool IsTargetable => isActiveAndEnabled;

        private void OnValidate()
        {
            if (!Guid.TryParse(_id, out _))
                _id = Guid.NewGuid().ToString();
        }

        [ContextMenu("Regenerate Id")]
        private void RegenerateId()
        {
            _id = Guid.NewGuid().ToString();
        }
    }
}