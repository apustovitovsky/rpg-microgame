using System;

namespace Game.Loot
{
    public interface ILootSessionService
    {
        LootSessionOpenResult TryOpen(
            Guid looterInstanceId,
            Guid sourceInstanceId);

        LootTakeResult TryTake(
            Guid sessionId,
            Guid itemInstanceId,
            int requestedCount);

        LootTakeResult TryTakeAll(Guid sessionId);

        bool TryGetSnapshot(
            Guid sessionId,
            out LootSessionSnapshot snapshot);

        bool TryGet(
            Guid sessionId,
            out LootSession session);

        bool TryGetByLooter(
            Guid looterInstanceId,
            out LootSession session);

        bool Close(Guid sessionId);
    }
}