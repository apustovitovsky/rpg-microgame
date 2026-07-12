namespace Game.Loot
{
    public interface ILootContainerAssetCatalog
    {
        bool TryGet(
            string id,
            out LootContainerDefinition definition);
    }
}