namespace Game.Actor
{
    public interface IActorIdentity
    {
        string InstanceId { get; }
        string DefinitionId { get; }

        void Initialize(
            string instanceId,
            string definitionId);
    }
}