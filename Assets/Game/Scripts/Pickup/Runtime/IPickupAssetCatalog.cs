namespace Game.Pickup
{
    public interface IPickupAssetCatalog
    {
        bool TryGet(
            string id,
            out PickupDefinition definition);
    }
}