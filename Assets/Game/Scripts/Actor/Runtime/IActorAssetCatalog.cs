namespace Game.Actor
{
    public interface IActorAssetCatalog
    {
        bool TryGet(
            string definitionId,
            out ActorDefinition definition);
    }
}