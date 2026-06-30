using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Game.Commands;

namespace Etheria.Game.Npc
{
    public interface INpcDialogueStarter
    {
        bool CanStartDialogue { get; }

        UniTask<ActorCommandResult> StartDialogueAsync(
            CancellationToken cancellationToken);

        void Clear();
    }
}