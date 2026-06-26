using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Game.Interaction;
using Etheria.Game.Npc;
using UnityEngine;
using VContainer;

namespace Etheria.Npc
{
    public sealed class NpcAgent :
        MonoBehaviour,
        INpcAgent,
        IInteractable
    {
        [SerializeField] private NpcDefinitionSO _definition;

        private INpcAgentRegistryWriter _registry;
        private NpcInteractionService _interaction;
        private NpcTaskScheduler _scheduler;
        private NpcMotor _motor;
        private bool _isRegistered;

        private Quaternion _homeRotation;
        public Quaternion HomeRotation => _homeRotation;

        private void Awake()
        {
            _homeRotation = transform.rotation;
        }

        public string NpcId =>
            _definition != null
                ? _definition.NpcId
                : string.Empty;

        public string CharacterId => NpcId;

        public Transform Transform => transform;

        public bool IsBusy =>
            _scheduler != null &&
            _scheduler.IsBusy;

        public bool CanInteract =>
            _interaction != null &&
            _interaction.CanInteract(NpcId);

        [Inject]
        public void Construct(
            INpcAgentRegistryWriter registry,
            NpcTaskScheduler scheduler,
            NpcInteractionService interaction,
            NpcMotor motor)
        {
            _registry = registry;
            _scheduler = scheduler;
            _interaction = interaction;
            _motor = motor;

            TryRegister();
        }

        public void Interact()
        {
            if (_interaction == null ||
                _scheduler == null ||
                _motor == null)
            {
                return;
            }

            if (!_interaction.Interact(NpcId))
                return;

            var interlocutor = _interaction.Interlocutor;
            if (interlocutor == null)
                return;

            _interaction.SetCompletedCallback(
                () => _scheduler.CancelCurrentTask());

            _scheduler.Enqueue(
                new DialogueTask(
                    _motor,
                    interlocutor,
                    NpcTaskPriorities.Dialogue));
        }

        private void OnEnable()
        {
            TryRegister();
        }

        private void OnDisable()
        {
            _interaction?.ClearCompletedCallback();
            Unregister();
        }

        private void TryRegister()
        {
            if (_isRegistered ||
                _registry == null ||
                !isActiveAndEnabled)
            {
                return;
            }

            _registry.Register(this);
            _isRegistered = true;
        }

        private void Unregister()
        {
            if (!_isRegistered ||
                _registry == null)
            {
                return;
            }

            _registry.Unregister(this);
            _isRegistered = false;
        }

        public bool TryMoveTo(
            Transform destination,
            Action<bool> completed = null)
        {
            if (_scheduler == null ||
                _motor == null ||
                destination == null)
            {
                completed?.Invoke(false);
                return false;
            }

            var task = new MovementTask(
                _motor,
                destination,
                NpcTaskPriorities.Command);

            _scheduler.Enqueue(task);

            WatchTaskAsync(
                    task,
                    completed)
                .Forget();

            return true;
        }

        public bool TryFollowRoute(
            IReadOnlyList<Transform> route,
            Action<bool> completed = null)
        {
            if (_scheduler == null ||
                _motor == null ||
                route == null ||
                route.Count == 0)
            {
                completed?.Invoke(false);
                return false;
            }

            FollowRouteAsync(
                    route,
                    completed)
                .Forget();

            return true;
        }

        public void TeleportTo(
            Vector3 position,
            Quaternion rotation)
        {
            CancelAllTasks();

            if (_motor != null)
            {
                _motor.TeleportTo(position, rotation);
                return;
            }

            transform.SetPositionAndRotation(
                position,
                rotation);
        }

        private async UniTaskVoid FollowRouteAsync(
            IReadOnlyList<Transform> route,
            Action<bool> completed)
        {
            for (int i = 0; i < route.Count; i++)
            {
                var destination = route[i];

                if (destination == null)
                {
                    completed?.Invoke(false);
                    return;
                }

                var task = new MovementTask(
                    _motor,
                    destination,
                    NpcTaskPriorities.Command);

                _scheduler.Enqueue(task);

                await WaitTaskAsync(task);

                if (task.Status != NpcTaskStatus.Completed)
                {
                    completed?.Invoke(false);
                    return;
                }
            }

            completed?.Invoke(true);
        }

        private async UniTaskVoid WatchTaskAsync(
            INpcTask task,
            Action<bool> completed)
        {
            await WaitTaskAsync(task);

            completed?.Invoke(
                task.Status == NpcTaskStatus.Completed);
        }

        private async UniTask WaitTaskAsync(
            INpcTask task)
        {
            await UniTask.WaitUntil(
                () => task.Status == NpcTaskStatus.Completed ||
                      task.Status == NpcTaskStatus.Failed ||
                      task.Status == NpcTaskStatus.Cancelled,
                cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        public void CancelAllTasks()
        {
            _scheduler?.CancelAll();
        }
    }
}