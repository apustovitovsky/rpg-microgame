using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Game.World;

namespace Etheria.Game.Npc
{
    public interface INpcRouteFollower
    {
        UniTask FollowPathAsync(
            NavigationPath path,
            CancellationToken cancellationToken);
    }
}