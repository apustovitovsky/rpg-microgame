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
            var result = _targetAcquisitionService.Acquire(CurrentTarget);

            if (result.Status == TargetAcquireStatus.None)
                return false;

            if (result.Status == TargetAcquireStatus.CurrentTarget)
            {
                ClearTarget();
                return true;
            }

            return TrySetTarget(result.Target);
        }

        public bool TryCycleTarget(int direction)
        {
            var result = _targetCycleService.Cycle(CurrentTarget, direction);
            if (result.Status == TargetCycleStatus.None)
                return false;

            return TrySetTarget(result.Target);
        }

        public bool TrySetTarget(ITargetable target)
        {
            if (!IsValid(target))
                return false;

            var result = _targetSelectionState.Set(target);
            return result.Status == TargetSelectionStatus.Selected ||
                   result.Status == TargetSelectionStatus.AlreadySelected;
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
