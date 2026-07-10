using System;
using Game.Input;
using UnityEngine;
using VContainer;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class ActorPossessable :
        MonoBehaviour,
        IPossessable
    {
        [SerializeField] private Transform _cameraPivot;

        private IActorInputBinder _inputBinder;

        public Transform CameraPivot =>
            _cameraPivot != null ? _cameraPivot : transform;

        [Inject]
        public void Construct(IActorInputBinder inputBinder)
        {
            _inputBinder = inputBinder
                ?? throw new ArgumentNullException(nameof(inputBinder));
        }

        public void BindInput(IActorInput input)
        {
            _inputBinder.Bind(input);
        }

        public void UnbindInput()
        {
            _inputBinder.Unbind();
        }
    }
}