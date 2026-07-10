using System;
using UnityEngine;
using VContainer;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class ActorView :
        MonoBehaviour,
        IActorView
    {
        [SerializeField] private Transform _cameraPivot;

        public ActorInstance Instance { get; private set; }

        public Transform Root => transform;

        public Transform CameraPivot =>
            _cameraPivot != null
                ? _cameraPivot
                : Root;

        [Inject]
        public void Construct(ActorInstance instance)
        {
            Instance = instance
                ?? throw new ArgumentNullException(nameof(instance));
        }
    }
}