using System.Collections.Generic;
using Etheria.Game.Camera;
using UnityEngine;

namespace Etheria.Features.Targeting
{
    public sealed class FirstValidTargetSelector : ITargetSelector
    {
        private readonly ICameraTransformProvider _cameraProvider;

        public FirstValidTargetSelector(ICameraTransformProvider cameraProvider)
        {
            _cameraProvider = cameraProvider;
        }

        public ITargetable SelectTarget(
            IReadOnlyList<ITargetable> candidates,
            ITargetingService targetingService)
        {
            ITargetable bestTarget = null;
            var bestAngle = float.MaxValue;

            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (!targetingService.IsValid(candidate))
                    continue;

                var toTarget = candidate.AimPoint.position - _cameraProvider.Position;
                var angle = Vector3.Angle(_cameraProvider.Forward, toTarget);

                if (angle >= bestAngle)
                    continue;

                bestAngle = angle;
                bestTarget = candidate;
            }

            return bestTarget;
        }
    }
}
