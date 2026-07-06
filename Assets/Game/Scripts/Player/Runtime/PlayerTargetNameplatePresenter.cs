using System;
using Game.Actor;
using Game.Targeting;
using UnityEngine;
using VContainer.Unity;

namespace Game.Player
{
    public sealed class PlayerTargetNameplatePresenter :
        IStartable,
        IDisposable
    {
        private readonly IPlayerService _player;
        private readonly ActorNameplatePool _pool;

        private ITargetProvider _targetProvider;
        private ActorNameplateView _currentView;
        private ITargetable _currentTarget;
        private Camera _camera;

        public PlayerTargetNameplatePresenter(
            IPlayerService player,
            ActorNameplatePool pool)
        {
            _player = player;
            _pool = pool;
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

            var actor = _player.CurrentActor;

            if (actor == null)
                return;

            _targetProvider = actor.TargetProvider;

            if (_targetProvider == null)
                return;

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
                target.TargetPoint == null ||
                string.IsNullOrWhiteSpace(target.TargetId))
            {
                ReleaseCurrent();
                return;
            }

            var currentActor = _player.CurrentActor;

            if (currentActor != null &&
                target.TargetId == currentActor.InstanceId)
            {
                ReleaseCurrent();
                return;
            }

            if (ReferenceEquals(_currentTarget, target))
                return;

            var camera = ResolveCamera();

            if (camera == null)
            {
                ReleaseCurrent();
                return;
            }

            ReleaseCurrent();

            _currentTarget = target;
            _currentView = _pool.Get(
                target.TargetPoint,
                target.TargetId,
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