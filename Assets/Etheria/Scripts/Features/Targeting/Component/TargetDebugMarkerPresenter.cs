using System;
using Etheria.Game.Targeting;
using VContainer.Unity;

namespace Etheria.Features.Targeting
{
    public sealed class TargetDebugMarkerPresenter : IStartable, IDisposable
    {
        private readonly ITargetingEvents _targetingEvents;
        private readonly TargetDebugMarker _targetDebugMarker;

        public TargetDebugMarkerPresenter(
            ITargetingEvents targetingEvents,
            TargetDebugMarker targetDebugMarker)
        {
            _targetingEvents = targetingEvents;
            _targetDebugMarker = targetDebugMarker;
        }

        public void Start()
        {
            _targetingEvents.TargetChanged += OnTargetChanged;
            OnTargetChanged(_targetingEvents.CurrentTarget);
        }

        public void Dispose()
        {
            _targetingEvents.TargetChanged -= OnTargetChanged;
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

