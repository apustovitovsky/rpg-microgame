
using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public sealed class ControlledTargetProvider : IControlledTargetProvider
    {
        public ITargetable ControlledTarget { get; private set; }

        public void SetTarget(ITargetable self)
        {
            ControlledTarget = self;
        }

        public void ClearTarget()
        {
            ControlledTarget = null;
        }
    }
}
