using System;
using System.Collections.Generic;
using Game.World;
using UnityEngine;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class ActorView :
        MonoBehaviour,
        IActorView,
        IWorldCapability
    {
        [SerializeField] private Transform _cameraPivot;
        [SerializeField] private Transform _targetPoint;
        [SerializeField] private Transform _uiAnchor;

        public Transform Root =>
            transform;

        public Transform TargetPoint => _targetPoint != null
            ? _targetPoint
            : Root;

        public Transform CameraPivot => _cameraPivot != null
            ? _cameraPivot
            : Root;

        public Transform UiAnchor => _uiAnchor != null
            ? _uiAnchor
            : Root;

        public IEnumerable<Type> PublishedTypes
        {
            get { yield return typeof(IActorView); }
        }
    }
}