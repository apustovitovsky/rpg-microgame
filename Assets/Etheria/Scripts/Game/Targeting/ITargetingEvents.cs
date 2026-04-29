using System;
using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public interface ITargetingEvents
    {
        ITargetable CurrentTarget { get; }
        event Action<ITargetable> TargetChanged;
    }

}