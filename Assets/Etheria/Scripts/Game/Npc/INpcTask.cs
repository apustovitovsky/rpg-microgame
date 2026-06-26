using System.Threading;
using Cysharp.Threading.Tasks;

namespace Etheria.Game.Npc
{
    public interface INpcTask
    {
        NpcTaskType Type { get; }
        NpcTaskStatus Status { get; }

        int Priority { get; }
        bool CanSuspend { get; }
        bool IsBlocking { get; }

        UniTask RunAsync(
            CancellationToken cancellationToken);
    }

    public enum NpcTaskType
    {
        Movement,
        Dialogue
    }
}