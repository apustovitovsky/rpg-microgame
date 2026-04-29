using System;
using Etheria.Game.Targeting;


namespace Etheria.Features.Targeting
{
    public sealed class TargetingService : ITargetingService
    {
        public ITargetable CurrentTarget => _targetSelectionState.CurrentTarget;

        private readonly ITargetValidator _targetValidator;
        private readonly ITargetAcquisitionService _targetAcquisitionService;
        private readonly ITargetCycleService _targetCycleService;
        private readonly ITargetSelectionState _targetSelectionState;

        public TargetingService(
            ITargetAcquisitionService targetAcquisitionService,
            ITargetCycleService targetCycleService,
            ITargetValidator targetValidator,
            ITargetSelectionState targetSelectionState)
        {
            _targetAcquisitionService = targetAcquisitionService;
            _targetCycleService = targetCycleService;
            _targetValidator = targetValidator;
            _targetSelectionState = targetSelectionState;
        }


        public bool TryAcquireFromView()
        {
            if (!_targetAcquisitionService.TryAcquire(CurrentTarget, out var target))
                return false;

            if (ReferenceEquals(CurrentTarget, target))
            {
                ClearTarget();
                return true;
            }

            return TrySetTarget(target);
        }

        public bool TryCycleTarget(int direction)
        {
            if (!_targetCycleService.TryCycle(CurrentTarget, direction, out var target))
                return false;

            return TrySetTarget(target);
        }

        public bool TrySetTarget(ITargetable target)
        {
            if (!IsValid(target))
                return false;

            return _targetSelectionState.TrySet(target);
        }

        public bool IsValid(ITargetable target)
        {
            return _targetValidator.IsValid(target);
        }

        public void ClearTarget()
        {
            _targetSelectionState.Clear();
        }
    }
}
