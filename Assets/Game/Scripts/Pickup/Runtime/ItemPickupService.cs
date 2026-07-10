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
        private readonly ISpawnedObjectRegistry _spawnedObjects;

        public ItemPickupService(
            ISpawnedObjectRegistry spawnedObjects)
        {
            _spawnedObjects = spawnedObjects;
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
                    _spawnedObjects.Despawn(instanceId);

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