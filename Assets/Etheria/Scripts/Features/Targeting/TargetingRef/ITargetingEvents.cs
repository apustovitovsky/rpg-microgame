using System;
using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public interface ITargetingEvents
    {
        event Action<ITargetable> TargetChanged;
    }
}