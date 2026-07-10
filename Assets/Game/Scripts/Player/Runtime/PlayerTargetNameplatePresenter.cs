using System;
using Game.Actor;
using Game.Targeting;
using Game.UI;
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
        private readonly IActorService _actors;
        private readonly IDisplayNameService _displayNames;
        private IActorTargeting _targeting;
        private TargetNameplateView _currentView;
        private ITargetable _currentTarget;
        private Camera _camera;

        public PlayerTargetNameplatePresenter(
            IPlayerService player,
            TargetNameplatePool pool,
            IActorService actors,
            IDisplayNameService displayNames)
        {
            _player = player;
            _pool = pool;
            _actors = actors;
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
            UnbindTargeting();
            ReleaseCurrent();
        }

        private void OnCurrentActorChanged()
        {
            UnbindTargeting();
            ReleaseCurrent();

            var actorInstanceId = _player.CurrentActor;

            if (actorInstanceId == Guid.Empty ||
                !_actors.TryGet(
                    actorInstanceId,
                    out var actor) ||
                actor.Targeting == null)
            {
                return;
            }

            _targeting = actor.Targeting;
            _targeting.CurrentTargetChanged += OnCurrentTargetChanged;
            OnCurrentTargetChanged(_targeting.CurrentTarget);
        }

        private void UnbindTargeting()
        {
            if (_targeting != null)
                _targeting.CurrentTargetChanged -= OnCurrentTargetChanged;

            _targeting = null;
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