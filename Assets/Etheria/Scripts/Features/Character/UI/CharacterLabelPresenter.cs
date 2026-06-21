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
        private readonly ITargetProvider _targetProvider;
        private readonly CharacterLabelPool _pool;

        private CharacterLabelView _currentView;
        private Camera _camera;

        public CharacterLabelPresenter(
            ITargetProvider targetProvider,
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

        private void OnTargetChanged(ITargetCandidate target)
        {
            ReleaseCurrentView();

            if (target == null ||
                target.UiAnchor == null ||
                _camera == null)
            {
                return;
            }

            _currentView = _pool.Get(
                target.UiAnchor,
                target.DisplayName,
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