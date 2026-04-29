using System;

namespace Etheria.Game.Targeting
{
    public interface IControlledTargetProvider
    {
        ITargetable ControlledTarget { get; }
        event Action<ITargetable> ControlledTargetChanged;

        void SetTarget(ITargetable target);
        void ClearTarget();
    }
}

