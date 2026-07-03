using System;
using Game.Actor;
using Game.Targeting;
using UnityEngine;
using VContainer.Unity;

namespace Game.Possession
{
    public sealed class PossessedActorTargetLabelPresenter :
        IStartable,
        IDisposable
    {
        private readonly IPossessionService _possession;
        private readonly ActorNameplatePool _pool;

        private TargetingController _targeting;
        private Targetable _self;
        private ActorNameplateView _currentView;
        private ITargetable _currentTarget;
        private Camera _camera;

        public PossessedActorTargetLabelPresenter(
            IPossessionService possession,
            ActorNameplatePool pool)
        {
            _possession = possession;
            _pool = pool;
        }

        public void Start()
        {
            _possession.CurrentActorChanged += OnCurrentActorChanged;
            OnCurrentActorChanged();
        }

        public void Dispose()
        {
            _possession.CurrentActorChanged -= OnCurrentActorChanged;
            UnbindTargeting();
            ReleaseCurrent();
        }

        private void OnCurrentActorChanged()
        {
            UnbindTargeting();
            ReleaseCurrent();

            var actor = _possession.CurrentActor;

            if (actor == null)
                return;

            actor.TryGet(out _targeting);
            actor.TryGet(out _self);

            if (_targeting == null)
                return;

            _targeting.CurrentTargetChanged += OnCurrentTargetChanged;
            OnCurrentTargetChanged(_targeting.CurrentTarget);
        }

        private void UnbindTargeting()
        {
            if (_targeting != null)
                _targeting.CurrentTargetChanged -= OnCurrentTargetChanged;

            _targeting = null;
            _self = null;
        }

        private void OnCurrentTargetChanged(ITargetable target)
        {
            if (ReferenceEquals(target, _self) ||
                target == null ||
                !target.IsTargetable ||
                target.TargetPoint == null ||
                string.IsNullOrWhiteSpace(target.TargetId))
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