namespace Game.Actor
{
    public interface IActorDefinitionCatalog
    {
        bool TryGet(
            string definitionId,
            out ActorDefinition definition);
    }
}