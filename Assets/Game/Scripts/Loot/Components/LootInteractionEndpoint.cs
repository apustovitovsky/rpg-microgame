using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using Game.Interaction;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Loot
{
    [DisallowMultipleComponent]
    public sealed class LootInteractionEndpoint :
        MonoBehaviour,
        IInteractionTarget,
        IModuleInstaller
    {
        [SerializeField] private Transform _interactionAnchor;

        [field: SerializeField]
        public float MaxRange { get; private set; } = 5f;

        private IInstanceIdentity _identity;
        private ILootSessionService _sessions;

        public Vector3 InteractionPoint =>
            _interactionAnchor != null
                ? _interactionAnchor.position
                : transform.position;

        public void Install(
            IContainerBuilder builder)
        {
            builder.RegisterComponent(this)
                .AsSelf()
                .As<IInteractionTarget>();

            builder.RegisterBinding<IInteractionTarget>();
        }

        [Inject]
        public void Construct(
            IInstanceIdentity identity,
            ILootSessionService sessions)
        {
            _identity = identity
                ?? throw new ArgumentNullException(nameof(identity));

            _sessions = sessions
                ?? throw new ArgumentNullException(nameof(sessions));
        }

        public bool CanInteract(InteractionContext context)
        {
            return _identity != null &&
                   _sessions != null &&
                   context.InteractorInstanceId != Guid.Empty &&
                   context.TargetInstanceId ==
                   _identity.InstanceId &&
                   context.InteractorInstanceId !=
                   _identity.InstanceId;
        }

        public UniTask InteractAsync(
            InteractionContext context,
            CancellationToken token)
        {
            if (!CanInteract(context))
                return UniTask.CompletedTask;

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
                    $"{openResult.Status}.",
                    this);

                return UniTask.CompletedTask;
            }

            if (!_sessions.TryGet(
                    openResult.SessionId,
                    out var session) ||
                session.SourceInstanceId !=
                _identity.InstanceId)
            {
                Debug.LogWarning(
                    "Loot session is already open for another source.",
                    this);

                return UniTask.CompletedTask;
            }

            if (!_sessions.TryGetSnapshot(
                    openResult.SessionId,
                    out var snapshot))
            {
                Debug.LogWarning(
                    "Loot session snapshot is unavailable.",
                    this);

                return UniTask.CompletedTask;
            }

            var takeResult = _sessions.TryTakeAll(
                openResult.SessionId);

            if (takeResult != LootTakeResult.Succeeded)
            {
                Debug.LogWarning(
                    $"Loot session '{openResult.SessionId:N}' " +
                    $"was not completed: {takeResult}.",
                    this);

                return UniTask.CompletedTask;
            }

            LogCollected(snapshot);
            return UniTask.CompletedTask;
        }

        private void LogCollected(
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

            Debug.Log(message.ToString(), this);
        }
    }
}