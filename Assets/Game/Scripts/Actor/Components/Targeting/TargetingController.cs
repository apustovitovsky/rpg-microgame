using Game.Input;
using UnityEngine;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class TargetingController : MonoBehaviour
    {
        [SerializeField] private TargetingConfigSO _config;
        [SerializeField] private PerceptionController _perception;
        [SerializeField] private ActorLookController _look;

        [SerializeField] private float _distanceScoreWeight = 100f;
        [SerializeField] private float _angleScoreWeight = 40f;

        private IActorInput _input;

        public GameObject CurrentTarget { get; private set; }
        public bool IsLocked { get; private set; }

        public void Bind(IActorInput input)
        {
            if (_input != null)
            {
                _input.OnLockOnToggled -= ToggleLock;
            }

            _input = input;

            if (_input != null)
            {
                _input.OnLockOnToggled += ToggleLock;
            }
        }

        public void Unbind()
        {
            if (_input != null)
            {
                _input.OnLockOnToggled -= ToggleLock;
            }

            _input = null;
        }

        private void OnDisable()
        {
            Unlock();
        }

        private void Update()
        {
            var bestTarget = FindBestTarget();

            if (!IsLocked)
            {
                CurrentTarget = bestTarget;
                return;
            }

            if (bestTarget == null ||
                CurrentTarget == null ||
                !ContainsCurrentTarget())
            {
                Unlock();
                CurrentTarget = bestTarget;
                return;
            }

            ApplyTarget(CurrentTarget.transform);
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
            CurrentTarget = FindBestTarget();

            if (CurrentTarget == null)
            {
                Unlock();
                return;
            }

            IsLocked = true;
            ApplyTarget(CurrentTarget.transform);
        }

        public void Unlock()
        {
            IsLocked = false;
            _look?.ClearTarget();
        }

        private GameObject FindBestTarget()
        {
            if (_perception == null ||
                _perception.Candidates.Count == 0 ||
                _look == null)
            {
                return null;
            }

            GameObject bestTarget = null;
            float bestScore = float.NegativeInfinity;

            foreach (var candidate in _perception.Candidates)
            {
                if (candidate == null)
                {
                    continue;
                }

                float score = Evaluate(candidate);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = candidate;
                }
            }

            return bestTarget;
        }

        private float Evaluate(GameObject candidate)
        {
            float distance = Vector3.Distance(
                _perception.Origin.position,
                candidate.transform.position);

            float distanceScore = DistanceScoreWeight / Mathf.Max(distance, 0.01f);

            Vector3 targetDirection = candidate.transform.position - _look.Position;
            float angleScore = Vector3.Dot(targetDirection.normalized, _look.Forward) * AngleScoreWeight;

            return distanceScore + angleScore;
        }

        private bool ContainsCurrentTarget()
        {
            foreach (var candidate in _perception.Candidates)
            {
                if (candidate == CurrentTarget)
                {
                    return true;
                }
            }

            return false;
        }

        private float DistanceScoreWeight =>
            _config != null
                ? _config.DistanceScoreWeight
                : _distanceScoreWeight;

        private float AngleScoreWeight =>
            _config != null
                ? _config.AngleScoreWeight
                : _angleScoreWeight;

        private void ApplyTarget(Transform target)
        {
            _look.SetTarget(target);
        }
    }
}