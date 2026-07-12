namespace Game.Item
{
    public interface IItemAssetCatalog
    {
        bool TryGet(
            string definitionId,
            out ItemDefinition definition);
    }
}