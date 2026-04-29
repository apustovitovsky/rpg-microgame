using System;
using Etheria.Game.Targeting;
using UnityEngine;

namespace Etheria.Features.Targeting
{
    public sealed class TargetSelectionState : ITargetSelectionState
    {
        public ITargetable CurrentTarget { get; private set; }

        public event Action<ITargetable> TargetChanged;

        public bool TrySet(ITargetable target)
        {
            if (target == null)
                return false;

            if (ReferenceEquals(CurrentTarget, target))
                return true;

            CurrentTarget = target;
            Debug.Log($"Target acquired: {CurrentTarget.DisplayName}");
            TargetChanged?.Invoke(CurrentTarget);
            return true;
        }

        public void Clear()
        {
            if (CurrentTarget == null)
                return;

            Debug.Log($"TargetingService: clearing target '{CurrentTarget.DisplayName}'.");
            CurrentTarget = null;
            TargetChanged?.Invoke(null);
        }
    }
}
