namespace Game.Inventory
{
    public interface IItemDefinitionCatalog
    {
        bool TryGet(
            string definitionId,
            out ItemDefinition definition);
    }
}