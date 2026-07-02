using System.Threading;
using Cysharp.Threading.Tasks;
using Game.CommandSystem;

namespace Game.Actor
{
    public interface IActorCombatHandler
    {
        UniTask<CommandStatus> AttackAsync(
            string targetActorId,
            CancellationToken cancellationToken);
    }
}