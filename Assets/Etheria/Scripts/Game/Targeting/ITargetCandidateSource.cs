using System;
using System.Collections.Generic;
using Etheria.Game.Common;
using UnityEngine;

namespace Etheria.Game.Targeting
{
    public interface ITargetCandidate : IIdentifiable<Guid>
    {
        string DisplayName { get; }

        Transform Root { get; }
        Transform AimPoint { get; }
        Transform UiAnchor { get; }

        bool IsTargetable { get; }
    }

    public interface ITargetCandidateSource
    {
        Transform Origin { get; }
        IReadOnlyCollection<ITargetCandidate> Candidates { get; }
    }
}