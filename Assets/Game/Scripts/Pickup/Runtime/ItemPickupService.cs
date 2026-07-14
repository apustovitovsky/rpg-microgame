using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Pickup
{
    public sealed class ItemPickupService : IItemPickupService
    {
        private readonly HashSet<Guid> _collecting = new();
        private readonly IWorldSpawner _worldSpawner;

        public ItemPickupService(IWorldSpawner worldSpawner)
        {
            _worldSpawner = worldSpawner
                ?? throw new ArgumentNullException(nameof(worldSpawner));
        }

        public async UniTask<CollectResult> CollectAsync(
            Guid collectorInstanceId,
            ICollectable collectable,
            CancellationToken token)
        {
            if (collectorInstanceId == Guid.Empty)
                return CollectResult.InvalidCollector;

            if (collectable == null ||
                collectable.InstanceId == Guid.Empty)
            {
                return CollectResult.InvalidCollectable;
            }

            var instanceId = collectable.InstanceId;

            if (!_collecting.Add(instanceId))
                return CollectResult.AlreadyInProgress;

            try
            {
                if (!collectable.CanCollect(collectorInstanceId))
                    return CollectResult.CannotCollect;

                var result = await collectable.CollectAsync(
                    collectorInstanceId,
                    token);

                if (result == CollectResult.Succeeded)
                    _worldSpawner.Despawn(instanceId);

                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return CollectResult.Failed;
            }
            finally
            {
                _collecting.Remove(instanceId);
            }
        }
    }
}