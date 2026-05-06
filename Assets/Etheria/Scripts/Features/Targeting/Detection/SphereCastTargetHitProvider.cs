using UnityEngine;

namespace Etheria.Features.Targeting
{
    public interface ITargetHitProvider
    {
        int GetHits(RaycastHit[] buffer);
    }

    public sealed class SphereCastTargetHitProvider : ITargetHitProvider
    {
        private readonly IViewRayProvider _viewRayProvider;
        private readonly TargetingSettingsSO _settings;

        public SphereCastTargetHitProvider(
            IViewRayProvider viewRayProvider,
            TargetingSettingsSO settings)
        {
            _viewRayProvider = viewRayProvider;
            _settings = settings;
        }

        public int GetHits(RaycastHit[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
                return 0;

            var ray = _viewRayProvider.GetRay();

            return Physics.SphereCastNonAlloc(
                ray,
                _settings.Radius,
                buffer,
                _settings.MaxDistance,
                _settings.TargetingMask,
                QueryTriggerInteraction.Ignore);
        }
    }
}