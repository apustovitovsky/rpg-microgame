using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Game.Npc;
using Etheria.Game.World;

namespace Etheria.Npc
{
    public sealed class NpcRouteFollower : INpcRouteFollower
    {
        private readonly INavigationGraphProvider _graphProvider;
        private readonly INpcMovementService _movement;
        private readonly INpcState _state;

        public NpcRouteFollower(
            INavigationGraphProvider graphProvider,
            INpcMovementService movement,
            INpcState state)
        {
            _graphProvider = graphProvider;
            _movement = movement;
            _state = state;
        }

        public async UniTask FollowPathAsync(
            NavigationPath path,
            CancellationToken cancellationToken)
        {
            if (path == null ||
                path.IsEmpty ||
                _graphProvider?.Graph == null ||
                _movement == null ||
                _state == null)
                return;

            var graph = _graphProvider.Graph;

            for (var i = 0; i < path.NodeIds.Count; i++)
            {
                var nodeId = path.NodeIds[i];

                if (!graph.TryGetNode(nodeId, out var node))
                    return;

                var isFinalNode =
                    i == path.NodeIds.Count - 1;

                _state.SetTarget(node.Id);

                if (isFinalNode)
                {
                    await _movement.MoveToCenterAsync(
                        node.Position,
                        cancellationToken);

                    await _movement.TurnToAsync(
                        node.Rotation,
                        cancellationToken);
                }
                else
                {
                    await _movement.MoveToAsync(
                        node.Position,
                        node.Radius,
                        cancellationToken);
                }

                _state.MarkReached(node.Id);
            }

            _state.ClearTarget();
        }
    }
}