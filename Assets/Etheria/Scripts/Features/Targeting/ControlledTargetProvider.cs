using System;
using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public sealed class ControlledTargetProvider : IControlledTargetProvider
    {
        public ITargetable ControlledTarget { get; private set; }

        public event Action<ITargetable> ControlledTargetChanged;

        public void SetTarget(ITargetable target)
        {
            if (ReferenceEquals(ControlledTarget, target))
                return;

            ControlledTarget = target;
            ControlledTargetChanged?.Invoke(ControlledTarget);
        }

        public void ClearTarget()
        {
            if (ControlledTarget == null)
                return;

            ControlledTarget = null;
            ControlledTargetChanged?.Invoke(null);
        }
    }
}
