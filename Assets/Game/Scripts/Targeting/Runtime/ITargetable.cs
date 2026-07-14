using System;
using UnityEngine;

namespace Game.Targeting
{
    public interface ITargetable
    {
        Guid InstanceId { get; }

        string DisplayName { get; }

        Transform TargetAnchor { get; }

        Transform UiAnchor { get; }

        bool IsTargetable { get; }
    }
}