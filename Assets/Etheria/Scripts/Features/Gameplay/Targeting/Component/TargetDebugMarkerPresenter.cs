using System;
using VContainer.Unity;

namespace Etheria.Features.Gameplay
{
    public sealed class TargetDebugMarkerPresenter : IStartable, IDisposable
    {
        private readonly ITargetingService _targetingService;
        private readonly TargetDebugMarker _targetDebugMarker;

        public TargetDebugMarkerPresenter(
            ITargetingService targetingService,
            TargetDebugMarker targetDebugMarker)
        {
            _targetingService = targetingService;
            _targetDebugMarker = targetDebugMarker;
        }

        public void Start()
        {
            _targetingService.TargetChanged += OnTargetChanged;
            OnTargetChanged(_targetingService.CurrentTarget);
        }

        public void Dispose()
        {
            _targetingService.TargetChanged -= OnTargetChanged;
        }

        private void OnTargetChanged(ITargetable target)
        {
            if (target == null || target.UiAnchor == null)
            {
                _targetDebugMarker.ClearTarget();
                return;
            }

            _targetDebugMarker.SetTarget(target.UiAnchor);
        }
    }
}

