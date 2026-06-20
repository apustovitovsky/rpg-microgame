using System;
using Etheria.Game.Actor;
using Etheria.Game.Targeting;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.Character
{
    public sealed class CharacterLabelPresenter :
        IStartable,
        IDisposable
    {
        private readonly IPlayerTargetProvider _targetProvider;
        private readonly CharacterLabelPool _pool;

        private CharacterLabelView _currentView;
        private Camera _camera;

        public CharacterLabelPresenter(
            IPlayerTargetProvider targetProvider,
            CharacterLabelPool pool)
        {
            _targetProvider = targetProvider;
            _pool = pool;
        }

        public void Start()
        {
            _camera = Camera.main;

            _targetProvider.TargetChanged += OnTargetChanged;
            OnTargetChanged(_targetProvider.CurrentTarget);
        }

        public void Dispose()
        {
            _targetProvider.TargetChanged -= OnTargetChanged;
            ReleaseCurrentView();
        }

        private void OnTargetChanged(Transform target)
        {
            ReleaseCurrentView();

            if (target == null || _camera == null)
                return;

            IActorIdentity identity =
                target.GetComponentInParent<IActorIdentity>();

            if (identity is not Component identityComponent)
                return;

            Transform uiAnchor =
                identityComponent.transform.Find("UiAnchor");

            if (uiAnchor == null)
                return;

            _currentView = _pool.Get(
                uiAnchor,
                identity.DisplayName,
                _camera);
        }

        private void ReleaseCurrentView()
        {
            if (_currentView == null)
                return;

            _pool.Release(_currentView);
            _currentView = null;
        }
    }
}