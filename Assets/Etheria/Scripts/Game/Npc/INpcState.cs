namespace Etheria.Game.Npc
{
    public interface INpcState
    {
        string CurrentNodeId { get; }
        string TargetNodeId { get; }

        string CurrentLocationId { get; }

        bool CanTalk { get; }
        bool IsFleeing { get; }
        bool IsDead { get; }

        bool IsAttachedToGraph { get; }
        bool HasTarget { get; }

        void AttachToNode(string nodeId);
        void SetTarget(string nodeId);
        void ClearTarget();
        void MarkReached(string nodeId);

        void SetLocation(string locationId);
        void SetCanTalk(bool canTalk);
        void SetFleeing(bool isFleeing);
        void SetDead(bool isDead);

        void Detach();
    }
}