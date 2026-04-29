using Etheria.Game.Targeting;
using VContainer.Unity;

namespace Etheria.Features.Targeting
{
    public sealed class TargetingTracker : ITickable
    {
        private readonly ITargetingService _targetingService;

        public TargetingTracker(ITargetingService targetingService)
        {
            _targetingService = targetingService;
        }

        public void Tick()
        {
            var currentTarget = _targetingService.CurrentTarget;
            if (currentTarget == null)
                return;

            if (_targetingService.IsValid(currentTarget))
                return;

            _targetingService.ClearTarget();
        }
    }
}
