using System;
using Game.World;
using UnityEngine;
using VContainer;

namespace Game.Targeting
{
    [DisallowMultipleComponent]
    public sealed class Targetable :
        MonoBehaviour,
        ITargetable
    {
        [SerializeField] private Transform _uiAnchor;
        [SerializeField] private Transform _targetAnchor;
        [SerializeField] private bool _isTargetable = true;

        public Guid InstanceId { get; private set; }

        public string DisplayName { get; private set; }

        public Transform UiAnchor => _uiAnchor != null
            ? _uiAnchor
            : transform;

        public Transform TargetAnchor => _targetAnchor != null
            ? _targetAnchor
            : transform;

        public bool IsTargetable =>
            _isTargetable &&
            InstanceId != Guid.Empty;

        [Inject]
        public void Construct(WorldInstance instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            InstanceId = instance.InstanceId;
            DisplayName = instance.DisplayName;
        }
    }
}