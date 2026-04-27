using VContainer.Unity;

namespace Etheria.Gameplay
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
            _targetingService.ValidateCurrentTarget();
        }
    }
}