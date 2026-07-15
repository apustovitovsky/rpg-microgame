using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using Game.Interaction;
using UnityEngine;

namespace Game.Loot
{
    public sealed class LootInteraction :
        IInteractable
    {
        private readonly IInstanceIdentity _identity;
        private readonly ILootSessionService _sessions;
        private readonly LootInteractionSettings _settings;

        public LootInteraction(
            IInstanceIdentity identity,
            ILootSessionService sessions,
            LootInteractionSettings settings)
        {
            _identity = identity
                ?? throw new ArgumentNullException(nameof(identity));

            _sessions = sessions
                ?? throw new ArgumentNullException(nameof(sessions));

            _settings = settings;
        }

        public Vector3 InteractionPoint =>
            _settings.InteractionAnchor.position;

        public float MaxRange =>
            _settings.MaxRange;

        public bool CanInteract(
            InteractionContext context)
        {
            return context.InteractorInstanceId != Guid.Empty &&
                   context.TargetInstanceId == _identity.InstanceId &&
                   context.InteractorInstanceId !=
                   _identity.InstanceId;
        }

        public UniTask<InteractionResult> InteractAsync(
            InteractionContext context,
            CancellationToken token)
        {
            if (!CanInteract(context))
            {
                return UniTask.FromResult(
                    InteractionResult.Rejected);
            }

            token.ThrowIfCancellationRequested();

            var openResult = _sessions.TryOpen(
                context.InteractorInstanceId,
                _identity.InstanceId);

            if (!openResult.Succeeded &&
                openResult.Status !=
                LootSessionOpenStatus.AlreadyOpen)
            {
                Debug.LogWarning(
                    $"Loot session was not opened: " +
                    $"{openResult.Status}.");

                return UniTask.FromResult(
                    InteractionResult.Rejected);
            }

            if (!_sessions.TryGet(
                    openResult.SessionId,
                    out var session) ||
                session.SourceInstanceId !=
                _identity.InstanceId)
            {
                Debug.LogWarning(
                    "Loot session is already open for another source.");

                return UniTask.FromResult(
                    InteractionResult.Rejected);
            }

            if (!_sessions.TryGetSnapshot(
                    openResult.SessionId,
                    out var snapshot))
            {
                Debug.LogWarning(
                    "Loot session snapshot is unavailable.");

                return UniTask.FromResult(
                    InteractionResult.Rejected);
            }

            var takeResult = _sessions.TryTakeAll(
                openResult.SessionId);

            if (takeResult != LootTakeResult.Succeeded)
            {
                Debug.LogWarning(
                    $"Loot session '{openResult.SessionId:N}' " +
                    $"was not completed: {takeResult}.");

                return UniTask.FromResult(
                    InteractionResult.Rejected);
            }

            LogCollected(snapshot);

            return UniTask.FromResult(
                InteractionResult.Completed);
        }

        private static void LogCollected(
            LootSessionSnapshot snapshot)
        {
            var message = new StringBuilder();

            message.Append("Loot collected:");

            if (snapshot.Entries.Count == 0)
            {
                message.Append("\n- no items");
            }
            else
            {
                foreach (var entry in snapshot.Entries)
                {
                    message.Append("\n- ")
                        .Append(entry.ItemDefinitionId)
                        .Append(" x")
                        .Append(entry.Count)
                        .Append(" (")
                        .Append(entry.ItemInstanceId.ToString("N"))
                        .Append(')');
                }
            }

            Debug.Log(message.ToString());
        }
    }

    public readonly struct LootInteractionSettings
    {
        public LootInteractionSettings(
            Transform interactionAnchor,
            float maxRange)
        {
            InteractionAnchor = interactionAnchor;
            MaxRange = maxRange;
        }

        public Transform InteractionAnchor { get; }

        public float MaxRange { get; }
    }
}