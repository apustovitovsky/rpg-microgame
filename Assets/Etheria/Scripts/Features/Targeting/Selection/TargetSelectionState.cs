using System;
using Etheria.Game.Targeting;
using UnityEngine;

namespace Etheria.Features.Targeting
{
    public sealed class TargetSelectionState : ITargetSelectionState
    {
        public ITargetable CurrentTarget { get; private set; }

        public event Action<ITargetable> TargetChanged;

        public TargetSelectionResult Set(ITargetable target)
        {
            if (target == null)
                return TargetSelectionResult.None;

            if (ReferenceEquals(CurrentTarget, target))
                return new TargetSelectionResult(TargetSelectionStatus.AlreadySelected, target);

            CurrentTarget = target;
            Debug.Log($"Target acquired: {CurrentTarget.DisplayName}");
            TargetChanged?.Invoke(CurrentTarget);
            return new TargetSelectionResult(TargetSelectionStatus.Selected, CurrentTarget);
        }

        public TargetSelectionResult Clear()
        {
            if (CurrentTarget == null)
                return TargetSelectionResult.None;

            var previousTarget = CurrentTarget;
            Debug.Log($"TargetingService: clearing target '{CurrentTarget.DisplayName}'.");
            CurrentTarget = null;
            TargetChanged?.Invoke(null);
            return new TargetSelectionResult(TargetSelectionStatus.Cleared, previousTarget);
        }
    }
}
