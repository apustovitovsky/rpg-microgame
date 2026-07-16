using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Actor
{
    public interface IActorNavigation
    {
        bool HasArrived { get; }

        bool IsMoving { get; }

        bool IsFacingComplete { get; }

        UniTask MoveToPositionAsync(
            Vector3 destination,
            CancellationToken cancellationToken);

        UniTask MoveToAsync(
            Vector3 destination,
            float arrivalRadius,
            CancellationToken cancellationToken);

        UniTask MoveToCenterAsync(
            Vector3 destination,
            CancellationToken cancellationToken);

        UniTask FaceDirectionAsync(
            Vector3 direction,
            CancellationToken cancellationToken);

        void FaceDirection(Vector3 direction);

        void Stop();

        IDisposable AcquirePause();

        void ClearFacing();
    }
}