using System;
using System.Collections.Generic;
using Game.Inventory;

namespace Game.Loot
{
    public sealed class LootSessionService :
        ILootSessionService
    {
        private readonly Dictionary<Guid, LootSession> _sessions =
            new();

        private readonly Dictionary<Guid, Guid> _sessionIdsByLooter =
            new();

        private readonly IInventoryService _inventories;
        private readonly IInventoryTransferService _transfers;

        public LootSessionService(
            IInventoryService inventories,
            IInventoryTransferService transfers)
        {
            _inventories = inventories;
            _transfers = transfers;
        }

        public LootSessionOpenResult TryOpen(
            Guid looterInstanceId,
            Guid sourceInstanceId)
        {
            if (looterInstanceId == Guid.Empty ||
                sourceInstanceId == Guid.Empty ||
                looterInstanceId == sourceInstanceId)
            {
                return new LootSessionOpenResult(
                    LootSessionOpenStatus.InvalidRequest,
                    Guid.Empty);
            }

            if (_sessionIdsByLooter.TryGetValue(
                    looterInstanceId,
                    out var existingSessionId))
            {
                return new LootSessionOpenResult(
                    LootSessionOpenStatus.AlreadyOpen,
                    existingSessionId);
            }

            var session = new LootSession(
                looterInstanceId,
                sourceInstanceId);

            _sessions.Add(
                session.Id,
                session);

            _sessionIdsByLooter.Add(
                looterInstanceId,
                session.Id);

            return new LootSessionOpenResult(
                LootSessionOpenStatus.Opened,
                session.Id);
        }

        public LootTakeResult TryTake(
            Guid sessionId,
            Guid itemInstanceId,
            int requestedCount)
        {
            if (sessionId == Guid.Empty ||
                itemInstanceId == Guid.Empty ||
                requestedCount <= 0)
            {
                return LootTakeResult.InvalidRequest;
            }

            if (!TryGet(sessionId, out var session))
                return LootTakeResult.SessionNotFound;

            if (!TryGetInventories(
                    sessionId,
                    session,
                    out var sourceInventory,
                    out var looterInventory,
                    out var failureResult))
            {
                return failureResult;
            }

            var transferResult = _transfers.TryTransfer(
                sourceInventory,
                looterInventory,
                itemInstanceId,
                requestedCount);

            return ToLootTakeResult(transferResult);
        }

        public LootTakeResult TryTakeAll(Guid sessionId)
        {
            if (sessionId == Guid.Empty)
                return LootTakeResult.InvalidRequest;

            if (!TryGet(sessionId, out var session))
                return LootTakeResult.SessionNotFound;

            if (!TryGetInventories(
                    sessionId,
                    session,
                    out var sourceInventory,
                    out var looterInventory,
                    out var failureResult))
            {
                return failureResult;
            }

            if (sourceInventory.Entries.Count >
                looterInventory.Capacity -
                looterInventory.Entries.Count)
            {
                return LootTakeResult.DestinationFull;
            }

            var entries = sourceInventory.Entries;
            var itemInstanceIds = new Guid[entries.Count];
            var counts = new int[entries.Count];

            for (var index = 0; index < entries.Count; index++)
            {
                itemInstanceIds[index] =
                    entries[index].Instance.InstanceId;

                counts[index] = entries[index].Count;
            }

            for (var index = 0;
                 index < itemInstanceIds.Length;
                 index++)
            {
                var transferResult = _transfers.TryTransfer(
                    sourceInventory,
                    looterInventory,
                    itemInstanceIds[index],
                    counts[index]);

                var result = ToLootTakeResult(transferResult);

                if (result != LootTakeResult.Succeeded)
                    return result;
            }

            Close(sessionId);
            return LootTakeResult.Succeeded;
        }

        public bool TryGetSnapshot(
            Guid sessionId,
            out LootSessionSnapshot snapshot)
        {
            snapshot = default;

            if (!TryGet(sessionId, out var session))
                return false;

            if (!TryGetInventories(
                    sessionId,
                    session,
                    out var sourceInventory,
                    out _,
                    out _))
            {
                return false;
            }

            var entries = new LootEntrySnapshot[
                sourceInventory.Entries.Count];

            for (var index = 0;
                 index < sourceInventory.Entries.Count;
                 index++)
            {
                var entry = sourceInventory.Entries[index];

                entries[index] = new LootEntrySnapshot(
                    entry.Instance.InstanceId,
                    entry.Definition.Id,
                    entry.Count);
            }

            snapshot = new LootSessionSnapshot(
                sessionId,
                entries);

            return true;
        }

        public bool TryGet(
            Guid sessionId,
            out LootSession session)
        {
            return _sessions.TryGetValue(
                sessionId,
                out session);
        }

        public bool TryGetByLooter(
            Guid looterInstanceId,
            out LootSession session)
        {
            session = null;

            if (!_sessionIdsByLooter.TryGetValue(
                    looterInstanceId,
                    out var sessionId))
            {
                return false;
            }

            return _sessions.TryGetValue(
                sessionId,
                out session);
        }

        public bool Close(Guid sessionId)
        {
            if (!_sessions.Remove(
                    sessionId,
                    out var session))
            {
                return false;
            }

            _sessionIdsByLooter.Remove(
                session.LooterInstanceId);

            return true;
        }

        private bool TryGetInventories(
            Guid sessionId,
            LootSession session,
            out InventoryInstance sourceInventory,
            out InventoryInstance looterInventory,
            out LootTakeResult failureResult)
        {
            sourceInventory = null;
            looterInventory = null;

            if (!_inventories.TryGet(
                    session.SourceInstanceId,
                    out sourceInventory))
            {
                Close(sessionId);

                failureResult =
                    LootTakeResult.SourceInventoryUnavailable;

                return false;
            }

            if (!_inventories.TryGet(
                    session.LooterInstanceId,
                    out looterInventory))
            {
                Close(sessionId);

                failureResult =
                    LootTakeResult.LooterInventoryUnavailable;

                return false;
            }

            failureResult = LootTakeResult.Succeeded;
            return true;
        }

        private static LootTakeResult ToLootTakeResult(
            InventoryTransferResult result)
        {
            return result switch
            {
                InventoryTransferResult.Succeeded
                    => LootTakeResult.Succeeded,

                InventoryTransferResult.SourceStackNotFound
                    => LootTakeResult.SourceStackNotFound,

                InventoryTransferResult.InsufficientAmount
                    => LootTakeResult.InsufficientAmount,

                InventoryTransferResult.DestinationFull
                    => LootTakeResult.DestinationFull,

                InventoryTransferResult.InvalidRequest
                    => LootTakeResult.InvalidRequest,

                _ => throw new ArgumentOutOfRangeException(
                    nameof(result),
                    result,
                    "Unknown inventory transfer result.")
            };
        }
    }
}