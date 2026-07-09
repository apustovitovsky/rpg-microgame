using System;
using Game.Input;
using Game.Targeting;

using UnityEngine;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class ActorTargetController :
        MonoBehaviour,
        ITargetProvider
    {
        [SerializeField] private ActorTargetSensor _perception;
        [SerializeField] private ActorLookController _look;

        [SerializeField] private float _distanceScoreWeight = 0f;
        [SerializeField] private float _angleScoreWeight = 100f;

        private IActorInput _input;
        private ITargetSelector _selector;

        public ITargetable CurrentTarget { get; private set; }
        public bool IsLocked { get; private set; }

        public event Action<ITargetable> CurrentTargetChanged;

        private void Awake()
        {
            _selector = new TargetSelector(
                new ITargetFilter[]
                {
                    new TargetableFilter()
                },
                new ITargetScorer[]
                {
                    new DistanceTargetScorer(_distanceScoreWeight),
                    new AngleTargetScorer(_angleScoreWeight)
                });
        }

        public void Bind(IActorInput input)
        {
            if (_input != null)
                _input.OnLockOnToggled -= ToggleLock;

            _input = input;

            if (_input != null)
                _input.OnLockOnToggled += ToggleLock;
        }

        public void Unbind()
        {
            if (_input != null)
                _input.OnLockOnToggled -= ToggleLock;

            _input = null;
        }

        private void OnDisable()
        {
            Unlock();
            SetCurrentTarget(null);
        }

        private void Update()
        {
            var bestTarget = FindBestTarget();

            if (!IsLocked)
            {
                SetCurrentTarget(bestTarget);
                return;
            }

            if (bestTarget == null ||
                CurrentTarget == null ||
                !ContainsCurrentTarget())
            {
                Unlock();
                SetCurrentTarget(bestTarget);
                return;
            }

            ApplyTarget(CurrentTarget);
        }

        public void ToggleLock()
        {
            if (IsLocked)
            {
                Unlock();
                return;
            }

            LockBestTarget();
        }

        public void LockBestTarget()
        {
            SetCurrentTarget(FindBestTarget());

            if (CurrentTarget == null)
            {
                Unlock();
                return;
            }

            IsLocked = true;
            ApplyTarget(CurrentTarget);
        }

        public void Unlock()
        {
            IsLocked = false;

            if (_look == null)
                return;

            _look.ClearTarget();
        }

        private ITargetable FindBestTarget()
        {
            if (_perception == null ||
                _perception.Candidates.Count == 0 ||
                _look == null)
            {
                return null;
            }

            return _selector.SelectBest(
                _perception.Candidates,
                _look.Position,
                _look.Forward);
        }

        private bool ContainsCurrentTarget()
        {
            foreach (var candidate in _perception.Candidates)
            {
                if (ReferenceEquals(candidate, CurrentTarget))
                    return true;
            }

            return false;
        }

        private void ApplyTarget(ITargetable target)
        {
            if (target == null ||
                target.TargetPoint == null)
            {
                Unlock();
                return;
            }

            _look.SetTarget(target.TargetPoint);
        }

        private void SetCurrentTarget(ITargetable target)
        {
            if (ReferenceEquals(CurrentTarget, target))
                return;

            CurrentTarget = target;
            CurrentTargetChanged?.Invoke(CurrentTarget);
        }
    }
}