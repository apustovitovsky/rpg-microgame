using System.Threading;
using Cysharp.Threading.Tasks;
using Game.CommandSystem;
using UnityEngine;

namespace Game.Actor
{
    public sealed class ActorCombat :
        MonoBehaviour,
        IActorCombatHandler
    {
        [SerializeField] private MonoBehaviour _handler;

        public UniTask<CommandStatus> AttackAsync(
            string targetActorId,
            CancellationToken cancellationToken)
        {
            if (_handler is not IActorCombatHandler handler)
            {
                return UniTask.FromResult(CommandStatus.HandlerNotFound);
            }

            return handler.AttackAsync(
                targetActorId,
                cancellationToken);
        }

        private void OnValidate()
        {
            if (_handler != null &&
                _handler is not IActorCombatHandler)
            {
                Debug.LogError(
                    $"{nameof(ActorCombat)} handler must implement {nameof(IActorCombatHandler)}.",
                    this);

                _handler = null;
            }
        }
    }
}