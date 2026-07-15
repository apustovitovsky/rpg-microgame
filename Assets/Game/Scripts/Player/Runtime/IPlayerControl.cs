using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Commands;
using Game.Targeting;
using UnityEngine;

namespace Game.Player
{
    public interface IPlayerControl
    {
        Guid ControlledInstanceId { get; }

        Vector3 ControlledPosition { get; }

        Vector3 InteractionOrigin { get; }

        ITargetable CurrentTarget { get; }

        event Action ControlledObjectChanged;

        event Action CurrentTargetChanged;

        UniTask<CommandResult> PossessAsync(
            Guid targetInstanceId,
            CancellationToken token);

        void Release();
    }
}