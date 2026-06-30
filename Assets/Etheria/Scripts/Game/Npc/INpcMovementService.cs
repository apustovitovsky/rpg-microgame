using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Etheria.Game.Npc
{
    public interface INpcMovementService
    {
        bool IsOnNavMesh { get; }

        UniTask MoveToAsync(
            Vector3 position,
            float radius,
            CancellationToken cancellationToken);

        UniTask MoveToCenterAsync(
            Vector3 position,
            CancellationToken cancellationToken);

        UniTask TurnToAsync(
            Quaternion rotation,
            CancellationToken cancellationToken);

        UniTask StopAsync(
            CancellationToken cancellationToken);

        void BeginManualRotation();

        void EndManualRotation();

        void FaceTowards(
            Vector3 worldPosition);
    }
}