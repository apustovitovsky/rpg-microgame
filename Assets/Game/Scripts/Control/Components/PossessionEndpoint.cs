using System;
using Game.Core;
using Game.Input;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Control
{
    [DisallowMultipleComponent]
    public sealed class PossessionEndpoint :
        MonoBehaviour,
        IPossessionEndpoint,
        IModuleInstaller
    {
        [SerializeField] private Transform _root;
        [SerializeField] private Transform _cameraPivot;

        private IControlInputBinder _inputBinder;

        public Transform Root =>
            _root != null ? _root : transform;

        public Transform CameraPivot =>
            _cameraPivot != null ? _cameraPivot : Root;

        public void Install(
            IContainerBuilder builder)
        {
            builder.RegisterComponent(this)
                .As<IPossessionEndpoint>();
        }

        [Inject]
        public void Construct(IControlInputBinder inputBinder)
        {
            _inputBinder = inputBinder
                ?? throw new ArgumentNullException(nameof(inputBinder));
        }

        public void BindInput(IControlInput input)
        {
            _inputBinder.Bind(input);
        }

        public void UnbindInput()
        {
            _inputBinder.Unbind();
        }
    }
}