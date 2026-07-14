using System;
using Game.Core;
using Game.World;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Targeting
{
    [DisallowMultipleComponent]
    public sealed class TargetableEndpoint :
        MonoBehaviour,
        ITargetable,
        IPrefabInstaller
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

        public void Install(
            IContainerBuilder builder)
        {
            builder.RegisterComponent(this)
                .AsSelf()
                .As<ITargetable>();
        }

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