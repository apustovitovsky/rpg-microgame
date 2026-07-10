using System;
using Game.Actor;
using Game.Targeting;
using Game.UI;
using Game.Core;
using UnityEngine;
using VContainer.Unity;

namespace Game.Player
{
    public sealed class PlayerTargetNameplatePresenter :
        IStartable,
        IDisposable
    {
        private readonly IPlayerService _player;
        private readonly TargetNameplatePool _pool;
        private readonly IInstanceRegistry<ITargetProvider> _targetProviders;
        private readonly IDisplayNameService _displayNames;
        private ITargetProvider _targetProvider;
        private TargetNameplateView _currentView;
        private ITargetable _currentTarget;
        private Camera _camera;

        public PlayerTargetNameplatePresenter(
            IPlayerService player,
            TargetNameplatePool pool,
            IInstanceRegistry<ITargetProvider> targetProviders,
            IDisplayNameService displayNames)
        {
            _player = player;
            _pool = pool;
            _targetProviders = targetProviders;
            _displayNames = displayNames;
        }

        public void Start()
        {
            _player.CurrentActorChanged += OnCurrentActorChanged;
            OnCurrentActorChanged();
        }

        public void Dispose()
        {
            _player.CurrentActorChanged -= OnCurrentActorChanged;
            UnbindTargetProvider();
            ReleaseCurrent();
        }

        private void OnCurrentActorChanged()
        {
            UnbindTargetProvider();
            ReleaseCurrent();

            var actorInstanceId = _player.CurrentActor;

            if (actorInstanceId == Guid.Empty ||
                !_targetProviders.TryGet(
                    actorInstanceId,
                    out _targetProvider))
            {
                return;
            }

            _targetProvider.CurrentTargetChanged += OnCurrentTargetChanged;
            OnCurrentTargetChanged(_targetProvider.CurrentTarget);
        }

        private void UnbindTargetProvider()
        {
            if (_targetProvider != null)
                _targetProvider.CurrentTargetChanged -= OnCurrentTargetChanged;

            _targetProvider = null;
        }

        private void OnCurrentTargetChanged(ITargetable target)
        {
            if (target == null ||
                !target.IsTargetable ||
                target.InstanceId == Guid.Empty)
            {
                ReleaseCurrent();
                return;
            }

            var currentActorId = _player.CurrentActor;

            if (target.InstanceId == currentActorId)
            {
                ReleaseCurrent();
                return;
            }

            if (ReferenceEquals(_currentTarget, target))
                return;

            if (!_displayNames.TryGet(
                    target.InstanceId,
                    out var displayName))
            {
                ReleaseCurrent();
                return;
            }

            var camera = ResolveCamera();

            if (camera == null)
            {
                ReleaseCurrent();
                return;
            }

            ReleaseCurrent();

            _currentTarget = target;
            _currentView = _pool.Get(
                target.UiAnchor,
                displayName,
                camera);
        }

        private Camera ResolveCamera()
        {
            if (_camera != null)
                return _camera;

            _camera = Camera.main;
            return _camera;
        }

        private void ReleaseCurrent()
        {
            if (_currentView != null)
                _pool.Release(_currentView);

            _currentView = null;
            _currentTarget = null;
        }
    }
}