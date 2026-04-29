using System;
using Etheria.Game.Targeting;
using VContainer.Unity;

namespace Etheria.Features.Targeting
{
    public sealed class TargetingTracker : IStartable, ITickable, IDisposable
    {
        private readonly ITargetingService _targetingService;
        private readonly IControlledTargetProvider _controlledTargetProvider;

        public TargetingTracker(
            ITargetingService targetingService,
            IControlledTargetProvider controlledTargetProvider)
        {
            _targetingService = targetingService;
            _controlledTargetProvider = controlledTargetProvider;
        }

        public void Start()
        {
            _controlledTargetProvider.ControlledTargetChanged += OnControlledTargetChanged;
        }

        public void Dispose()
        {
            _controlledTargetProvider.ControlledTargetChanged -= OnControlledTargetChanged;
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

        private void OnControlledTargetChanged(ITargetable controlledTarget)
        {
            if (controlledTarget != null)
                return;

            _targetingService.ClearTarget();
        }
    }
}
