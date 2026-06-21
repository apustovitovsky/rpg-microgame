using System;
using System.Collections.Generic;
using Etheria.Game.Input;
using Etheria.Game.Targeting;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.Character
{
    public sealed class CharacterTargetingService :
    ITargetProvider,
    IStartable,
    ITickable,
    IDisposable
    {
        private readonly ITargetCandidateSource _candidateSource;
        private readonly IPlayerInputSource _input;
        private readonly PlayerCameraLookController _cameraLook;

        public ITargetCandidate CurrentTarget { get; private set; }
        public bool IsLocked { get; private set; }

        public event Action<ITargetCandidate> TargetChanged;
        public event Action<bool> LockChanged;

        public CharacterTargetingService(
            ITargetCandidateSource candidateSource,
            IPlayerInputSource input,
            PlayerCameraLookController cameraLook)
        {
            _candidateSource = candidateSource;
            _input = input;
            _cameraLook = cameraLook;
        }

        public void Start()
        {
            _input.OnLockOnToggled += ToggleLock;
        }

        public void Dispose()
        {
            _input.OnLockOnToggled -= ToggleLock;
        }

        public void Tick()
        {
            UpdateBestTarget();
        }

        private void ToggleLock()
        {
            if (!IsLocked && CurrentTarget == null)
                return;

            SetLocked(!IsLocked);
        }

        private static bool IsValid(ITargetCandidate candidate)
        {
            return candidate != null
                && candidate.IsTargetable
                && candidate.AimPoint != null;
        }

        private void SetTarget(ITargetCandidate target)
        {
            if (ReferenceEquals(CurrentTarget, target))
                return;

            CurrentTarget = target;
            TargetChanged?.Invoke(target);
        }

        private void UpdateBestTarget()
        {
            ITargetCandidate newBestTarget;

            var candidates = _candidateSource.Candidates;

            if (candidates.Count == 0)
            {
                newBestTarget = null;
            }
            else if (candidates.Count == 1)
            {
                newBestTarget = GetOnlyCandidate(candidates);
            }
            else
            {
                newBestTarget = null;
                float bestTargetScore = 0f;

                foreach (ITargetCandidate target in candidates)
                {
                    if (!IsValid(target))
                        continue;

                    float distance = Vector3.Distance(
                        _candidateSource.Origin.position,
                        target.Root.position);

                    float distanceScore =
                        1f / Mathf.Max(distance, 0.01f) * 100f;

                    Vector3 targetDirection =
                        target.AimPoint.position -
                        _cameraLook.GetCameraPosition();

                    float angleInView = Vector3.Dot(
                        targetDirection.normalized,
                        _cameraLook.GetCameraForward());

                    float angleScore = angleInView * 40f;
                    float totalScore = distanceScore + angleScore;

                    if (totalScore > bestTargetScore)
                    {
                        bestTargetScore = totalScore;
                        newBestTarget = target;
                    }
                }
            }

            if (!IsLocked)
            {
                SetTarget(newBestTarget);
            }
            else
            {
                if (Contains(candidates, CurrentTarget))
                {
                    // Сохраняем зафиксированную цель.
                }
                else
                {
                    SetTarget(newBestTarget);
                    SetLocked(false);
                }
            }
        }

        private static ITargetCandidate GetOnlyCandidate(
            IReadOnlyCollection<ITargetCandidate> candidates)
        {
            foreach (var candidate in candidates)
                return candidate;

            return null;
        }

        private static bool Contains(
            IReadOnlyCollection<ITargetCandidate> candidates,
            ITargetCandidate target)
        {
            foreach (var candidate in candidates)
            {
                if (ReferenceEquals(candidate, target))
                    return true;
            }

            return false;
        }

        private void SetLocked(bool value)
        {
            if (IsLocked == value)
                return;

            IsLocked = value;
            LockChanged?.Invoke(value);
        }
    }
}