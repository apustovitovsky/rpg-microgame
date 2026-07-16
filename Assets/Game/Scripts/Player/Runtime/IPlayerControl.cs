using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Targeting;
using UnityEngine;

namespace Game.Player
{
    public enum PossessionResult
    {
        Rejected = 0,
        Completed = 1,
    }

    public interface IPlayerControl
    {
        Guid ControlledInstanceId { get; }

        Vector3 ControlledPosition { get; }

        Vector3 InteractionOrigin { get; }

        ITargetable CurrentTarget { get; }

        event Action ControlledObjectChanged;

        event Action CurrentTargetChanged;

        UniTask<PossessionResult> PossessAsync(
            Guid targetInstanceId,
            CancellationToken cancellationToken);

        void Release();
    }
}