using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Actor;

namespace Game.Navigation
{
    public interface INavigationPathFollower
    {
        UniTask<NavigationPathFollowResult> FollowAsync(
            IActorNavigation navigation,
            string startNodeId,
            string targetNodeId,
            CancellationToken cancellationToken);

        UniTask<NavigationPathFollowResult> FollowAsync(
            IActorNavigation navigation,
            string startLocationId,
            string startAnchorKey,
            string targetLocationId,
            string targetAnchorKey,
            CancellationToken cancellationToken);
    }

    public enum NavigationPathFollowResult
    {
        Completed,
        InvalidRequest,
        StartNodeNotFound,
        TargetNodeNotFound,
        StartAnchorNotFound,
        TargetAnchorNotFound,
        PathNotFound
    }
}