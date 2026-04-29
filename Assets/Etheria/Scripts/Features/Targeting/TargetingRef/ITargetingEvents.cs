using System;

namespace Etheria.Features.Targeting
{
    public interface ITargetingEvents
    {
        event Action<ITargetable> TargetChanged;
    }
}