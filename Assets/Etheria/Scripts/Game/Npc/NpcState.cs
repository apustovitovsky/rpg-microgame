namespace Etheria.Game.Npc
{
    public sealed class NpcState : INpcState
    {
        public string CurrentNodeId { get; private set; } =
            string.Empty;

        public string TargetNodeId { get; private set; } =
            string.Empty;

        public string CurrentLocationId { get; private set; } =
            string.Empty;

        public bool CanTalk { get; private set; } =
            true;

        public bool IsFleeing { get; private set; }

        public bool IsDead { get; private set; }

        public bool IsAttachedToGraph =>
            !string.IsNullOrWhiteSpace(CurrentNodeId);

        public bool HasTarget =>
            !string.IsNullOrWhiteSpace(TargetNodeId);

        public void AttachToNode(string nodeId)
        {
            CurrentNodeId = Normalize(nodeId);
        }

        public void SetTarget(string nodeId)
        {
            TargetNodeId = Normalize(nodeId);
        }

        public void ClearTarget()
        {
            TargetNodeId = string.Empty;
        }

        public void MarkReached(string nodeId)
        {
            CurrentNodeId = Normalize(nodeId);

            if (CurrentNodeId == TargetNodeId)
                TargetNodeId = string.Empty;
        }

        public void SetLocation(string locationId)
        {
            CurrentLocationId = Normalize(locationId);
        }

        public void SetCanTalk(bool canTalk)
        {
            CanTalk = canTalk;
        }

        public void SetFleeing(bool isFleeing)
        {
            IsFleeing = isFleeing;
        }

        public void SetDead(bool isDead)
        {
            IsDead = isDead;
        }

        public void Detach()
        {
            CurrentNodeId = string.Empty;
            TargetNodeId = string.Empty;
        }

        private static string Normalize(string value)
        {
            return value?.Trim() ?? string.Empty;
        }
    }
}