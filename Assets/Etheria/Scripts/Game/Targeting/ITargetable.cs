using System;
using Etheria.Game.Actor;
using Etheria.Game.Common;
using UnityEngine;

namespace Etheria.Game.Targeting
{
    public interface ITargetable : IIdentifiable<Guid>
    {
        Transform Root { get; }
        Transform AimPoint { get; }
        Transform UiAnchor { get; }
        bool IsTargetable { get; }
    }
}

