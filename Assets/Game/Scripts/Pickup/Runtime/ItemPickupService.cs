using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Pickup
{
    public sealed class ItemPickupService : IItemPickupService
    {
        private readonly HashSet<WorldId> _collecting = new();
        private readonly IWorldObjectRegistry _world;

        public ItemPickupService(IWorldObjectRegistry world)
        {
            _world = world;
        }

        public async UniTask<CollectResult> CollectAsync(
            WorldId collectorId,
            ICollectable collectable,
            CancellationToken token)
        {
            if (collectorId.IsEmpty)
                return CollectResult.InvalidCollector;

            if (collectable == null || collectable.WorldId.IsEmpty)
                return CollectResult.InvalidCollectable;

            var worldId = collectable.WorldId;

            if (!_collecting.Add(worldId))
                return CollectResult.AlreadyInProgress;

            try
            {
                if (!collectable.CanCollect(collectorId))
                    return CollectResult.CannotCollect;

                var result = await collectable.CollectAsync(
                    collectorId,
                    token);

                if (result == CollectResult.Succeeded)
                    _world.Despawn(worldId);

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
                _collecting.Remove(worldId);
            }
        }
    }
}