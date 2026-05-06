using Unity.Collections;

namespace Etheria.Features.NarrativeGeneration
{
    public static class RelationResolver
    {
        public static bool TryResolve(
            EntityId initiator,
            in WorldState worldState,
            NativeArray<RelationStep> relationNodes,
            out RelationContext context)
        {
            context = default;
            context.Initialize();

            // slot 0 is reserved for initiator
            context.SetSlot(0, initiator);

            for (int i = 0; i < relationNodes.Length; i++)
            {
                var node = relationNodes[i];
                var sourceEntityId = context.GetSlot(node.SourceSlot);

                if (!sourceEntityId.IsValid)
                {
                    return false;
                }

                if (!TryResolveNode(
                    sourceEntityId,
                    node,
                    worldState,
                    out var foundTarget))
                {
                    return false;
                }

                context.SetSlot(node.BindTargetSlot, foundTarget);
            }

            return true;
        }

        private static bool TryResolveNode(
            EntityId sourceEntityId,
            RelationStep node,
            in WorldState worldState,
            out EntityId foundTarget)
        {
            return node.Direction == RelationDirection.Outgoing
                ? TryResolveOutgoingNode(
                    sourceEntityId,
                    node,
                    worldState,
                    out foundTarget)
                : TryResolveIncomingNode(
                    sourceEntityId,
                    node,
                    worldState,
                    out foundTarget);
        }

        private static bool TryResolveOutgoingNode(
            EntityId sourceEntityId,
            RelationStep node,
            in WorldState worldState,
            out EntityId foundTarget)
        {
            foundTarget = EntityId.Invalid;
            var relationStore = worldState.RelationStore;

            if (!relationStore.TryGetFirstOutgoingRelation(sourceEntityId, out var relationId, out var iterator))
            {
                return false;
            }

            do
            {
                var relation = worldState.GetRelation(relationId);
                if (!TryMatchRelationTarget(node, worldState, relation, relation.Target, out foundTarget))
                {
                    continue;
                }

                return true;
            }
            while (relationStore.TryGetNextOutgoingRelation(out relationId, ref iterator));

            return false;
        }

        private static bool TryResolveIncomingNode(
            EntityId sourceEntityId,
            RelationStep node,
            in WorldState worldState,
            out EntityId foundTarget)
        {
            foundTarget = EntityId.Invalid;
            var relationStore = worldState.RelationStore;

            if (!relationStore.TryGetFirstIncomingRelation(sourceEntityId, out var relationId, out var iterator))
            {
                return false;
            }

            do
            {
                var relation = worldState.GetRelation(relationId);
                if (!TryMatchRelationTarget(node, worldState, relation, relation.Source, out foundTarget))
                {
                    continue;
                }

                return true;
            }
            while (relationStore.TryGetNextIncomingRelation(out relationId, ref iterator));

            return false;
        }

        private static bool TryMatchRelationTarget(
            RelationStep node,
            in WorldState worldState,
            RelationRecord relation,
            EntityId target,
            out EntityId foundTarget)
        {
            foundTarget = EntityId.Invalid;

            if (!node.RelationTagQuery.Matches(relation.RelationTags))
            {
                return false;
            }

            if (!target.IsValid)
            {
                return false;
            }

            if (!node.TargetEntityTagQuery.Matches(worldState.GetEntity(target.Value).EffectiveTags))
            {
                return false;
            }

            foundTarget = target;
            return true;
        }
    }
}
