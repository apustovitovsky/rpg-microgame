using System;
using UnityEngine;

namespace Etheria.Features.Targeting
{
    public interface ITargetable
    {
        Guid Id { get; }
        Transform Root { get; }
        Transform AimPoint { get; }
        Transform UiAnchor { get; }
        bool IsTargetable { get; }
    }
}

